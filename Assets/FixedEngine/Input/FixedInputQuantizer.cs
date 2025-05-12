using FixedEngine;
using UnityEngine;
using UnityEngine.Windows;

public static class FixedInputQuantizer
{
    /// <summary>
    /// Quantification au pas par défaut = 1 raw unit (1/16 pour Q8.4).
    /// </summary>
    public static FixedPoint<TFormat> Quantize<TFormat>(float input)
        where TFormat : struct, IFixedPointFormat
    {
        var fp = FixedPoint<TFormat>.FromFloat(input);
        return fp.RoundNearest();
    }

    /// <summary>
    /// Quantification au pas personnalisé (en float, ex: 0.125 pour 1/8).
    /// Tout se fait en fixe.
    /// </summary>
    public static FixedPoint<TFormat> Quantize<TFormat>(float input, float step)
        where TFormat : struct, IFixedPointFormat
    {
        var fp = FixedPoint<TFormat>.FromFloat(input);
        var stepFp = FixedPoint<TFormat>.FromFloat(step);
        var count = (fp / stepFp).RoundNearest();
        return count * stepFp;
    }

    /// <summary>
    /// Quantification directe d'un FixedPoint déjà obtenu.
    /// </summary>
    public static FixedPoint<TFormat> Quantize<TFormat>(FixedPoint<TFormat> fp)
        where TFormat : struct, IFixedPointFormat
    {
        return fp.RoundNearest();
    }

    /// <summary>
    /// Quantifie simultanément les composantes X et Y d'un Vector2.
    /// </summary>
    public static FixedVector2<TFormat> QuantizeVector2<TFormat>(Vector2 raw)
        where TFormat : struct, IFixedPointFormat
    {

        float threshold = 0.1f; // à ajuster selon ton Input System

        int dx = Mathf.Abs(raw.x) < threshold ? 0 : Mathf.RoundToInt(Mathf.Sign(raw.x));
        int dy = Mathf.Abs(raw.y) < threshold ? 0 : Mathf.RoundToInt(Mathf.Sign(raw.y));



        return new FixedVector2<TFormat>(
            FixedPoint<TFormat>.FromInt(dx),
            FixedPoint<TFormat>.FromInt(dy)
        );
    }

    /// <summary>
    /// Quantifie simultanément les composantes X, Y et Z d'un Vector3.
    /// </summary>
    public static FixedVector3<TFormat> QuantizeVector3<TFormat>(Vector3 raw)
        where TFormat : struct, IFixedPointFormat
    {
        return new FixedVector3<TFormat>(
            Quantize<TFormat>(raw.x),
            Quantize<TFormat>(raw.y),
            Quantize<TFormat>(raw.z)
        );
    }
}
