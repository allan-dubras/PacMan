using UnityEngine;
using System;
using System.Runtime.CompilerServices;
using FixedEngine;

public static class FixedTrigLUT
{
    private const int TableSize = 360;

    // ===== Tables statiques =====
    private static readonly FixedPoint<Q8_4>[] sinQ8_4 = Generate<Q8_4>();
    private static readonly FixedPoint<Q8_4U>[] sinQ8_4U = Generate<Q8_4U>();
    private static readonly FixedPoint<Q8_8>[] sinQ8_8 = Generate<Q8_8>();
    private static readonly FixedPoint<Q8_8U>[] sinQ8_8U = Generate<Q8_8U>();
    private static readonly FixedPoint<Q16_4>[] sinQ16_4 = Generate<Q16_4>();
    private static readonly FixedPoint<Q16_4U>[] sinQ16_4U = Generate<Q16_4U>();
    private static readonly FixedPoint<Q16_8>[] sinQ16_8 = Generate<Q16_8>();
    private static readonly FixedPoint<Q16_8U>[] sinQ16_8U = Generate<Q16_8U>();
    private static readonly FixedPoint<Q16_16>[] sinQ16_16 = Generate<Q16_16>();
    private static readonly FixedPoint<Q16_16U>[] sinQ16_16U = Generate<Q16_16U>();

    // ===== Accès public via cast =====
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FixedPoint<TFormat> Sin<TFormat>(FixedPoint<TFormat> degrees) where TFormat : struct, IFixedPointFormat
    {
        int angle = (int)Mathf.Round(degrees.ToFloat()) % 360;
        if (angle < 0) angle += 360;

        if (typeof(TFormat) == typeof(Q8_4)) return (FixedPoint<TFormat>)(object)sinQ8_4[angle];
        if (typeof(TFormat) == typeof(Q8_4U)) return (FixedPoint<TFormat>)(object)sinQ8_4U[angle];
        if (typeof(TFormat) == typeof(Q8_8)) return (FixedPoint<TFormat>)(object)sinQ8_8[angle];
        if (typeof(TFormat) == typeof(Q8_8U)) return (FixedPoint<TFormat>)(object)sinQ8_8U[angle];
        if (typeof(TFormat) == typeof(Q16_4)) return (FixedPoint<TFormat>)(object)sinQ16_4[angle];
        if (typeof(TFormat) == typeof(Q16_4U)) return (FixedPoint<TFormat>)(object)sinQ16_4U[angle];
        if (typeof(TFormat) == typeof(Q16_8)) return (FixedPoint<TFormat>)(object)sinQ16_8[angle];
        if (typeof(TFormat) == typeof(Q16_8U)) return (FixedPoint<TFormat>)(object)sinQ16_8U[angle];
        if (typeof(TFormat) == typeof(Q16_16)) return (FixedPoint<TFormat>)(object)sinQ16_16[angle];
        if (typeof(TFormat) == typeof(Q16_16U)) return (FixedPoint<TFormat>)(object)sinQ16_16U[angle];

        throw new NotSupportedException($"Sin<T>: Format {typeof(TFormat).Name} non pris en charge.");
    }

    // ===== Générateur générique de table sin =====
    private static FixedPoint<TFormat>[] Generate<TFormat>() where TFormat : struct, IFixedPointFormat
    {
        var table = new FixedPoint<TFormat>[TableSize];
        for (int i = 0; i < TableSize; i++)
        {
            float radians = i * Mathf.Deg2Rad;
            float value = Mathf.Sin(radians);
            table[i] = FixedPoint<TFormat>.FromFloat(value);
        }
        return table;
    }
}
