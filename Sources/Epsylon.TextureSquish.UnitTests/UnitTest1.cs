using Microsoft.VisualStudio.TestTools.UnitTesting;

using SixLabors.ImageSharp;

namespace Epsylon.TextureSquish.UnitTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            Utils.ProcessFile("TestFiles\\squish_test_original.png");

            Utils.ProcessFile("TestFiles\\UVGrid1.jpg");
            Utils.ProcessFile("TestFiles\\UVGrid2.jpg");

            Utils.ProcessFile("TestFiles\\Ivy1.png");

            Utils.ProcessFile("TestFiles\\Rainbow_to_alpha_gradient_large.png");
            Utils.ProcessFile("TestFiles\\Rainbow_to_alpha_gradient_small.png");

            Utils.ProcessFile("TestFiles\\fight_of_thrones_by_orkimides-d6sa500.png");            
        }

    }
}
