using System.Runtime.CompilerServices;
using FixedEngine;

namespace FixedEngine
{
    /// <summary>
    /// Partial struct extending FixedPoint<TFormat> with floor, ceil, round and cast operations.
    /// </summary>
    public partial struct FixedPoint<TFormat> where TFormat : struct, IFixedPointFormat
    {
        /// <summary>
        /// Explicit cast to int performs floor conversion using the original ToInt().
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator int(FixedPoint<TFormat> v)
        {
            return v.ToInt(); // Calls the ToInt() defined in the original FixedPoint.cs
        }

        /// <summary>
        /// Truncates the fixed-point value toward negative infinity (floor behavior).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FixedPoint<TFormat> Floor()
        {
            int shift = FixedFormatUtil<TFormat>.FractionBits;
            int mask = (1 << shift) - 1;
            int rawTrunc = this.Raw & ~mask;
            return new FixedPoint<TFormat>(rawTrunc);
        }

        /// <summary>
        /// Rounds the fixed-point value toward positive infinity (ceil behavior).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FixedPoint<TFormat> Ceil()
        {
            int shift = FixedFormatUtil<TFormat>.FractionBits;
            int mask = (1 << shift) - 1;
            int r = this.Raw;
            if ((r & mask) == 0)
                return this;
            int rawUp = (r & ~mask) + (1 << shift);
            return new FixedPoint<TFormat>(rawUp);
        }

        /// <summary>
        /// Rounds the fixed-point value to the nearest integer (>= .5 → ceil, else floor).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FixedPoint<TFormat> RoundNearest()
        {
            int shift = FixedFormatUtil<TFormat>.FractionBits;
            int mask = (1 << shift) - 1;
            int half = 1 << (shift - 1);
            int r = this.Raw;
            int frac = r & mask;
            int finalRaw = (frac < half)
                ? (r & ~mask)
                : ((r & ~mask) + (1 << shift));
            return new FixedPoint<TFormat>(finalRaw);
        }
    }
}
