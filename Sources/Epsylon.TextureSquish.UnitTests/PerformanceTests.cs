using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using StbSharp;

namespace Epsylon.TextureSquish.UnitTests
{
    [TestClass]
    public class PerformanceTests
    {
        private TestContext testContextInstance;

        /// <summary>
        ///  Gets or sets the test context which provides
        ///  information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }


        [TestMethod]
        public void PerformanceTestRaibow()
        {
            _PerformanceTest(TestContext, "TestFiles\\Rainbow_to_alpha_gradient_large.png");            
        }

        [TestMethod]
        public void PerformanceTestSprites()
        {            
            _PerformanceTest(TestContext, "TestFiles\\fight_of_thrones_by_orkimides-d6sa500.png");            
        }

        [TestMethod]
        public void PerformanceTestUVGrid()
        {            
            _PerformanceTest(TestContext, "TestFiles\\UVGrid1.jpg");            
        }

        [TestMethod]
        public void PerformanceTestIvy()
        {            
            _PerformanceTest(TestContext, "TestFiles\\ivy1.png");
        }

        [TestMethod]
        public void PerformanceTestWorstCase()
        {
            var imagePath = "TestFiles\\ivy1.png";

            var srcImg = SixLabors.ImageSharp.Image.Load(imagePath);
            var squishImg = srcImg.ToSquishImage();

            var benchmark = new BenchMark() { DefaultRepetitions = 30 };

            benchmark.Repeat("Dxt1", () =>
            {
                squishImg.Compress(CompressionMode.Dxt1, CompressionOptions.ColourIterativeClusterFit | CompressionOptions.UseParallelProcessing);                
            });

            benchmark.Repeat("Dxt3", () =>
            {
                squishImg.Compress(CompressionMode.Dxt3, CompressionOptions.ColourIterativeClusterFit | CompressionOptions.UseParallelProcessing);
            });

            benchmark.Repeat("Dxt5", () =>
            {
                squishImg.Compress(CompressionMode.Dxt5, CompressionOptions.ColourIterativeClusterFit | CompressionOptions.UseParallelProcessing);
            });

            TestContext.WriteLine(benchmark.ToString());
        }


        private static void _PerformanceTest(TestContext context, string imagePath)
        {
            context.WriteLine($"Using Image: {imagePath}");

            var srcImg = SixLabors.ImageSharp.Image.Load(imagePath);
            var squishImg = srcImg.ToSquishImage();

            var stbImg = LoadStbImage(imagePath);

            var benchmark = new BenchMark() { DefaultRepetitions = 100 };

            if (stbImg.Comp == 4)
            {
                benchmark.Repeat("STB", () => StbDxt.stb_compress_dxt(stbImg, true));

                benchmark.Repeat("STB HQ", () => StbDxt.stb_compress_dxt(stbImg, true, 2));
            }

            benchmark.Repeat("IterativeClusterFit", () =>
            {
                squishImg.Compress(CompressionMode.Dxt5, CompressionOptions.ColourIterativeClusterFit | CompressionOptions.UseParallelProcessing);
            });

            benchmark.Repeat("IterativeClusterFitAlt ", () =>
            {
                squishImg.Compress(CompressionMode.Dxt5, CompressionOptions.ColourClusterFitAlt | CompressionOptions.UseParallelProcessing);
            });

            benchmark.Repeat("NVidia", () => squishImg.CompressWithNvidia(CompressionMode.Dxt5) );
           
            context.WriteLine( benchmark.ToString() );

            var icf = benchmark.GetReport("IterativeClusterFit").TotalTime.TotalSeconds;
            var nvt = benchmark.GetReport("NVidia").TotalTime.TotalSeconds;

            context.WriteLine($"IterativeClusterFit is {icf/nvt} slower tha nVidia");

            var csv = benchmark.ToCSV();

            System.IO.File.WriteAllText(imagePath + ".csv", csv);
        }

        private static StbSharp.Image LoadStbImage(string imagePath)
        {
            using (var stream = File.OpenRead(imagePath))
            {
                return new StbSharp.ImageReader().Read(stream);
            }
        }        
    }
}
