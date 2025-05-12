namespace FixedEngine
{
    /// <summary>
    /// Formats de nombres fixes supportés par FixedPoint&lt;TFormat&gt;.
    /// Chaque struct doit implémenter IFixedPointFormat pour être reconnu par FixedPoint.
    /// </summary>
    public struct Q8_4 : IFixedPointFormat { public const int FractionBits = 4; public const int IntegerBits = 8; public const bool IsSigned = true; }
    public struct Q8_8 : IFixedPointFormat { public const int FractionBits = 8; public const int IntegerBits = 8; public const bool IsSigned = true; }
    public struct Q16_4 : IFixedPointFormat { public const int FractionBits = 4; public const int IntegerBits = 16; public const bool IsSigned = true; }
    public struct Q16_8 : IFixedPointFormat { public const int FractionBits = 8; public const int IntegerBits = 16; public const bool IsSigned = true; }
    public struct Q16_16 : IFixedPointFormat { public const int FractionBits = 16; public const int IntegerBits = 16; public const bool IsSigned = true; }

    public struct Q8_4U : IFixedPointFormat { public const int FractionBits = 4; public const int IntegerBits = 8; public const bool IsSigned = false; }
    public struct Q8_8U : IFixedPointFormat { public const int FractionBits = 8; public const int IntegerBits = 8; public const bool IsSigned = false; }
    public struct Q16_4U : IFixedPointFormat { public const int FractionBits = 4; public const int IntegerBits = 16; public const bool IsSigned = false; }
    public struct Q16_8U : IFixedPointFormat { public const int FractionBits = 8; public const int IntegerBits = 16; public const bool IsSigned = false; }
    public struct Q16_16U : IFixedPointFormat { public const int FractionBits = 16; public const int IntegerBits = 16; public const bool IsSigned = false; }
}
