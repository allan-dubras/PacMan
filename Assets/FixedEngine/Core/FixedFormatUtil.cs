using System;

namespace FixedEngine
{
    public static class FixedFormatUtil<TFormat>
    {
        public static readonly int FractionBits;
        public static readonly int IntegerBits;
        public static readonly bool IsSigned;

        static FixedFormatUtil()
        {
            var type = typeof(TFormat);
            FractionBits = (int)type.GetField("FractionBits").GetValue(null);
            IntegerBits = (int)type.GetField("IntegerBits").GetValue(null);
            IsSigned = (bool)type.GetField("IsSigned").GetValue(null);
        }
    }
}
