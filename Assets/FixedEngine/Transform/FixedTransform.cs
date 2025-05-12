using UnityEngine;

namespace FixedEngine
{
    /// <summary>
    /// Socle commun à tout transform déterministe fixe.
    /// Gère position, rotation (quaternion fixe), scale, hiérarchie et matrices.
    /// Permet de séparer totalement la logique (FixedTransform) du rendu Unity (Transform).
    /// </summary>
    public class FixedTransform<TFormat>
        where TFormat : struct, IFixedPointFormat
    {
        /// <summary>Position locale par rapport au parent (ou monde si Parent==null).</summary>
        public FixedVector3<TFormat> LocalPosition { get; set; }

        /// <summary>Rotation locale (quaternion fixe).</summary>
        public FixedQuaternion<TFormat> LocalRotation { get; set; }

        /// <summary>Échelle locale.</summary>
        public FixedVector3<TFormat> LocalScale { get; set; } = FixedVector3<TFormat>.One;

        /// <summary>Parent dans la hiérarchie fixe.</summary>
        public FixedTransform<TFormat> Parent { get; set; }

        /// <summary>Position en espace monde.</summary>
        public FixedVector3<TFormat> WorldPosition
            => Parent == null
               ? LocalPosition
               : Parent.LocalToWorldMatrix.MultiplyPoint(LocalPosition);

        /// <summary>Matrice de transformation locale → monde.</summary>
        public FixedMatrix4x4<TFormat> LocalToWorldMatrix
            => ComputeLocalToWorld(LocalPosition, LocalRotation, LocalScale);

        /// <summary>Matrice de transformation monde → locale.</summary>
        public FixedMatrix4x4<TFormat> WorldToLocalMatrix
            => LocalToWorldMatrix.Inverse();

        /// <summary>
        /// Applique ce FixedTransform à un Transform Unity (position, rotation, scale).
        /// Nécessite :
        ///  - FixedVector3&lt;TFormat&gt;.ToVector3()
        ///  - FixedQuaternion&lt;TFormat&gt;.ToQuaternion()
        /// </summary>
        public void ApplyToTransform(Transform target)
        {
            target.localPosition = LocalPosition.ToVector3();
            target.localRotation = LocalRotation.ToQuaternion();
            target.localScale = LocalScale.ToVector3();
        }

        // --- SOCLE TRS ---
        private static FixedMatrix4x4<TFormat> ComputeLocalToWorld(
            FixedVector3<TFormat> pos,
            FixedQuaternion<TFormat> rot,
            FixedVector3<TFormat> scl)
        {
            var t = FixedMatrix4x4<TFormat>.Translate(pos);
            var r = FixedMatrix4x4<TFormat>.Rotate(rot);
            var s = FixedMatrix4x4<TFormat>.Scale(scl);
            return t * r * s;
        }
    }
}
