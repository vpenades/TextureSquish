using System.IO;

using NUnit.Framework;

namespace Epsylon.TextureSquish.UnitTests
{
    
    public class ConversionTests
    {
        /// <summary>
        ///  Gets or sets the test context which provides
        ///  information about and functionality for the current test run.
        ///</summary>

        public TestContext TestContext => NUnit.Framework.TestContext.CurrentContext;
        //public TestContext TestContext { get; set; }

        [Test]
        public void ConversionTestNVidiaTextureTool() { _ConversionTest("NVIDIA"); }

        [Test]
        public void ConversionTestSTB() { _ConversionTest("STB"); }

        [Test]
        public void ConversionTestRangeFit() { _ConversionTest("RANGEFIT"); }

        [Test]
        public void ConversionTestClusterFit() { _ConversionTest("CLUSTERFIT"); }

        [Test]
        public void ConversionTestClusterFitIterative() { _ConversionTest("CLUSTERFIT_ITER"); }


        private void _ConversionTest(string method)
        {
            var files = new string[]
            {
                "fight_of_thrones_by_orkimides-d6sa500.png",
                "squish_test_original.png",
                "UVGrid1.jpg",
                "UVGrid2.jpg",
                "Ivy1.png",
                "Rainbow_to_alpha_gradient_large.png",
                "Rainbow_to_alpha_gradient_small.png",
                "tree.png",
                "rocks-diffuse.png",
                "rocks-normals.png"
            };

            foreach(var f in files)
            {
                System.Console.WriteLine(f);

                var ff = System.IO.Path.Combine("TestFiles", f);

                SquishUtils.ProcessFile(method, ff, txt => TestContext.WriteLine(txt), dstfn => TestContext.AddTestAttachment(dstfn));

                TestContext.WriteLine(string.Empty);
            }
        }

    }
}
