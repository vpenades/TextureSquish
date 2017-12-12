using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

using MathNet.Numerics.Statistics;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using IMAGE = SixLabors.ImageSharp.Image<SixLabors.ImageSharp.Rgba32>;

namespace Epsylon.TextureSquish.UnitTests
{
    static class NvidiaUtils
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

        public static Byte[] CompressWithNvidia(this Bitmap srcImage, CompressionMode flags)
        {
            Assert.AreEqual(8, IntPtr.Size, "nvtt.dll(x64) requires x64 runtime");

            var dxtCompressor = new Nvidia.TextureTools.Compressor();
            var inputOptions = new Nvidia.TextureTools.InputOptions();
            if ((flags & CompressionMode.Dxt1) == 0) inputOptions.SetAlphaMode(Nvidia.TextureTools.AlphaMode.Premultiplied);
            else inputOptions.SetAlphaMode(Nvidia.TextureTools.AlphaMode.None);

            inputOptions.SetTextureLayout(Nvidia.TextureTools.TextureType.Texture2D, srcImage.Width, srcImage.Height, 1);

            Nvidia.TextureTools.Format outputFormat = Nvidia.TextureTools.Format.DXT1;

            if ((flags & CompressionMode.Dxt3) != 0) outputFormat = Nvidia.TextureTools.Format.DXT3;
            if ((flags & CompressionMode.Dxt5) != 0) outputFormat = Nvidia.TextureTools.Format.DXT5;

            srcImage = srcImage.Clone();

            srcImage.SwapElements(2, 1, 0, 3);

            var dataHandle = GCHandle.Alloc(srcImage.Data, GCHandleType.Pinned);
            try
            {
                var dataPtr = dataHandle.AddrOfPinnedObject();

                inputOptions.SetMipmapData(dataPtr, srcImage.Width, srcImage.Height, 1, 0, 0);
                inputOptions.SetMipmapGeneration(false);
                inputOptions.SetGamma(1.0f, 1.0f);

                var compressionOptions = new Nvidia.TextureTools.CompressionOptions();
                compressionOptions.SetFormat(outputFormat);
                compressionOptions.SetQuality(Nvidia.TextureTools.Quality.Normal);

                var outputOptions = new Nvidia.TextureTools.OutputOptions();
                outputOptions.SetOutputHeader(false);

                using (var dataHandler = new DxtDataHandler(outputOptions))
                {
                    dxtCompressor.Compress(inputOptions, compressionOptions, outputOptions);

                    return dataHandler._buffer;
                }

            }
            finally
            {
                dataHandle.Free();
            }
        }

        public static IMAGE SquishImageWithNvidia(this IMAGE srcImage, CompressionMode flags)
        {
            var blocks = srcImage.ToSquishImage().CompressWithNvidia(flags);

            return Bitmap.Decompress(srcImage.Width, srcImage.Height, blocks, flags).ToImageSharp();
        }
    }

    class DxtDataHandler : IDisposable
    {
        public byte[] _buffer;
        int _offset;

        GCHandle delegateHandleBeginImage;
        GCHandle delegateHandleWriteData;

        public Nvidia.TextureTools.OutputOptions.WriteDataDelegate WriteData { get; private set; }
        public Nvidia.TextureTools.OutputOptions.ImageDelegate BeginImage { get; private set; }

        public DxtDataHandler(Nvidia.TextureTools.OutputOptions outputOptions)
        {
            WriteData = new Nvidia.TextureTools.OutputOptions.WriteDataDelegate(WriteDataInternal);
            BeginImage = new Nvidia.TextureTools.OutputOptions.ImageDelegate(BeginImageInternal);

            // Keep the delegate from being re-located or collected by the garbage collector.
            delegateHandleBeginImage = GCHandle.Alloc(BeginImage);
            delegateHandleWriteData = GCHandle.Alloc(WriteData);

            outputOptions.SetOutputHandler(BeginImage, WriteData);
        }

        ~DxtDataHandler()
        {
            Dispose(false);
        }

        void BeginImageInternal(int size, int width, int height, int depth, int face, int miplevel)
        {
            _buffer = new byte[size];
            _offset = 0;
        }

        bool WriteDataInternal(IntPtr data, int length)
        {
            Marshal.Copy(data, _buffer, _offset, length);
            _offset += length;
            return true;
        }

        #region IDisposable Support
        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Release managed objects
                    // ...
                }

                // Release native objects
                delegateHandleBeginImage.Free();
                delegateHandleWriteData.Free();

                disposed = true;
            }
        }


        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
