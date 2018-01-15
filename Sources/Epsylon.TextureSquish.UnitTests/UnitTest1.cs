using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Epsylon.TextureSquish.UnitTests
{
    [TestClass]
    public class UnitTest1
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

        [DataTestMethod]
        [DataRow("STB")]
        [DataRow("NVIDIA")]
        [DataRow("RANGEFIT")]
        [DataRow("CLUSTERFIT")]
        [DataRow("CLUSTERFIT_ALT")]
        [DataRow("CLUSTERFIT_ITER")]        
        public void ConversionTest1(string method)
        {
            SquishUtils.ProcessFile(method,"TestFiles\\fight_of_thrones_by_orkimides-d6sa500.png", TestContext);

            SquishUtils.ProcessFile(method,"TestFiles\\squish_test_original.png", TestContext);            

            SquishUtils.ProcessFile(method,"TestFiles\\UVGrid1.jpg", TestContext);
            SquishUtils.ProcessFile(method,"TestFiles\\UVGrid2.jpg", TestContext);

            SquishUtils.ProcessFile(method,"TestFiles\\Ivy1.png", TestContext);

            SquishUtils.ProcessFile(method,"TestFiles\\Rainbow_to_alpha_gradient_large.png", TestContext);
            SquishUtils.ProcessFile(method,"TestFiles\\Rainbow_to_alpha_gradient_small.png", TestContext);            
        }

        [TestMethod]
        public void PerformanceTest1()
        {
            var srcImg = SixLabors.ImageSharp.Image.Load("TestFiles\\Rainbow_to_alpha_gradient_large.png");
            var toSquish = srcImg.ToSquishImage();
            StbSharp.Image image;
            using (var stream = File.OpenRead("TestFiles\\Rainbow_to_alpha_gradient_large.png"))
                image = new StbSharp.ImageReader().Read(stream);

            var watch = new System.Diagnostics.Stopwatch();

            watch.Restart();
            for(int i=0; i < 100; ++i)
            {
                toSquish.Compress(CompressionMode.Dxt5 , CompressionOptions.ColourIterativeClusterFit | CompressionOptions.UseParallelProcessing);
            }
            watch.Stop();
            var netTime = watch.Elapsed;

            TestContext.WriteLine($"ClusterFit Iterative time: {netTime}");

            watch.Restart();
            for (int i = 0; i < 100; ++i)
            {
                toSquish.Compress(CompressionMode.Dxt5, CompressionOptions.ColourClusterFitAlt | CompressionOptions.UseParallelProcessing);
            }
            watch.Stop();
            netTime = watch.Elapsed;

            TestContext.WriteLine($"ClusterFit Alt time: {netTime}");

            watch.Restart();
            for (int i = 0; i < 100; ++i)
            {
                StbSharp.Stb.stb_compress_dxt(image, true);
            }
            watch.Stop();
            var stbTime = watch.Elapsed;

            TestContext.WriteLine($"Stb time: {stbTime}");

            watch.Restart();
            for (int i = 0; i < 100; ++i)
            {
                StbSharp.Stb.stb_compress_dxt(image, true, 2);
            }
            watch.Stop();
            var stbHqTime = watch.Elapsed;

            TestContext.WriteLine($"Stb HQ time: {stbHqTime}");

            watch.Restart();
            for (int i = 0; i < 100; ++i)
            {
                toSquish.CompressWithNvidia(CompressionMode.Dxt5);
            }
            watch.Stop();
            var nvidiaTime = watch.Elapsed;

            TestContext.WriteLine($"NVidia time: {nvidiaTime}");
        }

    }
}
