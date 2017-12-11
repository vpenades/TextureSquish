using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SixLabors.ImageSharp;

using Vec3 = System.Numerics.Vector3;
using Vec4 = System.Numerics.Vector4;

namespace Epsylon.TextureSquish.UnitTests
{
    
    using IMAGE = SixLabors.ImageSharp.Image<SixLabors.ImageSharp.Rgba32>;

    static class SquishUtils
    {
        public static Bitmap ToSquishImage(this IMAGE image)
        {
            var dst = new Bitmap(image.Width,image.Height);

            for(int y=0; y < dst.Height; ++y)
            {
                for(int x=0; x < dst.Width; ++x)
                {
                    dst[x,y] = image[x, y].Rgba;
                }
            }

            return dst;
        }

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

        public static float CompareToOriginal(this Bitmap a, Bitmap b)
        {
            if (a.Width != b.Width || a.Height != b.Height) throw new ArgumentException("bitmaps must be of same size", nameof(b));

            double error = 0;
            int count = 0;

            for (int i = 0; i < a.Data.Length; i += 4)
            {
                var av = a.Data.GetVec4(i);
                var bv = b.Data.GetVec4(i);

                if (av.W == 0 || bv.W == 0) continue;

                error += Vec4.Abs(bv - av)
                    .GetVec3()
                    .LengthManhattan();

                ++count;
            }

            return (float)(error / count);
        }

        public static IMAGE ToImageSharp(this Bitmap image)
        {
            var dst = new IMAGE(image.Width, image.Height);

            for (int y = 0; y < dst.Height; ++y)
            {
                for (int x = 0; x < dst.Width; ++x)
                {
                    dst[x, y] = new SixLabors.ImageSharp.Rgba32( image[x, y]);
                }
            }

            return dst;
        }

        public static IMAGE SquishImage(this IMAGE srcImage, CompressionMode mode, CompressionOptions options, TestContext context)
        {
            var srcBitmap = srcImage.ToSquishImage();

            

            var blocks = srcBitmap.Compress(mode,options);

            var dstBitmap = Bitmap.Decompress(srcImage.Width, srcImage.Height, blocks, mode);

            var error = (int)(100.0f * dstBitmap.CompareToOriginal(srcBitmap));

            context.WriteLine($"    Error: {error}");
            
            return dstBitmap.ToImageSharp();
        }

        
        
        public static void ProcessFile(string filePath, TestContext context)
        {
            var srcImg = SixLabors.ImageSharp.Image.Load(filePath);

            srcImg.SquishImageWithNvidia(CompressionMode.Dxt1).Save(System.IO.Path.ChangeExtension(filePath, "Dx1-Nvidia.png"));
            srcImg.SquishImageWithNvidia(CompressionMode.Dxt3).Save(System.IO.Path.ChangeExtension(filePath, "Dx3-Nvidia.png"));
            srcImg.SquishImageWithNvidia(CompressionMode.Dxt5).Save(System.IO.Path.ChangeExtension(filePath, "Dx5-Nvidia.png"));

            void process(CompressionMode mode,CompressionOptions options, string ext)
            {
                var dstFileName = System.IO.Path.ChangeExtension(filePath, ext);
                context.WriteLine($"{dstFileName} with {mode}");
                srcImg.SquishImage(mode, options, context).Save(dstFileName);
            }

            var flags = CompressionOptions.ColourRangeFit | CompressionOptions.UseParallelProcessing;            

            process(CompressionMode.Dxt1 , flags, "Dx1-RangeFit.png");
            process(CompressionMode.Dxt3 , flags, "Dx1-RangeFit.png");
            process(CompressionMode.Dxt5 , flags, "Dx1-RangeFit.png");

            flags = CompressionOptions.ColourClusterFit | CompressionOptions.UseParallelProcessing;

            process(CompressionMode.Dxt1 , flags, "Dx1-ClusterFit.png");
            process(CompressionMode.Dxt3 , flags, "Dx1-ClusterFit.png");
            process(CompressionMode.Dxt5 , flags, "Dx1-ClusterFit.png");

            flags = CompressionOptions.ColourIterativeClusterFit | CompressionOptions.UseParallelProcessing;

            process(CompressionMode.Dxt1 , flags, "Dx1-IterClusterFit.png");
            process(CompressionMode.Dxt3 , flags, "Dx1-IterClusterFit.png");
            process(CompressionMode.Dxt5 , flags, "Dx1-IterClusterFit.png");
        }        
    }


    
}
