using System;
using System.Reflection;

using BenchmarkDotNet.Running;

namespace Epsylon.TextureSquish.BenchMarks
{
    class Program
    {
        public static void Main(string[] args)
        {
            new BenchmarkSwitcher(typeof(Program).GetTypeInfo().Assembly).Run(args);
        }
    }
}
