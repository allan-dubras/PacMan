using System;
using UnityEngine;

namespace FixedEngine
{
    /// <summary>
    /// Quaternion fixe pour calculs déterministes.
    /// </summary>
    public struct FixedQuaternion<TFormat>
        where TFormat : struct, IFixedPointFormat
    {
        public FixedPoint<TFormat> x, y, z, w;

        public static readonly FixedQuaternion<TFormat> Identity =
            new FixedQuaternion<TFormat>(
                FixedMath.Zero<TFormat>(),
                FixedMath.Zero<TFormat>(),
                FixedMath.Zero<TFormat>(),
                FixedMath.One<TFormat>()
            );

        public FixedQuaternion(FixedPoint<TFormat> x, FixedPoint<TFormat> y, FixedPoint<TFormat> z, FixedPoint<TFormat> w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        /// <summary>
        /// Construit un quaternion à partir d'angles Euler (fixes) en radians.
        /// </summary>
        public static FixedQuaternion<TFormat> FromEuler(FixedVector3<TFormat> euler)
        {
            // demi-angle en fixe
            var halfEuler = euler * FixedMath.Half<TFormat>();

            // cos/sin en fixe
            var cx = FixedMath.Cos(halfEuler.x);
            var cy = FixedMath.Cos(halfEuler.y);
            var cz = FixedMath.Cos(halfEuler.z);
            var sx = FixedMath.Sin(halfEuler.x);
            var sy = FixedMath.Sin(halfEuler.y);
            var sz = FixedMath.Sin(halfEuler.z);

            // q = qz * qy * qx (ordre Z, Y, X)
            var qx = new FixedQuaternion<TFormat>(
                sx, FixedMath.Zero<TFormat>(), FixedMath.Zero<TFormat>(), cx);
            var qy = new FixedQuaternion<TFormat>(
                FixedMath.Zero<TFormat>(), sy, FixedMath.Zero<TFormat>(), cy);
            var qz = new FixedQuaternion<TFormat>(
                FixedMath.Zero<TFormat>(), FixedMath.Zero<TFormat>(), sz, cz);

            return qz * qy * qx;
        }

        /// <summary>
        /// Multiplication quaternion × quaternion.
        /// </summary>
        public static FixedQuaternion<TFormat> operator *(FixedQuaternion<TFormat> a, FixedQuaternion<TFormat> b)
        {
            return new FixedQuaternion<TFormat>(
                a.w * b.x + a.x * b.w + a.y * b.z - a.z * b.y,
                a.w * b.y - a.x * b.z + a.y * b.w + a.z * b.x,
                a.w * b.z + a.x * b.y - a.y * b.x + a.z * b.w,
                a.w * b.w - a.x * b.x - a.y * b.y - a.z * b.z
            );
        }

        /// <summary>
        /// Conjugé (inverse si quaternion unitaire).
        /// </summary>
        public FixedQuaternion<TFormat> Inverse()
        {
            return new FixedQuaternion<TFormat>(
                -x, -y, -z, w
            );
        }

        /// <summary>
        /// Applique la rotation à un vecteur.
        /// </summary>
        public static FixedVector3<TFormat> operator *(FixedQuaternion<TFormat> q, FixedVector3<TFormat> v)
        {
            // uv = q.xyz × v
            var uv = Cross(q, v);
            // uuv = q.xyz × uv
            var uuv = Cross(q, uv);

            // uv *= 2 * q.w (toutes opérations fixes)
            uv *= (FixedMath.One<TFormat>() * 2) * q.w;
            // uuv *= 2
            uuv *= FixedMath.One<TFormat>() * 2;

            return v + uv + uuv;
        }

        private static FixedVector3<TFormat> Cross(FixedQuaternion<TFormat> q, FixedVector3<TFormat> v)
        {
            return new FixedVector3<TFormat>(
                q.y * v.z - q.z * v.y,
                q.z * v.x - q.x * v.z,
                q.x * v.y - q.y * v.x
            );
        }

        /// <summary>
        /// Convertit ce quaternion fixe en Quaternion Unity.
        /// </summary>
        public Quaternion ToQuaternion()
        {
            return new Quaternion(
                x.ToFloat(),
                y.ToFloat(),
                z.ToFloat(),
                w.ToFloat()
            );
        }
    }
}
