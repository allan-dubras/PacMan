using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using FixedEngine;

public class PacManController : MonoBehaviour, IFixedTick
{
    [SerializeField] private InputActionAsset inputActionsAsset; // Référence au système d'input
    [SerializeField] private Tilemap collisionMap; // Tilemap utilisée pour détecter les collisions
    [SerializeField] private int tileSize = 8; // Taille d’une tuile en unités
    [SerializeField] private LogicGrid grid; // Grille logique contenant les infos sur les cellules (walkable ou non)

    [SerializeField] private int speedRaw = 256; // Vitesse exprimée en format fixe brut (Q8_8)
    private FixedPoint<Q8_8> _speed; // Vitesse convertie en FixedPoint
    private FixedTransform2D<Q8_8> _xf; // Représentation fixe de la position
    private FixedMover2D<Q8_8> _mover; // Objet moteur pour déplacer l’entité en coordonnées fixes

    private InputAction _moveAction; // Action d’input pour le déplacement

    private FixedVector2<Q8_8> _desiredDir = new FixedVector2<Q8_8>(); // Direction souhaitée par le joueur
    private FixedVector2<Q8_8> _currentDir = new FixedVector2<Q8_8>(); // Direction actuelle utilisée pour le déplacement
    private bool _isPreTurning; // Indique si un virage anticipé est en cours

    private Vector3Int _nextCell; // Cellule vers laquelle Pac-Man va se diriger

    private FixedPoint<Q8_8> _turnTargetX; // Cible X pour un virage anticipé
    private FixedPoint<Q8_8> _turnTargetY; // Cible Y pour un virage anticipé
    public FixedVector2<Q8_8> CurrentDirection => _currentDir;

    public FixedVector2<Q8_8> DesiredDir { get => _desiredDir; set => _desiredDir = value; }
    public FixedVector2<Q8_8> CurrentDir { get => _currentDir; set => _currentDir = value; }

    public void Awake()
    {
        // Input : récupération de la map et de l’action Move
        var map = inputActionsAsset.FindActionMap("Player", true);
        _moveAction = map.FindAction("Move", true);
        _moveAction.Enable();

        // Initialisation du moteur physique fixe
        _speed = new FixedPoint<Q8_8>(speedRaw);
        _xf = new FixedTransform2D<Q8_8>();
        _mover = new FixedMover2D<Q8_8> { Transform = _xf };

        // Alignement initial sur une cellule de grille
        _mover.SnapToCellFromUnity(transform.position, tileSize, grid.origin);

        // Calcul de la cellule de départ
        Vector3Int startCell = collisionMap.WorldToCell(transform.position);

        // Récupère la cellule logique correspondante
        var logic = grid.GetCell(startCell);

        // S’enregistre pour recevoir les FixedTicks via TickManager
        TickManager.Instance.Register(this);
    }

    public void OnDestroy()
    {
        // Se désinscrit du TickManager à la destruction
        TickManager.Instance.Unregister(this);
        _moveAction.Disable(); // Désactive l’input
    }

    public void FixedTick()
    {
        // Lecture de l’input analogique et quantification en directions fixes (haut, bas, gauche, droite)
        Vector2 rawInput = _moveAction.ReadValue<Vector2>();
        var inputDir = FixedInputQuantizer.QuantizeVector2<Q8_8>(rawInput);

        // On ignore les diagonales
        if ((inputDir.x.Raw != 0 && inputDir.y.Raw == 0) || (inputDir.y.Raw != 0 && inputDir.x.Raw == 0))
        {
            if (!_isPreTurning)
            {
                _desiredDir = inputDir; // Mémorise la direction souhaitée
            }
            else
            {
                // En pré-virage, on ignore les directions identiques ou opposées
                if (inputDir != _currentDir && inputDir != -_currentDir)
                {
                    _desiredDir = inputDir;
                }
            }
        }

        // Récupère la position actuelle, la cellule et le centre de la cellule
        FixedVector2<Q8_8> pos = _mover.Position2D;
        var worldPos = new Vector3(pos.x.ToFloat(), pos.y.ToFloat(), transform.position.z);
        Vector3Int cell = collisionMap.WorldToCell(worldPos);
        FixedVector2<Q8_8> center = GetFixedCenter(cell);

        // Si à l’arrêt, démarre le mouvement dans la direction souhaitée (si valide)
        if (_currentDir == FixedVector2<Q8_8>.Zero && _desiredDir != FixedVector2<Q8_8>.Zero && CanMove(cell, _desiredDir))
        {
            _currentDir = _desiredDir;
        }

        // Demi-tour autorisé à tout moment
        if (_desiredDir == -_currentDir && CanMove(cell, _desiredDir))
        {
            _currentDir = _desiredDir;
        }

        // Détection du pré-virage
        if (!_isPreTurning && _currentDir != FixedVector2<Q8_8>.Zero && _desiredDir != _currentDir && _desiredDir != -_currentDir && CanMove(cell, _desiredDir))
        {
            var nCell = cell + new Vector3Int(FixedMath.Sign(_desiredDir.x), FixedMath.Sign(_desiredDir.y), 0);

            // Vérifie que la cellule cible est navigable
            var logic = grid.GetCell(nCell);
            if (logic == null || !logic.isWalkable)
            {
                goto skipPreTurn; // Ignore si obstacle
            }

            // Calcule les coordonnées du centre de la cellule de virage
            var nextCenter = GetFixedCenter(nCell);

            // Calcule le point d’intersection pour tourner
            if (_currentDir.x.Raw != 0)
            {
                _turnTargetX = center.x;
                _turnTargetY = nextCenter.y;
            }
            else
            {
                _turnTargetX = nextCenter.x;
                _turnTargetY = center.y;
            }

            _isPreTurning = true; // Active le mode pré-virage
        }

    skipPreTurn:

        // Exécution du pré-virage si actif
        if (_isPreTurning)
        {
            FixedVector2<Q8_8> curPos = _mover.Position2D;
            var dx = _turnTargetX - curPos.x;
            var dy = _turnTargetY - curPos.y;

            FixedPoint<Q8_8> moveX = new FixedPoint<Q8_8>(0);
            FixedPoint<Q8_8> moveY = new FixedPoint<Q8_8>(0);

            // Mouvement progressif jusqu’au point de virage
            if (dx.Raw != 0)
            {
                moveX = (FixedMath.Abs(dx).Raw <= _speed.Raw) ? dx : FixedMath.Sign(dx) * _speed;
            }

            if (dy.Raw != 0)
            {
                moveY = (FixedMath.Abs(dy).Raw <= _speed.Raw) ? dy : FixedMath.Sign(dy) * _speed;
            }

            _mover.Move(new FixedVector2<Q8_8>(moveX, moveY));

            // Arrivé au point de virage → changement de direction
            if (dx.Raw == 0 && dy.Raw == 0)
            {
                _isPreTurning = false;
                _currentDir = _desiredDir;
            }
            return; // Stoppe ici pour ne pas doubler le mouvement
        }

        // Déplacement normal ou recentrage si obstacle
        if (_currentDir != FixedVector2<Q8_8>.Zero)
        {
            if (CanMove(cell, _currentDir))
            {
                safeMove(_currentDir); // Déplacement direct
            }
            else
            {
                // Recentrage progressif sur la cellule actuelle
                var deltaX = center.x - pos.x;
                var deltaY = center.y - pos.y;

                var moveX = (FixedMath.Abs(deltaX).Raw <= _speed.Raw) ? deltaX : FixedMath.Sign(deltaX) * _speed;
                var moveY = (FixedMath.Abs(deltaY).Raw <= _speed.Raw) ? deltaY : FixedMath.Sign(deltaY) * _speed;

                _mover.Move(new FixedVector2<Q8_8>(moveX, moveY));

                // Si recentré, stoppe le mouvement
                if (deltaX.Raw == moveX.Raw && deltaY.Raw == moveY.Raw)
                {
                    _currentDir = FixedVector2<Q8_8>.Zero;
                }
            }
        }
    }

    public void safeMove(FixedVector2<Q8_8> direction)
    {
        /*const int subSteps = 4;
       var step = _speed / FixedPoint<Q8_8>.FromInt(subSteps);
       var start = _mover.Position2D;

       //1) Cellule d'origine
       Vector3 worldStart = new Vector3(start.x.ToFloat(), start.y.ToFloat(), 0f);
       Vector3Int originCell = collisionMap.WorldToCell(worldStart);

       for (int i = 1; i <= subSteps; i++)
       {
           var offset = direction * (step * FixedPoint<Q8_8>.FromInt(i));
           var nextPos = start + offset;
           Vector3 worldPos = new Vector3(nextPos.x.ToFloat(), nextPos.y.ToFloat(), 0f);
           Vector3Int cell = collisionMap.WorldToCell(worldPos);
           var logic = grid.GetCell(cell);
           bool walkable = (logic == null) || logic.isWalkable;
       }*/

        // Déplacement simple (sous-étapes commentées si besoin d’ajouter une précision)
        _mover.Move(_currentDir * _speed);
    }

    public bool CanMove(Vector3Int cell, FixedVector2<Q8_8> dir)
    {
        // Vérifie la cellule en face dans la direction donnée
        int dx = dir.x.Raw > 0 ? 1 : dir.x.Raw < 0 ? -1 : 0;
        int dy = dir.y.Raw > 0 ? 1 : dir.y.Raw < 0 ? -1 : 0;

        _nextCell = new Vector3Int(cell.x + dx, cell.y + dy, 0);
        var logic = grid.GetCell(_nextCell);
        return (logic != null && logic.isWalkable) || logic == null;
    }

    public FixedVector2<Q8_8> GetFixedCenter(Vector3Int cell)
    {
        // Renvoie le centre d’une cellule sous forme de FixedVector2
        Vector3 worldCenter = collisionMap.GetCellCenterWorld(cell);
        return new FixedVector2<Q8_8>(worldCenter.x, worldCenter.y);
    }

    public void LateUpdate()
    {
        // Applique la position calculée en FixedEngine à l'objet Unity (affichage)
        _xf.ApplyToTransform(transform);
    }
}
