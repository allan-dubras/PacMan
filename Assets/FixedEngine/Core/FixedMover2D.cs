using UnityEngine;

namespace FixedEngine
{
    /// <summary>
    /// Composant logique pour positionner un objet en 2D, basé sur des coordonnées fixes.
    /// 1 unité Unity = 1 pixel. Aucun mouvement automatique.
    /// </summary>
    public class FixedMover2D<TFormat>
        where TFormat : struct, IFixedPointFormat
    {
        public FixedTransform2D<TFormat> Transform { get; set; }

        /// <summary>
        /// Initialise la position fixe à partir d’un Transform Unity (snappé au pixel).
        /// </summary>
        public void SnapFromUnityPosition(Vector3 unityPosition)
        {
            var fx = FixedMath.Floor(FixedPoint<TFormat>.FromFloat(unityPosition.x));
            var fy = FixedMath.Floor(FixedPoint<TFormat>.FromFloat(unityPosition.y));
            Transform.LocalPosition = new FixedVector3<TFormat>(fx, fy, FixedMath.Zero<TFormat>());
        }

        public void SnapToCellFromUnity(Vector3 unityPos, int tileSizeUnits, Vector2Int tilemapOrigin)
        {
            // 1) On détermine la cellule Unity
            int cellX = Mathf.FloorToInt(unityPos.x / tileSizeUnits);
            int cellY = Mathf.FloorToInt(unityPos.y / tileSizeUnits);

            // 2) On récupère le « raw » par unité fixe via FixedMath
            int rawPerUnit = FixedMath.One<TFormat>().Raw;

            // 3) Taille d’une tuile en raw
            int rawPerTile = tileSizeUnits * rawPerUnit;

            // 4) Décalage au centre : moitié de la tuile
            int centerOffset = (tileSizeUnits / 2) * rawPerUnit;

            // 5) Calcul de la position brute finale en raw
            int rawX = cellX * rawPerTile + centerOffset;
            int rawY = cellY * rawPerTile + centerOffset;

            // 6) On positionne le FixedTransform
            Transform.LocalPosition = new FixedVector3<TFormat>(
                new FixedPoint<TFormat>(rawX),
                new FixedPoint<TFormat>(rawY),
                FixedMath.Zero<TFormat>()       // on reste à 0 en Z
            );
        }

        /// <summary>
        /// Applique la position logique au Transform Unity, arrondie au pixel.
        /// </summary>
        public void ApplyToUnityTransform(Transform target)
        {
            var pos = Transform.LocalPosition;
            target.position = new Vector3(
                FixedMath.Floor(pos.x).ToFloat(),
                FixedMath.Floor(pos.y).ToFloat(),
                target.position.z
            );
        }

        public FixedVector2<TFormat> Position2D
        {
            get => new FixedVector2<TFormat>(Transform.LocalPosition.x, Transform.LocalPosition.y);
            set => Transform.LocalPosition = new FixedVector3<TFormat>(value.x, value.y, FixedMath.Zero<TFormat>());
        }

        public void Move(FixedVector2<TFormat> delta)
        {
            Position2D = Position2D + delta;
        }

        public void SetPosition(FixedVector2<TFormat> position)
        {
            Position2D = position;
        }
    }
}
