using System.IO;


/*
//----- MSTest Framework
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TESTCLASS = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
using TESTMETHOD = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
using TESTMETHODEX = Microsoft.VisualStudio.TestTools.UnitTesting.DataTestMethodAttribute;
using TESTPARAMS = Microsoft.VisualStudio.TestTools.UnitTesting.DataRowAttribute;
using TESTCONTEXT = Microsoft.VisualStudio.TestTools.UnitTesting.TestContext;
*/

//----- NUnit Framework
using NUnit.Framework;
using TESTCLASS = NUnit.Framework.TestFixtureAttribute;
using TESTMETHOD = NUnit.Framework.TestAttribute;
using TESTMETHODEX = NUnit.Framework.TestAttribute;
using TESTPARAMS = NUnit.Framework.TestCaseAttribute;
using TESTCONTEXT = NUnit.Framework.TestContext;

namespace Epsylon.TextureSquish.UnitTests
{
    [TESTCLASS]
    public class ConversionTests
    {
        /// <summary>
        ///  Gets or sets the test context which provides
        ///  information about and functionality for the current test run.
        ///</summary>

        public TestContext TestContext => NUnit.Framework.TestContext.CurrentContext;
        //public TestContext TestContext { get; set; }

        [TESTMETHOD]
        public void ConversionTestNVidiaTextureTool() { _ConversionTest("NVIDIA"); }

        [TESTMETHOD]
        public void ConversionTestSTB() { _ConversionTest("STB"); }

        [TESTMETHOD]
        public void ConversionTestRangeFit() { _ConversionTest("RANGEFIT"); }

        [TESTMETHOD]
        public void ConversionTestClusterFit() { _ConversionTest("CLUSTERFIT"); }

        [TESTMETHOD]
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
