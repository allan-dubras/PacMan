using UnityEngine;

namespace FixedEngine
{
    /// <summary>
    /// Spécialisation 2D de FixedTransform : gère le ForwardVector en 2D et l'application vers UnityEngine.Transform en 2D.
    /// </summary>
    public class FixedTransform2D<TFormat> : FixedTransform<TFormat>
        where TFormat : struct, IFixedPointFormat
    {
        /// <summary>
        /// Vecteur avant unitaire en 2D (X,Y), obtenu en appliquant la rotation locale autour de l'axe Z au vecteur (1,0,0).
        /// </summary>
        public FixedVector2<TFormat> ForwardVector
        {
            get
            {
                var f3 = LocalRotation * new FixedVector3<TFormat>(
                    FixedMath.One<TFormat>(),
                    FixedMath.Zero<TFormat>(),
                    FixedMath.Zero<TFormat>()
                );
                return new FixedVector2<TFormat>(f3.x, f3.y);
            }
        }

        /// <summary>
        /// Vecteur droit unitaire en 2D, perpendiculaire à ForwardVector.
        /// </summary>
        public FixedVector2<TFormat> RightVector
        {
            get
            {
                // Pour pi/2, on multiplie par 0.5 fixe plutôt que d'utiliser FromRaw
                var halfPi = FixedMath.PI<TFormat>() * FixedMath.Half<TFormat>();
                var rQuat = FixedQuaternion<TFormat>.FromEuler(
                    new FixedVector3<TFormat>(
                        FixedMath.Zero<TFormat>(),
                        FixedMath.Zero<TFormat>(),
                        halfPi
                    )
                );
                var right3 = LocalRotation * rQuat * new FixedVector3<TFormat>(
                    FixedMath.One<TFormat>(),
                    FixedMath.Zero<TFormat>(),
                    FixedMath.Zero<TFormat>()
                );
                return new FixedVector2<TFormat>(right3.x, right3.y);
            }
        }

        /// <summary>
        /// Applique ce FixedTransform2D à un Transform Unity en 2D (XY position, Z rotation, XY scale).
        /// </summary>
        public void ApplyToTransform2D(Transform target)
        {
            // Position monde
            var wp = WorldPosition;
            target.position = new Vector3(
                wp.x.ToFloat(),
                wp.y.ToFloat(),
                target.position.z
            );

            // Rotation Z en degrés
            float angleZ = LocalRotation.ToQuaternion().eulerAngles.z;
            target.rotation = Quaternion.Euler(0f, 0f, angleZ);

            // Échelle XY
            target.localScale = new Vector3(
                LocalScale.x.ToFloat(),
                LocalScale.y.ToFloat(),
                target.localScale.z
            );
        }
    }
}
