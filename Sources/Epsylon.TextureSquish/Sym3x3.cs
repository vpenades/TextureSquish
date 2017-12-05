using System;
using System.Collections.Generic;
using System.Text;

using Vec3 = System.Numerics.Vector3;
using Vec4 = System.Numerics.Vector4;

namespace Epsylon.TextureSquish
{
    struct Sym3x3
    {
        public Sym3x3(float s)
        {
            m_x = new float[6];

            for (int i = 0; i < 6; ++i) m_x[i] = s;
        }

        public float this[int index]
        {
            get { return m_x[index]; }
            set { m_x[index] = value; }
        }

        private readonly float[] m_x;

        public static Sym3x3 ComputeWeightedCovariance(int n, Vec3[] points, float[] weights)
        {
            // compute the centroid
            float total = 0.0f;
            var centroid = Vec3.Zero;

            for (int i = 0; i < n; ++i)
            {
                total += weights[i];
                centroid += weights[i] * points[i];
            }
            if (total > float.Epsilon) centroid /= total;

            // accumulate the covariance matrix
            var covariance = new Sym3x3( 0 );
            for (int i = 0; i < n; ++i)
            {
                Vec3 a = points[i] - centroid;
                Vec3 b = weights[i] * a;

                covariance[0] += a.X * b.X;
                covariance[1] += a.X * b.Y;
                covariance[2] += a.X * b.Z;
                covariance[3] += a.Y * b.Y;
                covariance[4] += a.Y * b.Z;
                covariance[5] += a.Z * b.Z;
            }

            // return it
            return covariance;
        }

        public static Vec3 ComputePrincipleComponent(Sym3x3 matrix)
        {
            Vec4 row0 = new Vec4(matrix[0], matrix[1], matrix[2], 0.0f);
            Vec4 row1 = new Vec4(matrix[1], matrix[3], matrix[4], 0.0f);
            Vec4 row2 = new Vec4(matrix[2], matrix[4], matrix[5], 0.0f);
            Vec4 v = new Vec4(1.0f);
            for (int i = 0; i < 8; ++i)
            {
                // matrix multiply
                Vec4 w = row0 * v.SplatX();
                w = row1.MultiplyAdd(v.SplatY(), w);
                w = row2.MultiplyAdd(v.SplatZ(), w);

                // get max component from xyz in all channels
                Vec4 a = Vec4.Max(w.SplatX(), Vec4.Max(w.SplatY(), w.SplatZ()));

                // divide through and advance
                v = w * a.Reciprocal();
            }

            return v.GetVec3();
        }

        public static Vec3 GetMultiplicity1Evector(Sym3x3 matrix, float evalue)
        {
            // compute M
            var m = new Sym3x3(0);
            m[0] = matrix[0] - evalue;
            m[1] = matrix[1];
            m[2] = matrix[2];
            m[3] = matrix[3] - evalue;
            m[4] = matrix[4];
            m[5] = matrix[5] - evalue;

            // compute U
            var u = new Sym3x3(0);
            u[0] = m[3] * m[5] - m[4] * m[4];
            u[1] = m[2] * m[4] - m[1] * m[5];
            u[2] = m[1] * m[4] - m[2] * m[3];
            u[3] = m[0] * m[5] - m[2] * m[2];
            u[4] = m[1] * m[2] - m[4] * m[0];
            u[5] = m[0] * m[3] - m[1] * m[1];

            // find the lVec3est component
            float mc = Math.Abs(u[0]);
            int mi = 0;
            for (int i = 1; i < 6; ++i)
            {
                float c = Math.Abs(u[i]);
                if (c > mc)
                {
                    mc = c;
                    mi = i;
                }
            }

            // pick the column with this component
            switch (mi)
            {
                case 0:  return new Vec3(u[0], u[1], u[2]);
                case 1:
                case 3:  return new Vec3(u[1], u[3], u[4]);
                default: return new Vec3(u[2], u[4], u[5]);
            }
        }

        public static Vec3 GetMultiplicity2Evector(Sym3x3 matrix, float evalue)
        {
            // compute M
            var m = new Sym3x3(0);
            m[0] = matrix[0] - evalue;
            m[1] = matrix[1];
            m[2] = matrix[2];
            m[3] = matrix[3] - evalue;
            m[4] = matrix[4];
            m[5] = matrix[5] - evalue;

            // find the lVec3est component
            float mc = Math.Abs(m[0]);
            int mi = 0;

            for (int i = 1; i < 6; ++i)
            {
                float c = Math.Abs(m[i]);
                if (c > mc)
                {
                    mc = c;
                    mi = i;
                }
            }

            // pick the first eigenvector based on this index
            switch (mi)
            {
                case 0:
                case 1: return new Vec3(-m[1], m[0], 0.0f);
                case 2: return new Vec3(m[2], 0.0f, -m[0]);
                case 3:
                case 4: return new Vec3(0.0f, -m[4], m[3]);
                default: return new Vec3(0.0f, -m[5], m[4]);
            }
        }        
    }
}
