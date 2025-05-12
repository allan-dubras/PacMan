using System;
using System.Runtime.CompilerServices;

using UnityEngine;

namespace FixedEngine
{
    public static class FixedVectorMath
    {
        // ========== FIXEDVECTOR2 ==========

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint<TFormat> Dot<TFormat>(FixedVector2<TFormat> a, FixedVector2<TFormat> b) where TFormat : struct, IFixedPointFormat
        {
            return a.x * b.x + a.y * b.y;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint<TFormat> SqrMagnitude<TFormat>(FixedVector2<TFormat> v) where TFormat : struct, IFixedPointFormat
        {
            return v.x * v.x + v.y * v.y;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint<TFormat> Magnitude<TFormat>(FixedVector2<TFormat> v) where TFormat : struct, IFixedPointFormat
        {
            return FixedMath.Sqrt(SqrMagnitude(v));
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedVector2<TFormat> Normalize<TFormat>(FixedVector2<TFormat> v) where TFormat : struct, IFixedPointFormat
        {
            var mag = Magnitude(v);
            if (FixedMath.IsZero(mag))
                return FixedVector2<TFormat>.Zero;

            return new FixedVector2<TFormat>(v.x / mag, v.y / mag);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedVector2<TFormat> ClampMagnitude<TFormat>(FixedVector2<TFormat> v, FixedPoint<TFormat> max) where TFormat : struct, IFixedPointFormat
        {
            var sqrMag = SqrMagnitude(v);
            var maxSqr = max * max;

            if (sqrMag <= maxSqr)
                return v;

            return Normalize(v) * max;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint<T> Distance<T>(FixedVector2<T> a, FixedVector2<T> b) where T : struct, IFixedPointFormat
        {
            return Magnitude(a - b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint<TFormat> AngleDeg<TFormat>(FixedVector2<TFormat> from, FixedVector2<TFormat> to) where TFormat : struct, IFixedPointFormat
        {
            var angleFrom = FixedMath.Atan2(from.y, from.x);
            var angleTo = FixedMath.Atan2(to.y, to.x);

            var delta = angleTo - angleFrom;
            if (delta.Raw < 0)
                delta += FixedMath.Fixed360<TFormat>();

            return delta;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint<T> SignedAngleDeg<T>(FixedVector2<T> from, FixedVector2<T> to) where T : struct, IFixedPointFormat
        {
            var angleFrom = FixedMath.Atan2(from.y, from.x);
            var angleTo = FixedMath.Atan2(to.y, to.x);

            var delta = angleTo - angleFrom;

            var full = FixedMath.Fixed360<T>();
            var half = FixedMath.Fixed180<T>();

            if (delta > half)
                delta -= full;
            else if (delta < -half)
                delta += full;

            return delta;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedVector2<T> Reflect<T>(FixedVector2<T> inDirection, FixedVector2<T> normal) where T : struct, IFixedPointFormat
        {
            // On suppose que `normal` est normalisé
            var dot = Dot(inDirection, normal);
            var twiceProj = normal * (dot + dot);
            return inDirection - twiceProj;
        }


        public static FixedVector2<T> Sign<T>(FixedVector2<T> v) where T : struct, IFixedPointFormat
            => new FixedVector2<T>(Math.Sign(v.x.Raw), Math.Sign(v.y.Raw));

        public static FixedVector2<T> Inverse<T>(FixedVector2<T> v) where T : struct, IFixedPointFormat
            => new FixedVector2<T>(-v.x, -v.y);

        // ========== FIXEDVECTOR3 ==========

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint<T> Dot3D<T>(FixedVector3<T> a, FixedVector3<T> b) where T : struct, IFixedPointFormat
        {
            return a.x * b.x + a.y * b.y + a.z * b.z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint<T> SqrMagnitude3D<T>(FixedVector3<T> v) where T : struct, IFixedPointFormat
        {
            return v.x * v.x + v.y * v.y + v.z * v.z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint<T> Magnitude3D<T>(FixedVector3<T> v) where T : struct, IFixedPointFormat
        {
            return FixedMath.Sqrt(SqrMagnitude3D(v));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedVector3<T> Normalize3D<T>(FixedVector3<T> v) where T : struct, IFixedPointFormat
        {
            var mag = Magnitude3D(v);
            if (FixedMath.IsZero(mag))
                return FixedVector3<T>.Zero;

            return new FixedVector3<T>(
                v.x / mag,
                v.y / mag,
                v.z / mag
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedVector3<T> ClampMagnitude3D<T>(FixedVector3<T> v, FixedPoint<T> max) where T : struct, IFixedPointFormat
        {
            var sqrMag = SqrMagnitude3D(v);
            var maxSqr = max * max;

            if (sqrMag <= maxSqr)
                return v;

            return Normalize3D(v) * max;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint<T> Distance3D<T>(FixedVector3<T> a, FixedVector3<T> b) where T : struct, IFixedPointFormat
        {
            return Magnitude3D(a - b);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint<T> AngleDeg3D<T>(FixedVector3<T> a, FixedVector3<T> b) where T : struct, IFixedPointFormat
        {
            var magA = Magnitude3D(a);
            var magB = Magnitude3D(b);

            if (FixedMath.IsZero(magA) || FixedMath.IsZero(magB))
                return FixedMath.Zero<T>();

            var dot = Dot3D(a, b);
            var cos = dot / (magA * magB);

            return FixedMath.Acos(cos);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint<T> SignedAngleDeg3D<T>(FixedVector3<T> from, FixedVector3<T> to, FixedVector3<T> referenceNormal) where T : struct, IFixedPointFormat
        {
            var angle = AngleDeg3D(from, to);
            var cross = Cross3D(from, to);
            var sign = FixedMath.Sign(Dot3D(cross, referenceNormal));
            return angle * sign;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedVector3<T> Cross3D<T>(FixedVector3<T> a, FixedVector3<T> b) where T : struct, IFixedPointFormat
        {
            return new FixedVector3<T>(
                a.y * b.z - a.z * b.y,
                a.z * b.x - a.x * b.z,
                a.x * b.y - a.y * b.x
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedVector3<T> Reflect3D<T>(FixedVector3<T> inDirection, FixedVector3<T> normal) where T : struct, IFixedPointFormat
        {
            var dot = Dot3D(inDirection, normal);
            var twiceProj = normal * (dot + dot);
            return inDirection - twiceProj;
        }


        public static FixedVector3<T> Sign<T>(FixedVector3<T> v) where T : struct, IFixedPointFormat
            => new FixedVector3<T>(Math.Sign(v.x.Raw), Math.Sign(v.y.Raw), Math.Sign(v.z.Raw));

        public static FixedVector3<T> Inverse<T>(FixedVector3<T> v) where T : struct, IFixedPointFormat
            => new FixedVector3<T>(-v.x, -v.y, -v.z);



        // ========== LERP VECTEURS ==========

        /// <summary>
        /// Interpolation linéaire entre deux FixedVector2<TFormat>;.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedVector2<TFormat> Lerp<TFormat>(
            FixedVector2<TFormat> a,
            FixedVector2<TFormat> b,
            FixedPoint<TFormat> t)
            where TFormat : struct, IFixedPointFormat
        {
            return new FixedVector2<TFormat>(
                FixedMath<TFormat>.Lerp(a.x, b.x, t),
                FixedMath<TFormat>.Lerp(a.y, b.y, t)
            );
        }

        /// <summary>
        /// Interpolation linéaire entre deux FixedVector3<TFormat>;.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedVector3<TFormat> Lerp<TFormat>(
            FixedVector3<TFormat> a,
            FixedVector3<TFormat> b,
            FixedPoint<TFormat> t)
            where TFormat : struct, IFixedPointFormat
        {
            return new FixedVector3<TFormat>(
                FixedMath<TFormat>.Lerp(a.x, b.x, t),
                FixedMath<TFormat>.Lerp(a.y, b.y, t),
                FixedMath<TFormat>.Lerp(a.z, b.z, t)
            );
        }
    }
}
