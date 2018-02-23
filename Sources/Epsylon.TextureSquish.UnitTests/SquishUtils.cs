using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SixLabors.ImageSharp;

using StbSharp;

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
                
        public static void ProcessFile(string method, string filePath, TestContext context)
        {
            var srcImg = SixLabors.ImageSharp.Image.Load(filePath);            

            void processSTB(CompressionMode mode, bool useAlpha, string ext)
            {
                var dstFileName = System.IO.Path.ChangeExtension(filePath, ext);
                context.WriteLine($"{dstFileName} with {mode}");
                
                using (var stream = System.IO.File.OpenRead(filePath))
                {
                    var srcImage = new StbSharp.ImageReader().Read(stream);

                    if (srcImage.Comp != 4) return; // TODO: should convert RGB to RGBA

                    // flags bits:
                    // 1 = dither
                    // 2 = refine count (1 or 2)                    

                    var bytes = StbDxt.stb_compress_dxt(srcImage, useAlpha, 2);                    

                    var dstImage = Bitmap
                        .Decompress(srcImage.Width, srcImage.Height, bytes, mode)
                        .ToImageSharp();

                    dstImage.Save(dstFileName);
                }
            }

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

            if (method == "STB")
            {
                processSTB(CompressionMode.Dxt1, false, "Dx1-STB-NoAlpha.png");
                processSTB(CompressionMode.Dxt1, true, "Dx1-STB.png");
                processSTB(CompressionMode.Dxt3, true, "Dx3-STB.png");
                processSTB(CompressionMode.Dxt5, true, "Dx5-STB.png");
                return;
            }

            if (method == "NVIDIA")
            {
                processNVidia(CompressionMode.Dxt1, "Dx1-NVidia.png");
                processNVidia(CompressionMode.Dxt3, "Dx3-NVidia.png");
                processNVidia(CompressionMode.Dxt5, "Dx5-NVidia.png");
                return;
            }                

            #if DEBUG
            var xflags = CompressionOptions.None;
            #else
            var xflags = CompressionOptions.UseParallelProcessing | CompressionOptions.None;
            #endif

            if (method == "RANGEFIT")
            {
                var flags = xflags | CompressionOptions.ColourRangeFit;

                processSquish(CompressionMode.Dxt1, flags, "Dx1-RangeFit.png");
                processSquish(CompressionMode.Dxt3, flags, "Dx3-RangeFit.png");
                processSquish(CompressionMode.Dxt5, flags, "Dx5-RangeFit.png");
                return;
            }

            if (method == "CLUSTERFIT")
            {
                var flags = xflags | CompressionOptions.ColourClusterFit;

                processSquish(CompressionMode.Dxt1, flags, "Dx1-ClusterFit.png");
                processSquish(CompressionMode.Dxt3, flags, "Dx3-ClusterFit.png");
                processSquish(CompressionMode.Dxt5, flags, "Dx5-ClusterFit.png");
                return;
            }

            if (method == "CLUSTERFIT_ALT")
            {
                var flags = xflags | CompressionOptions.ColourClusterFit;

                processSquish(CompressionMode.Dxt1, flags, "Dx1-ClusterFitAlt.png");
                processSquish(CompressionMode.Dxt3, flags, "Dx3-ClusterFitAlt.png");
                processSquish(CompressionMode.Dxt5, flags, "Dx5-ClusterFitAlt.png");
                return;
            }

            if (method == "CLUSTERFIT_ITER")
            {
                var flags = xflags | CompressionOptions.ColourClusterFit;

                processSquish(CompressionMode.Dxt1, flags, "Dx1-ClusterFitIter.png");
                processSquish(CompressionMode.Dxt3, flags, "Dx3-ClusterFitIter.png");
                processSquish(CompressionMode.Dxt5, flags, "Dx5-ClusterFitIter.png");
                return;
            }
        }        
    }


    
}
