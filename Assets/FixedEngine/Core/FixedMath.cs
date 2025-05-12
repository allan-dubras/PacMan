using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace FixedEngine
{
    public static class FixedMath
    {

        // ========== ARITHMÉTIQUE DE BASE ==========
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint<TFormat> Add<TFormat>(FixedPoint<TFormat> a, FixedPoint<TFormat> b) where TFormat : struct, IFixedPointFormat
        
        => new FixedPoint<TFormat>(a.Raw + b.Raw);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint<TFormat> Subtract<TFormat>(FixedPoint<TFormat> a, FixedPoint<TFormat> b) where TFormat : struct, IFixedPointFormat
        
            => new FixedPoint<TFormat>(a.Raw - b.Raw);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint<TFormat> Multiply<TFormat>(FixedPoint<TFormat> a, FixedPoint<TFormat> b) where TFormat : struct, IFixedPointFormat
            => new FixedPoint<TFormat>((int)(((long)a.Raw * b.Raw) >> FixedFormatUtil<TFormat>.FractionBits));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint<TFormat> Divide<TFormat>(FixedPoint<TFormat> a, FixedPoint<TFormat> b) where TFormat : struct, IFixedPointFormat
            => new FixedPoint<TFormat>((int)(((long)a.Raw << FixedFormatUtil<TFormat>.FractionBits) / b.Raw));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint<TFormat> Abs<TFormat>(FixedPoint<TFormat> v) where TFormat : struct, IFixedPointFormat
            => v.Raw >= 0 ? v : new FixedPoint<TFormat>(-v.Raw);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Sign<TFormat>(FixedPoint<TFormat> v) where TFormat : struct, IFixedPointFormat
            => Math.Sign(v.Raw);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint<TFormat> Min<TFormat>(FixedPoint<TFormat> a, FixedPoint<TFormat> b) where TFormat : struct, IFixedPointFormat
            => a.Raw < b.Raw ? a : b;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint<TFormat> Max<TFormat>(FixedPoint<TFormat> a, FixedPoint<TFormat> b) where TFormat : struct, IFixedPointFormat
            => a.Raw > b.Raw ? a : b;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint<TFormat> Clamp<TFormat>(FixedPoint<TFormat> value, FixedPoint<TFormat> min, FixedPoint<TFormat> max) where TFormat : struct, IFixedPointFormat
            => Max(min, Min(max, value));

        // ========== INTERPOLATION ==========

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint<TFormat> Lerp<TFormat>(FixedPoint<TFormat> a, FixedPoint<TFormat> b, FixedPoint<TFormat> t) where TFormat : struct, IFixedPointFormat
            => a + (b - a) * t;

        // ========== REMAP ==========

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint<TFormat> Remap<TFormat>(
            FixedPoint<TFormat> value,
            FixedPoint<TFormat> inMin, FixedPoint<TFormat> inMax,
            FixedPoint<TFormat> outMin, FixedPoint<TFormat> outMax) where TFormat : struct, IFixedPointFormat
        {

            var range = inMax - inMin;
            if (range.Raw == 0)
            {
                Debug.LogWarning($"[FixedMath<{typeof(TFormat).Name}>] Remap division by zero with inMin==inMax: {inMin.ToFloat()}");
                return outMin;
            }
            var t = (value - inMin) / range;
            return outMin + (outMax - outMin) * t;
        }

        // ========== ARRONDISSEMENT ET RACINE ==========

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint<TFormat> Round<TFormat>(FixedPoint<TFormat> value) where TFormat : struct, IFixedPointFormat
        {
            int fBits = FixedFormatUtil<TFormat>.FractionBits;
            int half = 1 << (fBits - 1);
            return new FixedPoint<TFormat>((value.Raw + half) & ~((1 << fBits) - 1));
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint<TFormat> Floor<TFormat>(FixedPoint<TFormat> value) where TFormat : struct, IFixedPointFormat
        {
            int fBits = FixedFormatUtil<TFormat>.FractionBits;
            return new FixedPoint<TFormat>(value.Raw & ~((1 << fBits) - 1));
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint<TFormat> Ceil<TFormat>(FixedPoint<TFormat> value) where TFormat : struct, IFixedPointFormat
        {
            int fBits = FixedFormatUtil<TFormat>.FractionBits;
            int mask = (1 << fBits) - 1;
            return new FixedPoint<TFormat>((value.Raw + mask) & ~mask);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint<TFormat> Square<TFormat>(FixedPoint<TFormat> v) where TFormat : struct, IFixedPointFormat
            => Multiply<TFormat>(v, v);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint<TFormat> Sqrt<TFormat>(FixedPoint<TFormat> value) where TFormat : struct, IFixedPointFormat
        {
            if (value.Raw <= 0)
                return FixedMath.Zero<TFormat>();

            int num = value.Raw;
            int fBits = FixedFormatUtil<TFormat>.FractionBits;

            // Convert to higher precision for more accuracy
            long n = ((long)num) << fBits;

            long x = n;
            long approx = n >> 1;

            // 6 itérations max, suffisant pour les formats fixes
            for (int i = 0; i < 6; i++)
            {
                if (approx == 0) break;
                approx = (approx + n / approx) >> 1;
            }

            return new FixedPoint<TFormat>((int)approx);
        }


        // ========== TRIGONOMÉTRIE ==========

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint<TFormat> Sin<TFormat>(FixedPoint<TFormat> degrees) where TFormat : struct, IFixedPointFormat
        {
            return FixedTrigLUT.Sin(degrees);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint<TFormat> Cos<TFormat>(FixedPoint<TFormat> degrees) where TFormat : struct, IFixedPointFormat
        {
            return Sin<TFormat>(degrees + FixedMath.Fixed90<TFormat>());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint<TFormat> Acos<TFormat>(FixedPoint<TFormat> x) where TFormat : struct, IFixedPointFormat
        {
            // Clamp x in [-1, 1]
            var one = FixedMath.One<TFormat>();
            x = FixedMath.Clamp(x, -one, one);

            // Préparer 1/6 pour ce format
            FixedPoint<TFormat> third;
            if (typeof(TFormat) == typeof(Q8_4)) third = (FixedPoint<TFormat>)(object)new FixedPoint<Q8_4>(3);         // 1/6 × 16
            else if (typeof(TFormat) == typeof(Q8_4U)) third = (FixedPoint<TFormat>)(object)new FixedPoint<Q8_4U>(3);
            else if (typeof(TFormat) == typeof(Q8_8)) third = (FixedPoint<TFormat>)(object)new FixedPoint<Q8_8>(42);   // 1/6 × 256
            else if (typeof(TFormat) == typeof(Q8_8U)) third = (FixedPoint<TFormat>)(object)new FixedPoint<Q8_8U>(42);
            else if (typeof(TFormat) == typeof(Q16_4)) third = (FixedPoint<TFormat>)(object)new FixedPoint<Q16_4>(3);  // 1/6 × 16
            else if (typeof(TFormat) == typeof(Q16_4U)) third = (FixedPoint<TFormat>)(object)new FixedPoint<Q16_4U>(3);
            else if (typeof(TFormat) == typeof(Q16_8)) third = (FixedPoint<TFormat>)(object)new FixedPoint<Q16_8>(42); // 1/6 × 256
            else if (typeof(TFormat) == typeof(Q16_8U)) third = (FixedPoint<TFormat>)(object)new FixedPoint<Q16_8U>(42);
            else if (typeof(TFormat) == typeof(Q16_16)) third = (FixedPoint<TFormat>)(object)new FixedPoint<Q16_16>(10922); // 1/6 × 65536
            else if (typeof(TFormat) == typeof(Q16_16U)) third = (FixedPoint<TFormat>)(object)new FixedPoint<Q16_16U>(10922);
            else throw new NotSupportedException($"Acos<T>: Format {typeof(TFormat).Name} non pris en charge.");

            // Approximation : acos(x) ≈ 90° - (x + x³ / 6)
            var x3 = x * x * x;
            var ninety = FixedMath.Fixed90<TFormat>();
            var approx = ninety - (x + x3 * third);

            return FixedMath.Clamp(approx, FixedMath.Zero<TFormat>(), FixedMath.Fixed180<TFormat>());
        }



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint<TFormat> Atan2<TFormat>(FixedPoint<TFormat> y, FixedPoint<TFormat> x) where TFormat : struct, IFixedPointFormat
        {
            if (x.Raw == 0 && y.Raw == 0)
                return FixedMath.Zero<TFormat>();

            FixedPoint<TFormat> absY = FixedMath.Abs(y);
            FixedPoint<TFormat> angle;

            if (x.Raw > 0)
            {
                var r = (x - absY) / (x + absY);
                angle = FixedMath.Fixed45<TFormat>() - FixedMath.Fixed15<TFormat>() * r;
            }
            else if (x.Raw < 0)
            {
                var r = (x + absY) / (absY - x);
                angle = FixedMath.Fixed135<TFormat>() - FixedMath.Fixed15<TFormat>() * r;
            }
            else // x == 0
            {
                angle = FixedMath.Fixed90<TFormat>();
            }

            if (y.Raw < 0)
                angle = -angle;

            // quadrant correction
            if (x.Raw < 0)
                angle += FixedMath.Fixed180<TFormat>();

            // normalization to [0, 360)
            var fullCircle = FixedMath.Fixed360<TFormat>();
            if (angle.Raw < 0)
                angle += fullCircle;
            else if (angle.Raw >= fullCircle.Raw)
                angle -= fullCircle;

            return angle;
        }

        // ========== CONSTANTES ==========

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint<TFormat> Zero<TFormat>() where TFormat : struct, IFixedPointFormat
        {
            return new FixedPoint<TFormat>(0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint<TFormat> One<TFormat>() where TFormat : struct, IFixedPointFormat
        {
            return new FixedPoint<TFormat>(1 << FixedFormatUtil<TFormat>.FractionBits);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint<TFormat> Half<TFormat>() where TFormat : struct, IFixedPointFormat
        {
            return new FixedPoint<TFormat>(1 << (FixedFormatUtil<TFormat>.FractionBits - 1));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint<TFormat> PI<TFormat>() where TFormat : struct, IFixedPointFormat
        {
            if (typeof(TFormat) == typeof(Q8_4))
                return (FixedPoint<TFormat>)(object)new FixedPoint<Q8_4>(50); // 3.125
            if (typeof(TFormat) == typeof(Q8_4U))
                return (FixedPoint<TFormat>)(object)new FixedPoint<Q8_4U>(50); // 3.125

            if (typeof(TFormat) == typeof(Q8_8))
                return (FixedPoint<TFormat>)(object)new FixedPoint<Q8_8>(804); // 3.140625
            if (typeof(TFormat) == typeof(Q8_8U))
                return (FixedPoint<TFormat>)(object)new FixedPoint<Q8_8U>(804); // 3.140625

            if (typeof(TFormat) == typeof(Q16_4))
                return (FixedPoint<TFormat>)(object)new FixedPoint<Q16_4>(50); // 3.125
            if (typeof(TFormat) == typeof(Q16_4U))
                return (FixedPoint<TFormat>)(object)new FixedPoint<Q16_4U>(50); // 3.125

            if (typeof(TFormat) == typeof(Q16_8))
                return (FixedPoint<TFormat>)(object)new FixedPoint<Q16_8>(804); // 3.140625
            if (typeof(TFormat) == typeof(Q16_8U))
                return (FixedPoint<TFormat>)(object)new FixedPoint<Q16_8U>(804); // 3.140625

            if (typeof(TFormat) == typeof(Q16_16))
                return (FixedPoint<TFormat>)(object)new FixedPoint<Q16_16>(205887); // 3.1415863037
            if (typeof(TFormat) == typeof(Q16_16U))
                return (FixedPoint<TFormat>)(object)new FixedPoint<Q16_16U>(205887); // 3.1415863037

            throw new NotSupportedException($"PI<T>: Format {typeof(TFormat).Name} non pris en charge.");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint<TFormat> TWO_PI<TFormat>() where TFormat : struct, IFixedPointFormat
        {
            return PI<TFormat>() + PI<TFormat>();
        }

        

        public static FixedPoint<TFormat> Fixed15<TFormat>() where TFormat : struct, IFixedPointFormat
        {
            if (typeof(TFormat) == typeof(Q8_4)) return (FixedPoint<TFormat>)(object)new FixedPoint<Q8_4>(240); // 15 × 16
            if (typeof(TFormat) == typeof(Q8_4U)) return (FixedPoint<TFormat>)(object)new FixedPoint<Q8_4U>(240);
            if (typeof(TFormat) == typeof(Q8_8)) return (FixedPoint<TFormat>)(object)new FixedPoint<Q8_8>(3840); // 15 × 256
            if (typeof(TFormat) == typeof(Q8_8U)) return (FixedPoint<TFormat>)(object)new FixedPoint<Q8_8U>(3840);
            if (typeof(TFormat) == typeof(Q16_4)) return (FixedPoint<TFormat>)(object)new FixedPoint<Q16_4>(240);
            if (typeof(TFormat) == typeof(Q16_4U)) return (FixedPoint<TFormat>)(object)new FixedPoint<Q16_4U>(240);
            if (typeof(TFormat) == typeof(Q16_8)) return (FixedPoint<TFormat>)(object)new FixedPoint<Q16_8>(3840);
            if (typeof(TFormat) == typeof(Q16_8U)) return (FixedPoint<TFormat>)(object)new FixedPoint<Q16_8U>(3840);
            if (typeof(TFormat) == typeof(Q16_16)) return (FixedPoint<TFormat>)(object)new FixedPoint<Q16_16>(983040); // 15 × 65536
            if (typeof(TFormat) == typeof(Q16_16U)) return (FixedPoint<TFormat>)(object)new FixedPoint<Q16_16U>(983040);

            throw new NotSupportedException($"Fixed15<T>: Format {typeof(TFormat).Name} non pris en charge.");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint<TFormat> Fixed45<TFormat>() where TFormat : struct, IFixedPointFormat
        {
            if (typeof(TFormat) == typeof(Q8_4)) return (FixedPoint<TFormat>)(object)new FixedPoint<Q8_4>(720);
            if (typeof(TFormat) == typeof(Q8_4U)) return (FixedPoint<TFormat>)(object)new FixedPoint<Q8_4U>(720);
            if (typeof(TFormat) == typeof(Q8_8)) return (FixedPoint<TFormat>)(object)new FixedPoint<Q8_8>(11520);
            if (typeof(TFormat) == typeof(Q8_8U)) return (FixedPoint<TFormat>)(object)new FixedPoint<Q8_8U>(11520);
            if (typeof(TFormat) == typeof(Q16_4)) return (FixedPoint<TFormat>)(object)new FixedPoint<Q16_4>(720);
            if (typeof(TFormat) == typeof(Q16_4U)) return (FixedPoint<TFormat>)(object)new FixedPoint<Q16_4U>(720);
            if (typeof(TFormat) == typeof(Q16_8)) return (FixedPoint<TFormat>)(object)new FixedPoint<Q16_8>(11520);
            if (typeof(TFormat) == typeof(Q16_8U)) return (FixedPoint<TFormat>)(object)new FixedPoint<Q16_8U>(11520);
            if (typeof(TFormat) == typeof(Q16_16)) return (FixedPoint<TFormat>)(object)new FixedPoint<Q16_16>(2949120);
            if (typeof(TFormat) == typeof(Q16_16U)) return (FixedPoint<TFormat>)(object)new FixedPoint<Q16_16U>(2949120);

            throw new NotSupportedException($"Fixed45<T>: Format {typeof(TFormat).Name} non pris en charge.");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint<TFormat> Fixed90<TFormat>() where TFormat : struct, IFixedPointFormat
        {
            if (typeof(TFormat) == typeof(Q8_4)) return (FixedPoint<TFormat>)(object)new FixedPoint<Q8_4>(1440); // 90 × 16
            if (typeof(TFormat) == typeof(Q8_4U)) return (FixedPoint<TFormat>)(object)new FixedPoint<Q8_4U>(1440);
            if (typeof(TFormat) == typeof(Q8_8)) return (FixedPoint<TFormat>)(object)new FixedPoint<Q8_8>(23040); // 90 × 256
            if (typeof(TFormat) == typeof(Q8_8U)) return (FixedPoint<TFormat>)(object)new FixedPoint<Q8_8U>(23040);
            if (typeof(TFormat) == typeof(Q16_4)) return (FixedPoint<TFormat>)(object)new FixedPoint<Q16_4>(1440);
            if (typeof(TFormat) == typeof(Q16_4U)) return (FixedPoint<TFormat>)(object)new FixedPoint<Q16_4U>(1440);
            if (typeof(TFormat) == typeof(Q16_8)) return (FixedPoint<TFormat>)(object)new FixedPoint<Q16_8>(23040);
            if (typeof(TFormat) == typeof(Q16_8U)) return (FixedPoint<TFormat>)(object)new FixedPoint<Q16_8U>(23040);
            if (typeof(TFormat) == typeof(Q16_16)) return (FixedPoint<TFormat>)(object)new FixedPoint<Q16_16>(5898240);
            if (typeof(TFormat) == typeof(Q16_16U)) return (FixedPoint<TFormat>)(object)new FixedPoint<Q16_16U>(5898240);

            throw new NotSupportedException($"Fixed90<T>: Format {typeof(TFormat).Name} non pris en charge.");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint<TFormat> Fixed135<TFormat>() where TFormat : struct, IFixedPointFormat
        {
            if (typeof(TFormat) == typeof(Q8_4)) return (FixedPoint<TFormat>)(object)new FixedPoint<Q8_4>(2160);
            if (typeof(TFormat) == typeof(Q8_4U)) return (FixedPoint<TFormat>)(object)new FixedPoint<Q8_4U>(2160);
            if (typeof(TFormat) == typeof(Q8_8)) return (FixedPoint<TFormat>)(object)new FixedPoint<Q8_8>(34560);
            if (typeof(TFormat) == typeof(Q8_8U)) return (FixedPoint<TFormat>)(object)new FixedPoint<Q8_8U>(34560);
            if (typeof(TFormat) == typeof(Q16_4)) return (FixedPoint<TFormat>)(object)new FixedPoint<Q16_4>(2160);
            if (typeof(TFormat) == typeof(Q16_4U)) return (FixedPoint<TFormat>)(object)new FixedPoint<Q16_4U>(2160);
            if (typeof(TFormat) == typeof(Q16_8)) return (FixedPoint<TFormat>)(object)new FixedPoint<Q16_8>(34560);
            if (typeof(TFormat) == typeof(Q16_8U)) return (FixedPoint<TFormat>)(object)new FixedPoint<Q16_8U>(34560);
            if (typeof(TFormat) == typeof(Q16_16)) return (FixedPoint<TFormat>)(object)new FixedPoint<Q16_16>(8847360);
            if (typeof(TFormat) == typeof(Q16_16U)) return (FixedPoint<TFormat>)(object)new FixedPoint<Q16_16U>(8847360);

            throw new NotSupportedException($"Fixed135<T>: Format {typeof(TFormat).Name} non pris en charge.");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint<TFormat> Fixed180<TFormat>() where TFormat : struct, IFixedPointFormat
        {
            if (typeof(TFormat) == typeof(Q8_4)) return (FixedPoint<TFormat>)(object)new FixedPoint<Q8_4>(2880); // 180 × 16
            if (typeof(TFormat) == typeof(Q8_4U)) return (FixedPoint<TFormat>)(object)new FixedPoint<Q8_4U>(2880);
            if (typeof(TFormat) == typeof(Q8_8)) return (FixedPoint<TFormat>)(object)new FixedPoint<Q8_8>(46080); // 180 × 256
            if (typeof(TFormat) == typeof(Q8_8U)) return (FixedPoint<TFormat>)(object)new FixedPoint<Q8_8U>(46080);
            if (typeof(TFormat) == typeof(Q16_4)) return (FixedPoint<TFormat>)(object)new FixedPoint<Q16_4>(2880);
            if (typeof(TFormat) == typeof(Q16_4U)) return (FixedPoint<TFormat>)(object)new FixedPoint<Q16_4U>(2880);
            if (typeof(TFormat) == typeof(Q16_8)) return (FixedPoint<TFormat>)(object)new FixedPoint<Q16_8>(46080);
            if (typeof(TFormat) == typeof(Q16_8U)) return (FixedPoint<TFormat>)(object)new FixedPoint<Q16_8U>(46080);
            if (typeof(TFormat) == typeof(Q16_16)) return (FixedPoint<TFormat>)(object)new FixedPoint<Q16_16>(11796480); // 180 × 65536
            if (typeof(TFormat) == typeof(Q16_16U)) return (FixedPoint<TFormat>)(object)new FixedPoint<Q16_16U>(11796480);

            throw new NotSupportedException($"Fixed180<T>: Format {typeof(TFormat).Name} non pris en charge.");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint<TFormat> Fixed360<TFormat>() where TFormat : struct, IFixedPointFormat
        {
            if (typeof(TFormat) == typeof(Q8_4)) return (FixedPoint<TFormat>)(object)new FixedPoint<Q8_4>(5760); // 360 × 16
            if (typeof(TFormat) == typeof(Q8_4U)) return (FixedPoint<TFormat>)(object)new FixedPoint<Q8_4U>(5760);
            if (typeof(TFormat) == typeof(Q8_8)) return (FixedPoint<TFormat>)(object)new FixedPoint<Q8_8>(92160); // 360 × 256
            if (typeof(TFormat) == typeof(Q8_8U)) return (FixedPoint<TFormat>)(object)new FixedPoint<Q8_8U>(92160);
            if (typeof(TFormat) == typeof(Q16_4)) return (FixedPoint<TFormat>)(object)new FixedPoint<Q16_4>(5760);
            if (typeof(TFormat) == typeof(Q16_4U)) return (FixedPoint<TFormat>)(object)new FixedPoint<Q16_4U>(5760);
            if (typeof(TFormat) == typeof(Q16_8)) return (FixedPoint<TFormat>)(object)new FixedPoint<Q16_8>(92160);
            if (typeof(TFormat) == typeof(Q16_8U)) return (FixedPoint<TFormat>)(object)new FixedPoint<Q16_8U>(92160);
            if (typeof(TFormat) == typeof(Q16_16)) return (FixedPoint<TFormat>)(object)new FixedPoint<Q16_16>(23592960); // 360 × 65536
            if (typeof(TFormat) == typeof(Q16_16U)) return (FixedPoint<TFormat>)(object)new FixedPoint<Q16_16U>(23592960);

            throw new NotSupportedException($"Fixed360<T>: Format {typeof(TFormat).Name} non pris en charge.");
        }



        // ========== CONVERSIONS ANGULAIRES ==========

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint<TFormat> Deg2Rad<TFormat>() where TFormat : struct, IFixedPointFormat
        {
            return PI<TFormat>() / Fixed180<TFormat>();
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint<TFormat> Rad2Deg<TFormat>() where TFormat : struct, IFixedPointFormat
        {
            return Fixed180<TFormat>() / PI<TFormat>();
        }

        // ========== COMPARAISONS ==========

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Approximately<TFormat>(FixedPoint<TFormat> a, FixedPoint<TFormat> b, FixedPoint<TFormat> tolerance) where TFormat : struct, IFixedPointFormat
            => Abs(a - b) <= tolerance;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsZero<TFormat>(FixedPoint<TFormat> v) where TFormat : struct, IFixedPointFormat
            => v.Raw == 0;

        // ========== CONVERSION ==========
        public static FixedPoint<TTo> Convert<TFrom, TTo>(FixedPoint<TFrom> v) 
            where TFrom : struct, IFixedPointFormat
            where TTo : struct, IFixedPointFormat
            => new FixedPoint<TTo>(v.Raw);


        

    }



    // ========== WRAPPER ==========

    /// <summary>
    /// Wrapper générique pour tous les calculs en FixedPoint de format TFormat.
    /// </summary>
    public static class FixedMath<TFormat>
        where TFormat : struct, IFixedPointFormat
    {
        /// <summary>Valeur nulle en virgule fixe pour le format TFormat.</summary>
        public static FixedPoint<TFormat> Zero => FixedMath.Zero<TFormat>();

        /// <summary>Valeur unitaire (1.0) en virgule fixe pour le format TFormat.</summary>
        public static FixedPoint<TFormat> One => FixedMath.One<TFormat>();

        /// <summary>Demi (0.5) en virgule fixe pour le format TFormat.</summary>
        public static FixedPoint<TFormat> Half => FixedMath.Half<TFormat>();

        /// <summary>Addition de deux FixedPoint&lt;TFormat&gt;.</summary>
        public static FixedPoint<TFormat> Add(FixedPoint<TFormat> a, FixedPoint<TFormat> b)
            => FixedMath.Add<TFormat>(a, b);

        /// <summary>Soustraction de deux FixedPoint&lt;TFormat&gt;.</summary>
        public static FixedPoint<TFormat> Subtract(FixedPoint<TFormat> a, FixedPoint<TFormat> b)
            => FixedMath.Subtract<TFormat>(a, b);

        /// <summary>Multiplication de deux FixedPoint&lt;TFormat&gt;.</summary>
        public static FixedPoint<TFormat> Multiply(FixedPoint<TFormat> a, FixedPoint<TFormat> b)
            => FixedMath.Multiply<TFormat>(a, b);

        /// <summary>Division de deux FixedPoint&lt;TFormat&gt;.</summary>
        public static FixedPoint<TFormat> Divide(FixedPoint<TFormat> a, FixedPoint<TFormat> b)
            => FixedMath.Divide<TFormat>(a, b);

        /// <summary>Interpolation linéaire entre deux FixedPoint&lt;TFormat&gt;.</summary>
        public static FixedPoint<TFormat> Lerp(FixedPoint<TFormat> a, FixedPoint<TFormat> b, FixedPoint<TFormat> t)
            => FixedMath.Lerp<TFormat>(a, b, t);

        // Ajoutez ici d'autres méthodes (Min, Max, Clamp, Sin, Cos, etc.) si nécessaire.
    }

}
