using System;

namespace FixedEngine
{
    /// <summary>
    /// Matrice 4×4 fixe pour TRS, multiplication et transformation de points.
    /// </summary>
    public struct FixedMatrix4x4<TFormat>
        where TFormat : struct, IFixedPointFormat
    {
        private readonly FixedPoint<TFormat>[] m;

        public FixedMatrix4x4(FixedPoint<TFormat>[] elements)
        {
            if (elements == null || elements.Length != 16)
                throw new ArgumentException("FixedMatrix4x4 requires exactly 16 elements.");
            m = elements;
        }

        /// <summary>
        /// Matrice identité.
        /// </summary>
        public static FixedMatrix4x4<TFormat> Identity => new FixedMatrix4x4<TFormat>(new[]
        {
            FixedMath.One<TFormat>(),  FixedMath.Zero<TFormat>(), FixedMath.Zero<TFormat>(), FixedMath.Zero<TFormat>(),
            FixedMath.Zero<TFormat>(), FixedMath.One<TFormat>(),  FixedMath.Zero<TFormat>(), FixedMath.Zero<TFormat>(),
            FixedMath.Zero<TFormat>(), FixedMath.Zero<TFormat>(), FixedMath.One<TFormat>(),  FixedMath.Zero<TFormat>(),
            FixedMath.Zero<TFormat>(), FixedMath.Zero<TFormat>(), FixedMath.Zero<TFormat>(), FixedMath.One<TFormat>()
        });

        /// <summary>
        /// Créée une translation fixe.
        /// </summary>
        public static FixedMatrix4x4<TFormat> Translate(FixedVector3<TFormat> t)
        {
            var e = Identity.m.Clone() as FixedPoint<TFormat>[];
            e[12] = t.x;
            e[13] = t.y;
            e[14] = t.z;
            return new FixedMatrix4x4<TFormat>(e);
        }

        /// <summary>
        /// Créée une mise à l'échelle fixe.
        /// </summary>
        public static FixedMatrix4x4<TFormat> Scale(FixedVector3<TFormat> s)
        {
            var e = Identity.m.Clone() as FixedPoint<TFormat>[];
            e[0] = s.x;
            e[5] = s.y;
            e[10] = s.z;
            return new FixedMatrix4x4<TFormat>(e);
        }

        /// <summary>
        /// Créée une rotation fixe à partir d'un quaternion.
        /// </summary>
        public static FixedMatrix4x4<TFormat> Rotate(FixedQuaternion<TFormat> q)
        {
            var xx = q.x * q.x;
            var yy = q.y * q.y;
            var zz = q.z * q.z;
            var xy = q.x * q.y;
            var xz = q.x * q.z;
            var yz = q.y * q.z;
            var wx = q.w * q.x;
            var wy = q.w * q.y;
            var wz = q.w * q.z;

            return new FixedMatrix4x4<TFormat>(new[]
            {
                FixedMath.One<TFormat>() - (yy + zz), (xy - wz),                     (xz + wy),                     FixedMath.Zero<TFormat>(),
                (xy + wz),                          FixedMath.One<TFormat>() - (xx + zz), (yz - wx),                     FixedMath.Zero<TFormat>(),
                (xz - wy),                          (yz + wx),                     FixedMath.One<TFormat>() - (xx + yy), FixedMath.Zero<TFormat>(),
                FixedMath.Zero<TFormat>(),          FixedMath.Zero<TFormat>(),     FixedMath.Zero<TFormat>(),      FixedMath.One<TFormat>()
            });
        }

        /// <summary>
        /// Multiplication matricielle.
        /// </summary>
        public static FixedMatrix4x4<TFormat> operator *(FixedMatrix4x4<TFormat> a, FixedMatrix4x4<TFormat> b)
        {
            var r = new FixedPoint<TFormat>[16];
            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    var sum = FixedMath.Zero<TFormat>();
                    for (int k = 0; k < 4; k++)
                    {
                        sum += a.m[row * 4 + k] * b.m[k * 4 + col];
                    }
                    r[row * 4 + col] = sum;
                }
            }
            return new FixedMatrix4x4<TFormat>(r);
        }

        /// <summary>
        /// Transforme un point (w=1) par la matrice.
        /// </summary>
        public FixedVector3<TFormat> MultiplyPoint(FixedVector3<TFormat> p)
        {
            var x = m[0] * p.x + m[4] * p.y + m[8] * p.z + m[12];
            var y = m[1] * p.x + m[5] * p.y + m[9] * p.z + m[13];
            var z = m[2] * p.x + m[6] * p.y + m[10] * p.z + m[14];
            return new FixedVector3<TFormat>(x, y, z);
        }

        /// <summary>
        /// Inversion de matrice 4×4 (non implémentée).
        /// </summary>
        public FixedMatrix4x4<TFormat> Inverse()
        {
            // Inversion par Gauss-Jordan pour matrice 4x4 fixe
            // Copie des valeurs dans des tableaux de travail
            var a = new FixedPoint<TFormat>[16];
            var inv = new FixedPoint<TFormat>[16];
            Array.Copy(m, a, 16);
            Array.Copy(Identity.m, inv, 16);

            // Itération sur chaque ligne/pivot
            for (int i = 0; i < 4; i++)
            {
                // Pivot principal
                var pivot = a[i * 4 + i];
                if (pivot.Raw == 0)
                    throw new InvalidOperationException("Matrix is singular and cannot be inverted.");

                // Normalisation de la ligne i
                var invPivot = FixedMath.One<TFormat>() / pivot;
                for (int j = 0; j < 4; j++)
                {
                    a[i * 4 + j] *= invPivot;
                    inv[i * 4 + j] *= invPivot;
                }

                // Élimination sur les autres lignes
                for (int r = 0; r < 4; r++)
                {
                    if (r == i) continue;
                    var factor = a[r * 4 + i];
                    for (int c = 0; c < 4; c++)
                    {
                        a[r * 4 + c] -= factor * a[i * 4 + c];
                        inv[r * 4 + c] -= factor * inv[i * 4 + c];
                    }
                }
            }

            return new FixedMatrix4x4<TFormat>(inv);
        }
    }
}
