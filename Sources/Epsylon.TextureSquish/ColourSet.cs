using System;

using Vec3 = System.Numerics.Vector3;
using Vec4 = System.Numerics.Vector4;

namespace Epsylon.TextureSquish
{
    /// <summary>
    /// Represents a set of block colours
    /// </summary>
    class ColourSet
    {
        #region lifecycle

        public ColourSet(Byte[] rgba, int mask, CompressionMode mode,CompressionOptions options)
        {
            // check the compression mode for dxt1
            bool isDxt1 = ((mode & CompressionMode.Dxt1) != 0);
            bool weightByAlpha = ((options & CompressionOptions.WeightColourByAlpha) != 0);
            Initialize(rgba, mask, isDxt1 ? 128 : 1, weightByAlpha);
        }

        #endregion

        #region data

        private int _Count;
        private bool _Transparent;

        private readonly Vec3[] _Points = new Vec3[16];
        private readonly float[] _Weights = new float[16];
        private readonly int[] _Remap = new int[16];

        #endregion

        #region properties

        public int Count => _Count;

        public Vec3[] Points => _Points;

        public float[] Weights => _Weights;

        public bool IsTransparent => _Transparent;

        #endregion

        #region code

        public void Initialize(byte[] rgba, int mask, int alphaThreshold, bool weightByAlpha)
        {
            _Count = 0;
            _Transparent = false;

            // create the minimal set
            for (int i = 0; i < 16; ++i)
            {
                // check this pixel is enabled
                int bit = 1 << i;
                if ((mask & bit) == 0)
                {
                    _Remap[i] = -1;
                    continue;
                }

                // check for transparent pixels when using dxt1
                if (rgba[4 * i + 3] < alphaThreshold)
                {
                    _Remap[i] = -1;
                    _Transparent = true;
                    continue;
                }

                // loop over previous points for a match
                for (int j = 0; ; ++j)
                {
                    // allocate a new point
                    if (j == i)
                    {
                        AddPoint(rgba, i, weightByAlpha);
                        break;
                    }

                    // check for a match
                    if (IsMatch(rgba, mask, alphaThreshold, i, j))
                    {
                        AddWeight(rgba, i, j, weightByAlpha);
                        break;
                    }
                }
            }

            // square root the weights
            for (int i = 0; i < _Count; ++i)
                _Weights[i] = (float)Math.Sqrt(_Weights[i]);
        }

        private static bool IsMatch(byte[] rgba, int mask, int alphaThreshold, int i, int j)
        {
            int oldbit = 1 << j;

            if ((mask & oldbit) == 0) return false;

            i *= 4;
            j *= 4;

            if (rgba[i + 0] != rgba[j + 0]) return false;
            if (rgba[i + 1] != rgba[j + 1]) return false;
            if (rgba[i + 2] != rgba[j + 2]) return false;
            if (rgba[j + 3] < alphaThreshold) return false;

            return true;
        }

        private void AddWeight(byte[] rgba, int i, int j, bool weightByAlpha)
        {
            // get the index of the match
            int index = _Remap[j];

            _Remap[i] = index;

            if (weightByAlpha)
            {
                // ensure there is always non-zero weight even for zero alpha
                float w = (float)(rgba[4 * i + 3] + 1) / 256.0f;
                // map to this point and increase the weight
                _Weights[index] += w;
            }
            else
            {
                // map to this point and increase the weight
                _Weights[index] += 1.0f;
            }
        }

        private void AddPoint(byte[] rgba, int i, bool weightByAlpha)
        {
            _Remap[i] = _Count;

            i *= 4;

            _Points[_Count] = new Vec3(rgba[i + 0], rgba[i + 1], rgba[i + 2]) / 255.0f;

            if (weightByAlpha)
            {
                // ensure there is always non-zero weight even for zero alpha
                float w = (float)(rgba[i + 3] + 1) / 256.0f;
                _Weights[_Count] = w;
            }
            else
            {
                _Weights[_Count] = 1.0f;
            }
            
            // advance
            ++_Count;
        }        

        public void RemapIndices(Byte[] source, Byte[] target)
        {
            for (int i = 0; i < 16; ++i)
            {
                int j = _Remap[i];

                target[i] = j == -1 ? (Byte)3 : source[j];
            }
        }

        #endregion

    };

}


