
using System;
using System.Collections.Generic;
using System.Text;

using BenchmarkDotNet.Attributes;

namespace Epsylon.TextureSquish.BenchMarks
{

    [RPlotExporter, RankColumn]
    [SimpleJob(launchCount: 1, warmupCount: 5, targetCount: 5)]
    public class Compression
    {
        private readonly Dictionary<String, Bitmap> _Bitmaps = new Dictionary<String, Bitmap>();                

        [GlobalSetup]
        public void ReadImages()
        {
            foreach(var key in ImageNames)
            {
                var bmp = SixLabors.ImageSharp.Image.Load<SixLabors.ImageSharp.PixelFormats.Rgba32>(key).ToSquishImage();

                _Bitmaps[key] = bmp;
            }            
        }

        public IEnumerable<String> ImageNames
        {
            get
            {
                yield return "TestFiles\\rocks-diffuse.png";
                // yield return "TestFiles\\rocks-normals.png";
                // yield return "TestFiles\\tree.png";
                // yield return "TestFiles\\Ivy1.png";
                // yield return "TestFiles\\Rainbow_to_alpha_gradient_large.png";
                // yield return "TestFiles\\UVGrid1.jpg";
                yield return "TestFiles\\fight_of_thrones_by_orkimides-d6sa500.png";
            }
        }

        [ParamsSource(nameof(ImageNames))]
        public String ImageName;

        [Benchmark(Baseline = true, Description = "NVidia Texture Tool")]
        public void NVidiaCompression()
        {
            var bmp = _Bitmaps[ImageName];
            
            bmp.CompressWithNvidia(CompressionMode.Dxt1);
            
        }

        [Benchmark(Description = "TextureSquish Iterative Cluster Fit")]
        public void TexSquishIterClusterFitCompression()
        {
            var bmp = _Bitmaps[ImageName];

            bmp.Compress(CompressionMode.Dxt1, CompressionOptions.ColourClusterFit);            
        }
    }
}
