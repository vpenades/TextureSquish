using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Epsylon.TextureSquish.UnitTests
{
    [TestClass]
    public class ConversionTests
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
        public void ConversionTestNVidiaTextureTool() { _ConversionTest("NVIDIA"); }

        [TestMethod]
        public void ConversionTestSTB() { _ConversionTest("STB"); }

        [TestMethod]
        public void ConversionTestRangeFit() { _ConversionTest("RANGEFIT"); }

        [TestMethod]
        public void ConversionTestClusterFit() { _ConversionTest("CLUSTERFIT"); }

        [TestMethod]
        public void ConversionTestClusterFitIterative() { _ConversionTest("CLUSTERFIT_ITER"); }


        private void _ConversionTest(string method)
        {
            SquishUtils.ProcessFile(method, "TestFiles\\fight_of_thrones_by_orkimides-d6sa500.png",txt=> TestContext.WriteLine(txt));

            SquishUtils.ProcessFile(method, "TestFiles\\squish_test_original.png", txt => TestContext.WriteLine(txt));

            SquishUtils.ProcessFile(method, "TestFiles\\UVGrid1.jpg", txt => TestContext.WriteLine(txt));
            SquishUtils.ProcessFile(method, "TestFiles\\UVGrid2.jpg", txt => TestContext.WriteLine(txt));

            SquishUtils.ProcessFile(method, "TestFiles\\Ivy1.png", txt => TestContext.WriteLine(txt));

            SquishUtils.ProcessFile(method, "TestFiles\\Rainbow_to_alpha_gradient_large.png", txt => TestContext.WriteLine(txt));
            SquishUtils.ProcessFile(method, "TestFiles\\Rainbow_to_alpha_gradient_small.png", txt => TestContext.WriteLine(txt));

            SquishUtils.ProcessFile(method, "TestFiles\\tree.png", txt => TestContext.WriteLine(txt));

            SquishUtils.ProcessFile(method, "TestFiles\\rocks-diffuse.png", txt => TestContext.WriteLine(txt));
            SquishUtils.ProcessFile(method, "TestFiles\\rocks-normals.png", txt => TestContext.WriteLine(txt));
        }

    }
}
