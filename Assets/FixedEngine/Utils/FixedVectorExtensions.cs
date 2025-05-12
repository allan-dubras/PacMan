using System.Runtime.CompilerServices;
using FixedEngine;

namespace FixedEngine.Extensions
{
    public static class FixedVectorExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedVector2<TFormat> SnapToTileCenter<TFormat>(this FixedVector2<TFormat> v)
            where TFormat : struct, IFixedPointFormat
        {
            int shift = FixedFormatUtil<TFormat>.FractionBits;
            int xr = (v.x.Raw >> shift) << shift;
            int yr = (v.y.Raw >> shift) << shift;
            return new FixedVector2<TFormat>(
                new FixedPoint<TFormat>(xr),
                new FixedPoint<TFormat>(yr)
            );
        }
    }
}
