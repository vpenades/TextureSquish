using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SixLabors.ImageSharp;

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

            context.WriteLine(dstBitmap.CompareRGBToOriginal(srcBitmap).ToString());
            
            return dstBitmap.ToImageSharp();
        }
                
        public static void ProcessFile(string filePath, TestContext context)
        {
            var srcImg = SixLabors.ImageSharp.Image.Load(filePath);

            void processNVidia(CompressionMode mode, string ext)
            {
                var dstFileName = System.IO.Path.ChangeExtension(filePath, ext);
                context.WriteLine($"{dstFileName} with {mode}");
                srcImg.SquishImageWithNvidia(mode, context).Save(dstFileName);
            }

            void processSquish(CompressionMode mode, CompressionOptions options, string ext)
            {
                var dstFileName = System.IO.Path.ChangeExtension(filePath, ext);
                context.WriteLine($"{dstFileName} with {mode}");
                srcImg.SquishImage(mode, options, context).Save(dstFileName);
            }            

            processNVidia(CompressionMode.Dxt1, "Dx1-NVidia.png");
            processNVidia(CompressionMode.Dxt3, "Dx3-NVidia.png");
            processNVidia(CompressionMode.Dxt5, "Dx5-NVidia.png");

            var flags = CompressionOptions.ColourRangeFit | CompressionOptions.UseParallelProcessing | CompressionOptions.WeightColourByAlpha;

            processSquish(CompressionMode.Dxt1 , flags, "Dx1-RangeFit.png");
            processSquish(CompressionMode.Dxt3 , flags, "Dx3-RangeFit.png");
            processSquish(CompressionMode.Dxt5 , flags, "Dx5-RangeFit.png");

            flags = CompressionOptions.ColourClusterFit | CompressionOptions.UseParallelProcessing | CompressionOptions.WeightColourByAlpha;

            processSquish(CompressionMode.Dxt1 , flags, "Dx1-ClusterFit.png");
            processSquish(CompressionMode.Dxt3 , flags, "Dx3-ClusterFit.png");
            processSquish(CompressionMode.Dxt5 , flags, "Dx5-ClusterFit.png");

            flags = CompressionOptions.ColourIterativeClusterFit | CompressionOptions.UseParallelProcessing | CompressionOptions.WeightColourByAlpha;

            processSquish(CompressionMode.Dxt1 , flags, "Dx1-IterClusterFit.png");
            processSquish(CompressionMode.Dxt3 , flags, "Dx3-IterClusterFit.png");
            processSquish(CompressionMode.Dxt5 , flags, "Dx5-IterClusterFit.png");
        }        
    }


    
}
