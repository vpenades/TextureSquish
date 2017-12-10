using Microsoft.VisualStudio.TestTools.UnitTesting;

using SixLabors.ImageSharp;

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

        [TestMethod]
        public void ConversionTest1()
        {
            SquishUtils.ProcessFile("TestFiles\\squish_test_original.png");            

            SquishUtils.ProcessFile("TestFiles\\UVGrid1.jpg");
            SquishUtils.ProcessFile("TestFiles\\UVGrid2.jpg");

            SquishUtils.ProcessFile("TestFiles\\Ivy1.png");

            SquishUtils.ProcessFile("TestFiles\\Rainbow_to_alpha_gradient_large.png");
            SquishUtils.ProcessFile("TestFiles\\Rainbow_to_alpha_gradient_small.png");

            SquishUtils.ProcessFile("TestFiles\\fight_of_thrones_by_orkimides-d6sa500.png");            
        }

        [TestMethod]
        public void PerformanceTest1()
        {
            var srcImg = SixLabors.ImageSharp.Image.Load("TestFiles\\Rainbow_to_alpha_gradient_large.png");

            var toSquish = srcImg.ToSquishImage();

            var watch = new System.Diagnostics.Stopwatch();

            watch.Restart();
            for(int i=0; i < 100; ++i)
            {
                toSquish.Compress(CompressionMode.Dxt5 | CompressionMode.ColourIterativeClusterFit | CompressionMode.UseParallelProcessing);
            }
            watch.Stop();
            var netTime = watch.Elapsed;

            TestContext.WriteLine($"NET time: {netTime}");

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
