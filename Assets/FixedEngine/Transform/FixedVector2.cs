using UnityEngine;

namespace FixedEngine
{

    public interface IFixedVector<TFormat>
    {
        Vector2 ToVector2();
    }

    public struct FixedVector2<TFormat> : IFixedVector<TFormat> where TFormat : struct, IFixedPointFormat
    {
        public FixedPoint<TFormat> x { get; private set; }
        public FixedPoint<TFormat> y { get; private set; }

        public FixedVector2(FixedPoint<TFormat> x, FixedPoint<TFormat> y)
        {
            this.x = x;
            this.y = y;
        }

        public Vector2 ToVector2() => new Vector2(x.ToFloat(), y.ToFloat());
        public static implicit operator Vector2(FixedVector2<TFormat> v) => v.ToVector2();

        public static FixedVector2<TFormat> Zero => new(FixedMath.Zero<TFormat>(), FixedMath.Zero<TFormat>());
        public static FixedVector2<TFormat> One => new(FixedMath.One<TFormat>(), FixedMath.One<TFormat>());
        public static FixedVector2<TFormat> Up => new(FixedMath.Zero<TFormat>(), FixedMath.One<TFormat>());
        public static FixedVector2<TFormat> Down => new(FixedMath.Zero<TFormat>(), -FixedMath.One<TFormat>());
        public static FixedVector2<TFormat> Left => new(-FixedMath.One<TFormat>(), FixedMath.Zero<TFormat>());
        public static FixedVector2<TFormat> Right => new(FixedMath.One<TFormat>(), FixedMath.Zero<TFormat>());


        // ========== OPÉRATEURS ==========

        public static FixedVector2<TFormat> operator +(FixedVector2<TFormat> a, FixedVector2<TFormat> b)
            => new FixedVector2<TFormat>(a.x + b.x, a.y + b.y);

        public static FixedVector2<TFormat> operator -(FixedVector2<TFormat> a, FixedVector2<TFormat> b)
            => new FixedVector2<TFormat>(a.x - b.x, a.y - b.y);

        public static FixedVector2<TFormat> operator *(FixedVector2<TFormat> a, FixedPoint<TFormat> scalar)
            => new FixedVector2<TFormat>(a.x * scalar, a.y * scalar);

        public static FixedVector2<TFormat> operator *(FixedVector2<TFormat> a, float scalar)
            => a * FixedPoint<TFormat>.FromFloat(scalar);

        public static FixedVector2<TFormat> operator *(FixedVector2<TFormat> a, int scalar)
            => a * FixedPoint<TFormat>.FromFloat(scalar);

        public static FixedVector2<TFormat> operator -(FixedVector2<TFormat> v)
            => new FixedVector2<TFormat>(-v.x, -v.y);

#if UNITY_EDITOR
        public override string ToString() => $"({x.ToFloat():F4}, {y.ToFloat():F4})";
#else
        public override string ToString() => base.ToString();
#endif
        public override bool Equals(object obj)
            => obj is FixedVector2<TFormat> other && x == other.x && y == other.y;

        public static bool operator ==(FixedVector2<TFormat> a, FixedVector2<TFormat> b)
        {
            // on compare ici les raw de FixedPoint pour rester 100 % int
            return a.x.Raw == b.x.Raw && a.y.Raw == b.y.Raw;
        }

        public static bool operator !=(FixedVector2<TFormat> a, FixedVector2<TFormat> b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
            => (rawX: x.Raw, rawY: y.Raw).GetHashCode();

        // ========== DÉLÉGATION VERS FIXEDVECTORMATH ==========

        public FixedPoint<TFormat> SqrMagnitude => FixedVectorMath.SqrMagnitude(this);
        public FixedPoint<TFormat> Magnitude => FixedVectorMath.Magnitude(this);
        public Vector2 Normalized => FixedVectorMath.Normalize(this);
        public FixedVector2<TFormat> Sign() => FixedVectorMath.Sign(this);
        public FixedVector2<TFormat> ClampMagnitude(FixedPoint<TFormat> max) => FixedVectorMath.ClampMagnitude(this, max);
    }
}
