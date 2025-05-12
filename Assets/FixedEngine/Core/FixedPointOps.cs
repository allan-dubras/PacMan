using System;
using System.Runtime.CompilerServices;

namespace FixedEngine
{
    public partial struct FixedPoint<TFormat> where TFormat : struct, IFixedPointFormat
    {
        // ========== OPERATORS ==========

        public static FixedPoint<TFormat> operator +(FixedPoint<TFormat> a, FixedPoint<TFormat> b)
            => new FixedPoint<TFormat>(a.Raw + b.Raw);

        public static FixedPoint<TFormat> operator -(FixedPoint<TFormat> a, FixedPoint<TFormat> b)
            => new FixedPoint<TFormat>(a.Raw - b.Raw);

        public static FixedPoint<TFormat> operator *(FixedPoint<TFormat> a, FixedPoint<TFormat> b)
        {
            long result = (long)a.Raw * b.Raw;
            return new FixedPoint<TFormat>((int)(result >> FixedFormatUtil<TFormat>.FractionBits));
        }

        public static FixedPoint<TFormat> operator /(FixedPoint<TFormat> a, FixedPoint<TFormat> b)
        {
            if (b.Raw == 0)
                throw new DivideByZeroException($"[FixedPoint<{typeof(TFormat).Name}>] Division par zéro");

            long result = ((long)a.Raw << FixedFormatUtil<TFormat>.FractionBits) / b.Raw;
            return new FixedPoint<TFormat>((int)result);
        }

        public static FixedPoint<TFormat> operator %(FixedPoint<TFormat> a, FixedPoint<TFormat> b)
        {
            if (b.Raw == 0)
                throw new DivideByZeroException($"[FixedPoint<{typeof(TFormat).Name}>] Modulo par zéro");

            return new FixedPoint<TFormat>(a.Raw % b.Raw);
        }

        // ========== COMPARISONS ==========

        public static bool operator ==(FixedPoint<TFormat> a, FixedPoint<TFormat> b) => a.Raw == b.Raw;
        public static bool operator !=(FixedPoint<TFormat> a, FixedPoint<TFormat> b) => a.Raw != b.Raw;
        public static bool operator <(FixedPoint<TFormat> a, FixedPoint<TFormat> b) => a.Raw < b.Raw;
        public static bool operator >(FixedPoint<TFormat> a, FixedPoint<TFormat> b) => a.Raw > b.Raw;
        public static bool operator <=(FixedPoint<TFormat> a, FixedPoint<TFormat> b) => a.Raw <= b.Raw;
        public static bool operator >=(FixedPoint<TFormat> a, FixedPoint<TFormat> b) => a.Raw >= b.Raw;

        // ========== DELEGATION TO FIXEDMATH ==========

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint<TFormat> Abs(FixedPoint<TFormat> v) => FixedMath.Abs(v);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint<TFormat> Clamp(FixedPoint<TFormat> v, FixedPoint<TFormat> min, FixedPoint<TFormat> max)
            => FixedMath.Clamp(v, min, max);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint<TFormat> Min(FixedPoint<TFormat> a, FixedPoint<TFormat> b)
            => FixedMath.Min(a, b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint<TFormat> Max(FixedPoint<TFormat> a, FixedPoint<TFormat> b)
            => FixedMath.Max(a, b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint<TFormat> Lerp(FixedPoint<TFormat> a, FixedPoint<TFormat> b, FixedPoint<TFormat> t)
            => FixedMath.Lerp(a, b, t);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Sign(FixedPoint<TFormat> v) => FixedMath.Sign(v);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint<TFormat> Remap(FixedPoint<TFormat> value,
                                                 FixedPoint<TFormat> inMin,
                                                 FixedPoint<TFormat> inMax,
                                                 FixedPoint<TFormat> outMin,
                                                 FixedPoint<TFormat> outMax)
            => FixedMath.Remap(value, inMin, inMax, outMin, outMax);

        // ========== SYSTEM ==========

        public override bool Equals(object obj)
            => obj is FixedPoint<TFormat> other && Raw == other.Raw;

        public override int GetHashCode() => Raw;
    }
}
