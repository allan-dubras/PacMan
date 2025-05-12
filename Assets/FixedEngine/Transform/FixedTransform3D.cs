using UnityEngine;

namespace FixedEngine
{
    /// <summary>
    /// Spécialisation 3D de FixedTransform : gère le ForwardVector, RightVector et UpVector en 3D,
    /// et permet d'appliquer la transform fixe à un Transform Unity en 3D.
    /// </summary>
    public class FixedTransform3D<TFormat> : FixedTransform<TFormat>
        where TFormat : struct, IFixedPointFormat
    {
        /// <summary>
        /// Vecteur avant unitaire en 3D (X, Y, Z), rotation du vecteur (1,0,0).
        /// </summary>
        public FixedVector3<TFormat> ForwardVector
            => LocalRotation * new FixedVector3<TFormat>(
                   FixedMath.One<TFormat>(),
                   FixedMath.Zero<TFormat>(),
                   FixedMath.Zero<TFormat>()
               );

        /// <summary>
        /// Vecteur droit unitaire en 3D, rotation du vecteur (0,1,0).
        /// </summary>
        public FixedVector3<TFormat> RightVector
            => LocalRotation * new FixedVector3<TFormat>(
                   FixedMath.Zero<TFormat>(),
                   FixedMath.One<TFormat>(),
                   FixedMath.Zero<TFormat>()
               );

        /// <summary>
        /// Vecteur haut unitaire en 3D, rotation du vecteur (0,0,1).
        /// </summary>
        public FixedVector3<TFormat> UpVector
            => LocalRotation * new FixedVector3<TFormat>(
                   FixedMath.Zero<TFormat>(),
                   FixedMath.Zero<TFormat>(),
                   FixedMath.One<TFormat>()
               );

        /// <summary>
        /// Applique ce FixedTransform3D à un Transform Unity (position, rotation, scale).
        /// </summary>
        public void ApplyToTransform3D(Transform target)
        {
            // Position monde
            target.position = WorldPosition.ToVector3();
            // Rotation 3D
            target.rotation = LocalRotation.ToQuaternion();
            // Échelle locale
            target.localScale = LocalScale.ToVector3();
        }
    }
}
