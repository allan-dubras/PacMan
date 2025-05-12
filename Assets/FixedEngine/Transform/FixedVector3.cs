using System.Runtime.CompilerServices;

using UnityEngine;

namespace FixedEngine
{

    public interface IFixedVector3D<TFormat> : IFixedVector<TFormat> where TFormat : struct, IFixedPointFormat
    {
        FixedPoint<TFormat> z { get; }
        Vector3 ToVector3();
    }

    public struct FixedVector3<TFormat> : IFixedVector3D<TFormat> where TFormat : struct, IFixedPointFormat
    {
        public FixedPoint<TFormat> x { get; private set; }
        public FixedPoint<TFormat> y { get; private set; }
        public FixedPoint<TFormat> z { get; private set; }

        public FixedVector3(FixedPoint<TFormat> x, FixedPoint<TFormat> y, FixedPoint<TFormat> z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vector3 ToVector3() => new Vector3(x.ToFloat(), y.ToFloat(), z.ToFloat());
        public Vector2 ToVector2() => new Vector2(x.ToFloat(), y.ToFloat());

        public static implicit operator Vector3(FixedVector3<TFormat> v) => v.ToVector3();

        public static FixedVector3<TFormat> Zero => new FixedVector3<TFormat>(FixedMath.Zero<TFormat>(), FixedMath.Zero<TFormat>(), FixedMath.Zero<TFormat>());
        public static FixedVector3<TFormat> One => new FixedVector3<TFormat>(FixedMath.One<TFormat>(), FixedMath.One<TFormat>(), FixedMath.One<TFormat>());
        public static FixedVector3<TFormat> Up => new FixedVector3<TFormat>(FixedMath.Zero<TFormat>(), FixedMath.One<TFormat>(), FixedMath.Zero<TFormat>());
        public static FixedVector3<TFormat> Down => new FixedVector3<TFormat>(FixedMath.Zero<TFormat>(), -FixedMath.One<TFormat>(), FixedMath.Zero<TFormat>());
        public static FixedVector3<TFormat> Left => new FixedVector3<TFormat>(-FixedMath.One<TFormat>(), FixedMath.Zero<TFormat>(), FixedMath.Zero<TFormat>());
        public static FixedVector3<TFormat> Right => new FixedVector3<TFormat>(FixedMath.One<TFormat>(), FixedMath.Zero<TFormat>(), FixedMath.Zero<TFormat>());
        public static FixedVector3<TFormat> Forward => new FixedVector3<TFormat>(FixedMath.Zero<TFormat>(), FixedMath.Zero<TFormat>(), FixedMath.One<TFormat>());
        public static FixedVector3<TFormat> Back => new FixedVector3<TFormat>(FixedMath.Zero<TFormat>(), FixedMath.Zero<TFormat>(), -FixedMath.One<TFormat>());

        // ========== OPÉRATEURS ==========

        public static FixedVector3<TFormat> operator +(FixedVector3<TFormat> a, FixedVector3<TFormat> b)
            => new FixedVector3<TFormat>(a.x + b.x, a.y + b.y, a.z + b.z);

        public static FixedVector3<TFormat> operator -(FixedVector3<TFormat> a, FixedVector3<TFormat> b)
            => new FixedVector3<TFormat>(a.x - b.x, a.y - b.y, a.z - b.z);

        public static FixedVector3<TFormat> operator *(FixedVector3<TFormat> a, FixedPoint<TFormat> scalar)
            => new FixedVector3<TFormat>(a.x * scalar, a.y * scalar, a.z * scalar);

        public static FixedVector3<TFormat> operator *(FixedVector3<TFormat> a, float scalar)
            => a * FixedPoint<TFormat>.FromFloat(scalar);

        public static FixedVector3<TFormat> operator *(FixedVector3<TFormat> a, int scalar)
            => a * FixedPoint<TFormat>.FromFloat(scalar);

        public static FixedVector3<TFormat> operator -(FixedVector3<TFormat> v)
            => new FixedVector3<TFormat>(-v.x, -v.y, -v.z);

#if UNITY_EDITOR
        public override string ToString()
            => $"({x.ToFloat():F4}, {y.ToFloat():F4}, {z.ToFloat():F4})";
#else
        public override string ToString() => base.ToString();
#endif

        public override bool Equals(object obj)
            => obj is FixedVector3<TFormat> other
               && x == other.x && y == other.y && z == other.z;

        public override int GetHashCode()
            => (rawX: x.Raw, rawY: y.Raw, rawZ: z.Raw).GetHashCode();



        

        // ========== DÉLÉGATION VERS FIXEDVECTORMATH ==========

        public FixedPoint<TFormat> SqrMagnitude => FixedVectorMath.SqrMagnitude3D(this);
        public FixedPoint<TFormat> Magnitude => FixedVectorMath.Magnitude3D(this);
        public FixedVector3<TFormat> Normalized => FixedVectorMath.Normalize3D(this);
        public FixedVector3<TFormat> Sign() => FixedVectorMath.Sign(this);
        public FixedVector3<TFormat> ClampMagnitude(FixedPoint<TFormat> max) => FixedVectorMath.ClampMagnitude3D(this, max);
    }
}
