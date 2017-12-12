using System;
using System.Collections.Generic;
using System.Text;

using MathNet.Numerics.Statistics;

using Vec3 = System.Numerics.Vector3;
using Vec4 = System.Numerics.Vector4;

namespace Epsylon.TextureSquish.UnitTests
{
    static class BitmapCompareUtils
    {
        public static Vec3 GetVec3(this Vec4 v) { return new Vec3(v.X, v.Y, v.Z); }

        public static Vec4 GetVec4(this Byte[] array, int startIndex)
        {
            var x = array[startIndex + 0];
            var y = array[startIndex + 1];
            var z = array[startIndex + 2];
            var w = array[startIndex + 3];

            return new Vec4((float)x / 255.0f, (float)y / 255.0f, (float)z / 255.0f, (float)w / 255.0f);
        }

        public static float LengthManhattan(this Vec3 v)
        {
            return Math.Abs(v.X) + Math.Abs(v.Y) + Math.Abs(v.Z);
        }

        public static float[] GetRGBManhattanLengthTo(this Bitmap a, Bitmap b)
        {
            if (a.Width != b.Width || a.Height != b.Height) throw new ArgumentException("bitmaps must be of same size", nameof(b));

            var mlenghs = new List<float>();

            for (int i = 0; i < a.Data.Length; i += 4)
            {
                var av = a.Data.GetVec4(i);
                var bv = b.Data.GetVec4(i);

                bool isTransparent = av.W == 0 || bv.W == 0;

                var rgb = isTransparent ? 0.0f : Vec4.Abs(bv - av)
                    .GetVec3()
                    .LengthManhattan();

                mlenghs.Add(rgb);
            }

            return mlenghs.ToArray();
        }

        public static BitmapCompareResult CompareRGBToOriginal(this Bitmap a, Bitmap b)
        {
            var mlength = a.GetRGBManhattanLengthTo(b);

            var r = new BitmapCompareResult
            {
                StandardDeviation = mlength.StandardDeviation(),
                Maximum = mlength.MaximumAbsolute(),
                Median = mlength.Median()
            };

            return r;
        }
    }

    public struct BitmapCompareResult
    {
        public double Median;
        public double Maximum;
        public double StandardDeviation;

        public override string ToString()
        {
            return $"Median:{Median:0.###} Max:{Maximum:0.###} Std:{StandardDeviation:0.###}";
        }
    }
}
