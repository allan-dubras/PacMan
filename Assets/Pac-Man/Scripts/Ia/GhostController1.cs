using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using FixedEngine;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Ny
{
    public abstract class GhostController1 : MonoBehaviour, IFixedTick
    {
        [SerializeField] protected Tilemap CollisionMap;                                            // TileMap utiliser pour la détection
        [SerializeField] protected int TileSize = 8;                                               // Taille d'une tuille
        [SerializeField] protected LogicGrid Grid;

        [SerializeField] protected int speedRaw = 256;
        private FixedPoint<Q8_8> _speed;                                                     // Vitesse convertie en valeur à virgule fixe
        private FixedTransform2D<Q8_8> _xf;                                                 // Représente la position transformée en 2D fixe.
        private FixedMover2D<Q8_8> _mover;

        private FixedVector2<Q8_8> _desiredDir = new FixedVector2<Q8_8>();           // Direction vers laquelle le joueur veut se diriger
        private FixedVector2<Q8_8> _currentDir = new FixedVector2<Q8_8>();
        private FixedVector2<Q8_8> _lastDir = new FixedVector2<Q8_8>();
        private Vector3Int _nextCell;                                           // Cellule cible pour le virage

        protected Vector3Int _targetCell;
        [SerializeField] protected PacManController pacman;



        protected void Awake()
        {
            // Init FixedEngine
            _speed = new FixedPoint<Q8_8>(speedRaw);                               //
            _xf = new FixedTransform2D<Q8_8>();                                   // Initialise la vitesse, la position et le systeme de mouvement en coordonnées fixes.
            _mover = new FixedMover2D<Q8_8> { Transform = _xf };                 //

            _mover.SnapToCellFromUnity(transform.position, TileSize, Grid.origin);    // Aligne Pac-Man sur la cellule la plus proche.


            Vector3Int startCell = CollisionMap.WorldToCell(transform.position);  // Calcule la cellule actuelle de Pac-Man et obtient ses données logiques (walkable, etc)
            var logic = Grid.GetCell(startCell);                                 //

            TickManager.Instance.Register(this);   // Enregistre l'objet dans un gestionnaire de mise à jour fixe.
        }

        void OnDestroy()
        {
            TickManager.Instance.Unregister(this); // Nettoyage : désactive l'input et se désinscrit du TickManager

        }

        public void FixedTick()
        {
            // 1) Position courante, cellule et centre
            FixedVector2<Q8_8> pos = _mover.Position2D;                                           /// 
            var worldPos = new Vector3(pos.x.ToFloat(), pos.y.ToFloat(), transform.position.z);   /// Position actuelle de Pac-Man,
            Vector3Int cell = CollisionMap.WorldToCell(worldPos);                                 /// convertie en cellule logigque et en coordonnées centrées.
            FixedVector2<Q8_8> center = GetFixedCenter(cell);

            // 2) Détermine intersection
            int walkableCount = 0;

            foreach (var adj in new[] { Vector3Int.up, Vector3Int.left, Vector3Int.down, Vector3Int.right })
            {
                var logic = Grid.GetCell(cell + adj);
                if (logic == null || logic.isGhostWalkable)
                {
                    walkableCount++;
                }

            }
            bool atIntersection = walkableCount > 2;
            // 3) Trouver nouvelle cellule cible
            if (pos == center)
            {
                _targetCell = CalculateTargetCell(cell);
            }

            // 4) Choisir de la direction
            if (pos == center && atIntersection)
            {

                _desiredDir = BestDirection(cell,_lastDir, _targetCell);

                if (_desiredDir != FixedVector2<Q8_8>.Zero)
                {
                    _currentDir = _desiredDir;
                }
            }




            // 7) Protège contre le fait de traverser le murs, sinon déplacement normal.
            // Si Pac-Man peut continuer dans sa direction, il avance.
            // Sinon, il se recentre sur la cellule actuelle (empeche de passer à travers un mur).
            if (_currentDir != FixedVector2<Q8_8>.Zero)
            {

                if (CanMove(cell, _currentDir))
                {
                    SafeMode(_currentDir);
                }
                else
                {
                    var delta = center - pos;
                    bool CloseX = FixedMath.Abs(delta.x).Raw <= _speed.Raw;
                    bool CloseY = FixedMath.Abs(delta.y).Raw <= _speed.Raw;

                    var ideal = _currentDir * _speed;

                    var moveX = FixedMath.Abs(ideal.x) > FixedMath.Abs(delta.x)
                        ? delta.x
                        : ideal.x;
                    var moveY = FixedMath.Abs(ideal.y) > FixedMath.Abs(delta.y)
                        ? delta.y
                        : ideal.y;

                    _mover.Move(new FixedVector2<Q8_8>(moveX, moveY));

                    if (CloseX && CloseY)
                    {
                        var newDir = BestDirection(cell, _currentDir, _targetCell);
                        if (newDir != FixedVector2<Q8_8>.Zero)
                        {
                            _currentDir = newDir;
                        }
                    }

                }
            }
        }
        private FixedVector2<Q8_8> BestDirection(Vector3Int currentCell, FixedVector2<Q8_8> previousDir, Vector3Int targetCell)
        {
            FixedVector2<Q8_8> bestDir = new FixedVector2<Q8_8>();
            int bestDist2 = int.MaxValue;

            // Ordres de priorité
            var adjs = new[]
            {
                Vector3Int.up,
                Vector3Int.left,
                Vector3Int.down,
                Vector3Int.right,

            };

            var dirs = new[]
            {
                new FixedVector2<Q8_8>(FixedPoint<Q8_8>.Zero,FixedPoint<Q8_8>.One),
                new FixedVector2<Q8_8>(-FixedPoint<Q8_8>.One,FixedPoint<Q8_8>.Zero),
                new FixedVector2<Q8_8>(FixedPoint<Q8_8>.Zero,-FixedPoint<Q8_8>.One),
                new FixedVector2<Q8_8>(FixedPoint<Q8_8>.One,FixedPoint<Q8_8>.Zero),

            };

            for (int i = 0; i < dirs.Length; i++)
            {
                var adj = adjs[i];
                var dir = dirs[i];

                // a) éliminer le demi-tour
                if (dir == -previousDir)
                {
                    continue;
                }

                // b) 
                if (!CanMove(currentCell, dir))
                {
                    continue;
                }

                // c)
                var voisin = currentCell + adj;
                int dx = voisin.x - targetCell.x;
                int dy = voisin.y - targetCell.y;
                int dist2 = dx * dx + dy * dy;
                if (bestDir == FixedVector2<Q8_8>.Zero || dist2 < bestDist2)
                {
                    bestDir = dir;
                    bestDist2 = dist2;
                }
            }

           

            return bestDir;
        }

        protected abstract Vector3Int CalculateTargetCell(Vector3Int cell);

        public bool CanMove(Vector3Int cell, FixedVector2<Q8_8> dir)
        {
            int dx = dir.x.Raw > 0 ? 1 : dir.x.Raw < 0 ? -1 : 0;
            int dy = dir.y.Raw > 0 ? 1 : dir.y.Raw < 0 ? -1 : 0;

            _nextCell = new Vector3Int(cell.x + dx, cell.y + dy, 0);
            var logic = Grid.GetCell(_nextCell);
            return (logic != null && logic.isGhostWalkable) || logic == null;
        }

        /// <summary>
        /// Calcule les coordonnées fixes du centre d'une cellule de la tileMap
        /// </summary>
        private FixedVector2<Q8_8> GetFixedCenter(Vector3Int cell)
        {
            Vector3 worldCenter = CollisionMap.GetCellCenterWorld(cell);
            return new FixedVector2<Q8_8>(worldCenter.x, worldCenter.y);

        }
        private void SafeMode(FixedVector2<Q8_8> direction)
        {
            #region Resoudre bug
            /*const int subSteps = 4;
            var step = _speed / FixedPoint<Q8_8>.FromInt(subSteps);
            var start = _mover.Position2D;

            // 1) Cellule origine
            Vector3 worldStart = new Vector3(start.x.ToFloat(), start.y.ToFloat(), 0f);
            Vector3Int originCell = CollisionMap.WorldToCell(worldStart);

            for (int i = 1; i <= subSteps; i++)
            {
                var offset = direction * (step * FixedPoint<Q8_8>.FromInt(i));
                var nextPos = start + offset;
                Vector3 worldPos = new Vector3(nextPos.x.ToFloat(), nextPos.y.ToFloat(), 0f);
                Vector3Int cell = CollisionMap.WorldToCell(worldPos);
                var logic = Grid.GetCell(cell);
                bool walkable = (logic == null) || logic.isWalkable;
            }*/
            #endregion

            _mover.Move(_currentDir * _speed);  // Déplacement simple d'un pas dans la direction actuelle
            _lastDir = direction;
        }

        /// <summary>
        /// Met à jour le Transform Unity à partir de la position fixe _xf.
        /// </summary>
        private void LateUpdate()
        {
            _xf.ApplyToTransform(transform);

        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || CollisionMap == null)
                return;

            Vector3 worldCenter = CollisionMap.GetCellCenterWorld(_targetCell);
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube(worldCenter, CollisionMap.cellSize);



        }
    }

}
