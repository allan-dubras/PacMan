using UnityEngine;

namespace FixedEngine
{
    /// <summary>
    /// Composant logique pour positionner un objet en 3D, basé sur des coordonnées fixes.
    /// 1 unité Unity = 1 pixel. Aucun mouvement automatique.
    /// </summary>
    public class FixedMover3D<TFormat>
        where TFormat : struct, IFixedPointFormat
    {
        public FixedTransform3D<TFormat> Transform { get; set; }

        /// <summary>
        /// Initialise la position fixe à partir d’un Transform Unity (snappé au pixel, arrondi bas).
        /// </summary>
        public void SnapFromUnityPosition(Vector3 unityPosition)
        {
            var fx = FixedMath.Floor(FixedPoint<TFormat>.FromFloat(unityPosition.x));
            var fy = FixedMath.Floor(FixedPoint<TFormat>.FromFloat(unityPosition.y));
            var fz = FixedMath.Floor(FixedPoint<TFormat>.FromFloat(unityPosition.z));
            Transform.LocalPosition = new FixedVector3<TFormat>(fx, fy, fz);
        }

        /// <summary>
        /// Applique la position logique au Transform Unity, arrondie au pixel (floor).
        /// </summary>
        public void ApplyToUnityTransform(Transform target)
        {
            var pos = Transform.LocalPosition;
            target.position = new Vector3(
                FixedMath.Floor(pos.x).ToFloat(),
                FixedMath.Floor(pos.y).ToFloat(),
                FixedMath.Floor(pos.z).ToFloat()
            );
        }

        public FixedVector3<TFormat> Position3D
        {
            get => Transform.LocalPosition;
            set => Transform.LocalPosition = value;
        }

        public void Move(FixedVector3<TFormat> delta)
        {
            Position3D = Position3D + delta;
        }

        public void SetPosition(FixedVector3<TFormat> position)
        {
            Position3D = position;
        }
    }
}
