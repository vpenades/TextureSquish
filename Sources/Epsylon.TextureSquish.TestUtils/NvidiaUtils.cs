using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

using IMAGE = SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>;

namespace Epsylon.TextureSquish
{
    public static class NvidiaUtils
    {
        public static void BGRAtoRGBA(byte[] data)
        {
            for (var x = 0; x < data.Length; x += 4)
            {
                data[x] ^= data[x + 2];
                data[x + 2] ^= data[x];
                data[x] ^= data[x + 2];
            }
        }

        public static IMAGE SquishImageWithNvidia(this IMAGE srcImage, CompressionMode mode, Action<string> logger)
        {
            var srcBitmap = srcImage.ToSquishImage();

            var blocks = srcBitmap.CompressWithNvidia(mode);

            var dstBitmap = Bitmap.Decompress(srcImage.Width, srcImage.Height, blocks, mode);

            return dstBitmap.ToImageSharp();
        }


        public static Byte[] CompressWithNvidia(this Bitmap srcImage, CompressionMode mode)
        {
            // System.Diagnostics.Debug.Assert(IntPtr.Size == 8, "nvtt.dll(x64) requires x64 runtime");

            srcImage = srcImage.Clone();
            srcImage.SwapElements(2, 1, 0, 3);

            using (var ddsCompressor = new TeximpNet.Compression.Compressor())
            {
                var inputOptions = ddsCompressor.Input;
                inputOptions.SetTextureLayout(TeximpNet.Compression.TextureType.Texture2D, srcImage.Width, srcImage.Height, 1);
                inputOptions.SetMipmapGeneration(false);
                inputOptions.SetGamma(1.0f, 1.0f);
                inputOptions.AlphaMode = (mode & CompressionMode.Dxt1) == 0
                    ? TeximpNet.Compression.AlphaMode.Premultiplied
                    : TeximpNet.Compression.AlphaMode.None;

                var compressionOptions = ddsCompressor.Compression;
                compressionOptions.Quality = TeximpNet.Compression.CompressionQuality.Normal;
                if ((mode & CompressionMode.Dxt1) != 0) compressionOptions.Format = TeximpNet.Compression.CompressionFormat.DXT1;
                if ((mode & CompressionMode.Dxt3) != 0) compressionOptions.Format = TeximpNet.Compression.CompressionFormat.DXT3;
                if ((mode & CompressionMode.Dxt5) != 0) compressionOptions.Format = TeximpNet.Compression.CompressionFormat.DXT5;

                return Compress(srcImage.Data, srcImage.Width, srcImage.Height, ddsCompressor);
            }                    
        }

        private static unsafe Byte[] Compress(Byte[] data, int width, int height, TeximpNet.Compression.Compressor ddsCompressor)
        {
            var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);

            try
            {
                var dataPtr = dataHandle.AddrOfPinnedObject();
                var mipData = new TeximpNet.DDS.MipData(width, height, width * 4, dataPtr, false);

                ddsCompressor.Input.SetMipmapData(mipData, true);
                ddsCompressor.Output.OutputHeader = false;

                if (ddsCompressor.Process(out var compressedImages))
                {
                    var outData = compressedImages.MipChains[0][0];

                    var outSpan = new Span<Byte>((Byte*)outData.Data, outData.SizeInBytes);

                    return outSpan.ToArray();
                }

                throw new Exception(ddsCompressor.LastErrorString);

            }
            finally
            {
                dataHandle.Free();
            }
        }

    }
}
