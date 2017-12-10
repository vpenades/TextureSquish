using System;
using System.Collections.Generic;
using System.Text;

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

        public static IMAGE SquishImage(this IMAGE srcImage, CompressionMode flags)
        {
            var blocks = srcImage.ToSquishImage().Compress(flags);

            return Bitmap.Decompress(srcImage.Width, srcImage.Height, blocks, flags).ToImageSharp();
        }

        
        
        public static void ProcessFile(string filePath)
        {
            var srcImg = SixLabors.ImageSharp.Image.Load(filePath);

            srcImg.SquishImageWithNvidia(CompressionMode.Dxt1).Save(System.IO.Path.ChangeExtension(filePath, "Dx1-Nvidia.png"));
            srcImg.SquishImageWithNvidia(CompressionMode.Dxt3).Save(System.IO.Path.ChangeExtension(filePath, "Dx3-Nvidia.png"));
            srcImg.SquishImageWithNvidia(CompressionMode.Dxt5).Save(System.IO.Path.ChangeExtension(filePath, "Dx5-Nvidia.png"));

            CompressionMode flags = CompressionMode.ColourRangeFit | CompressionMode.UseParallelProcessing;

            srcImg.SquishImage(CompressionMode.Dxt1 | flags).Save(System.IO.Path.ChangeExtension(filePath, "Dx1-RangeFit.png"));
            srcImg.SquishImage(CompressionMode.Dxt3 | flags).Save(System.IO.Path.ChangeExtension(filePath, "Dx3-RangeFit.png"));
            srcImg.SquishImage(CompressionMode.Dxt5 | flags).Save(System.IO.Path.ChangeExtension(filePath, "Dx5-RangeFit.png"));

            flags = CompressionMode.ColourClusterFit | CompressionMode.UseParallelProcessing;

            srcImg.SquishImage(CompressionMode.Dxt1 | flags).Save(System.IO.Path.ChangeExtension(filePath, "Dx1-ClusterFit.png"));
            srcImg.SquishImage(CompressionMode.Dxt3 | flags).Save(System.IO.Path.ChangeExtension(filePath, "Dx3-ClusterFit.png"));
            srcImg.SquishImage(CompressionMode.Dxt5 | flags).Save(System.IO.Path.ChangeExtension(filePath, "Dx5-ClusterFit.png"));

            flags = CompressionMode.ColourIterativeClusterFit | CompressionMode.UseParallelProcessing;

            srcImg.SquishImage(CompressionMode.Dxt1 | flags).Save(System.IO.Path.ChangeExtension(filePath, "Dx1-IterClusterFit.png"));
            srcImg.SquishImage(CompressionMode.Dxt3 | flags).Save(System.IO.Path.ChangeExtension(filePath, "Dx3-IterClusterFit.png"));
            srcImg.SquishImage(CompressionMode.Dxt5 | flags).Save(System.IO.Path.ChangeExtension(filePath, "Dx5-IterClusterFit.png"));            
        }        
    }


    
}
