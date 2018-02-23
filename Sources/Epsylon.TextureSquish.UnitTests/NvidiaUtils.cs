using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

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

        public static Byte[] CompressWithNvidia(this Bitmap srcImage, CompressionMode mode)
        {
            Assert.AreEqual(8, IntPtr.Size, "nvtt.dll(x64) requires x64 runtime");

            srcImage = srcImage.Clone();
            srcImage.SwapElements(2, 1, 0, 3);

            var inputOptions = new Nvidia.TextureTools.InputOptions();
            inputOptions.SetTextureLayout(Nvidia.TextureTools.TextureType.Texture2D, srcImage.Width, srcImage.Height, 1);
            inputOptions.SetMipmapGeneration(false);
            inputOptions.SetGamma(1.0f, 1.0f);
            if ((mode & CompressionMode.Dxt1) == 0) inputOptions.SetAlphaMode(Nvidia.TextureTools.AlphaMode.Premultiplied);
            else inputOptions.SetAlphaMode(Nvidia.TextureTools.AlphaMode.None);            

            var compressionOptions = new Nvidia.TextureTools.CompressionOptions();
            compressionOptions.SetQuality(Nvidia.TextureTools.Quality.Normal);
            if ((mode & CompressionMode.Dxt1) != 0) compressionOptions.SetFormat(Nvidia.TextureTools.Format.DXT1);
            if ((mode & CompressionMode.Dxt3) != 0) compressionOptions.SetFormat(Nvidia.TextureTools.Format.DXT3);
            if ((mode & CompressionMode.Dxt5) != 0) compressionOptions.SetFormat(Nvidia.TextureTools.Format.DXT5);            

            return DxtDataHandler.Compress(srcImage.Data, srcImage.Width,srcImage.Height, inputOptions, compressionOptions);            
        }

        public static IMAGE SquishImageWithNvidia(this IMAGE srcImage, CompressionMode mode, TestContext context)
        {
            var srcBitmap = srcImage.ToSquishImage();

            var blocks = srcBitmap.CompressWithNvidia(mode);

            var dstBitmap = Bitmap.Decompress(srcImage.Width, srcImage.Height, blocks, mode);

            context.WriteLine(dstBitmap.CompareRGBToOriginal(srcBitmap).ToString());

            return dstBitmap.ToImageSharp();
        }
    }

    class DxtDataHandler : IDisposable
    {
        #region lifecycle

        private DxtDataHandler(Nvidia.TextureTools.OutputOptions outputOptions)
        {
            _WriteData = new Nvidia.TextureTools.OutputOptions.WriteDataDelegate(WriteDataInternal);
            _BeginImage = new Nvidia.TextureTools.OutputOptions.ImageDelegate(BeginImageInternal);

            // Keep the delegate from being re-located or collected by the garbage collector.
            delegateHandleBeginImage = GCHandle.Alloc(_BeginImage);
            delegateHandleWriteData = GCHandle.Alloc(_WriteData);

            outputOptions.SetOutputHandler(_BeginImage, _WriteData);
        }

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

        ~DxtDataHandler()
        {
            Dispose(false);
        }

        #endregion

        #region data

        private bool disposed = false;

        private byte[] _buffer;
        private int _offset;

        private GCHandle delegateHandleBeginImage;
        private GCHandle delegateHandleWriteData;

        private Nvidia.TextureTools.OutputOptions.WriteDataDelegate _WriteData;
        private Nvidia.TextureTools.OutputOptions.ImageDelegate _BeginImage;

        #endregion        

        #region code

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

        #endregion

        #region API

        public static Byte[] Compress(Byte[] data, int width, int height, Nvidia.TextureTools.InputOptions inOptions, Nvidia.TextureTools.CompressionOptions compressionOptions)
        {
            var dxtCompressor = new Nvidia.TextureTools.Compressor();

            var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);

            try
            {
                var dataPtr = dataHandle.AddrOfPinnedObject();
                inOptions.SetMipmapData(dataPtr, width, height, 1, 0, 0);

                var outOptions = new Nvidia.TextureTools.OutputOptions();
                outOptions.SetOutputHeader(false);

                using (var dataHandler = new DxtDataHandler(outOptions))
                {
                    dxtCompressor.Compress(inOptions, compressionOptions, outOptions);

                    return dataHandler._buffer;
                }
            }
            finally
            {
                dataHandle.Free();
            }
        }

        #endregion
    }
}
