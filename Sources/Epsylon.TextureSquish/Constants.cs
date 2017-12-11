using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Epsylon.TextureSquish
{
    
    public enum CompressionMode
    {
        /// <summary>
        /// Use DXT1 compression.
        /// </summary>
        Dxt1 = 1,

        /// <summary>
        /// Use DXT3 compression.
        /// </summary>
        Dxt3 = 2,

        /// <summary>
        /// Use DXT5 compression.
        /// </summary>
        Dxt5 = 4,        
    }

    [Flags]
    public enum CompressionOptions
    {
        /// <summary>
        /// Use a fast but low quality colour compressor.
        /// </summary>
        ColourRangeFit = 16,

        /// <summary>
        /// Use a slow but high quality colour compressor (the default).
        /// </summary>
        ColourClusterFit = 32,

        /// <summary>
        /// Use a very slow but very high quality colour compressor.
        /// </summary>
        ColourIterativeClusterFit = 64,

        /// <summary>
        /// Use a perceptual metric for colour error (the default).
        /// </summary>
        ColourMetricPerceptual = 256,

        /// <summary>
        /// Use a uniform metric for colour error.
        /// </summary>
        ColourMetricUniform = 512,

        /// <summary>
        /// Weight the colour by alpha during cluster fit (disabled by default).
        /// </summary>
        /// <remarks>
        /// When doing Cluster compression, the kernel checks the most commonly
        /// used colors of each block, given more weight to colors more commonly
        /// used. Given that semitransparent pixels might be less important than
        /// opaque pixels, we can weight in this behavior. The ideal usage of
        /// this flag is for processing sprites with semitransparent edges.
        /// This is NOT AlphaPremultiply.
        /// </remarks>
        WeightColourByAlpha = 1024,

        /// <summary>
        /// Uses multithreading to increase compression speed.
        /// </summary>
        UseParallelProcessing = 2048,
    }

    static class ConstantsExtensions
    {
        public static CompressionOptions FixFlags(this CompressionOptions flags)
        {
            // grab the flag bits            
            var fit = flags & (CompressionOptions.ColourIterativeClusterFit | CompressionOptions.ColourClusterFit | CompressionOptions.ColourRangeFit);
            var metric = flags & (CompressionOptions.ColourMetricPerceptual | CompressionOptions.ColourMetricUniform);
            var extra = flags & (CompressionOptions.WeightColourByAlpha | CompressionOptions.UseParallelProcessing);

            // set defaults            
            if (fit == 0) fit = CompressionOptions.ColourClusterFit;
            if (metric == 0) metric = CompressionOptions.ColourMetricPerceptual;

            // done
            return fit | metric | extra;
        }        
    }
}
