using System;
using System.Collections.Generic;

using Vec3 = System.Numerics.Vector3;
using Vec4 = System.Numerics.Vector4;

namespace Epsylon.TextureSquish
{
    // based on https://github.com/castano/nvidia-texture-tools/blob/master/src/nvtt/squish/clusterfit.cpp
    class ClusterFitAlt : ColourFit
    {
        private static readonly Vec4 HALF_HALF2 = new Vec4(0.5f, 0.5f, 0.5f, 0.25f);
        private static readonly Vec4 HALF = new Vec4(0.5f);
        private static readonly Vec4 TWO = new Vec4(2);
        private static readonly Vec4 GRID = new Vec4(31.0f, 63.0f, 31.0f, 1.0f);
        private static readonly Vec4 GRIDRCP = new Vec4(1) / GRID;
        private static readonly Vec4 ONETHIRD_ONETHIRD2 = new Vec4(1.0f / 3.0f, 1.0f / 3.0f, 1.0f / 3.0f, 1.0f / 9.0f);
        private static readonly Vec4 TWOTHIRDS_TWOTHIRDS2 = new Vec4(2.0f / 3.0f, 2.0f / 3.0f, 2.0f / 3.0f, 4.0f / 9.0f);
        private static readonly Vec4 TWONINETHS = new Vec4(2.0f / 9.0f);        

        public ClusterFitAlt(ColourSet colours, CompressionOptions flags) : base(colours)
        {
            // initialise the metric
            bool perceptual = ((flags & CompressionOptions.ColourMetricPerceptual) != 0);

            m_metric = perceptual ? new Vec4(0.2126f, 0.7152f, 0.0722f, 1) : Vec4.One;
            m_metricSqr = m_metric * m_metric;            

            // get the covariance matrix
            var covariance = Sym3x3.ComputeWeightedCovariance(m_colours.Count, m_colours.Points, m_colours.Weights, m_metric.GetVec3());

            // compute the principle component
            m_principle = Sym3x3.ComputePrincipleComponent(covariance);
        }

        private readonly Vec3 m_principle;
        private readonly Vec4 m_metric;
        private readonly Vec4 m_metricSqr;

        private readonly Byte[] m_order = new Byte[16];        
        private readonly Vec4[] m_weighted = new Vec4[16];
        private readonly Vec4[] m_unweighted = new Vec4[16];
        private readonly float[] m_weights = new float[16];        

        // input for least squares
        private readonly float[] m_alpha = new float[16];
        private readonly float[] m_beta = new float[16];        

        private bool ConstructOrdering(Vec3 axis)
        {
            // cache some values
            var count = m_colours.Count;
            var values = m_colours.Points;           

            // build the list of dot products
            var dps = new float[count];
            for (int i = 0; i < count; ++i)
            {
                dps[i] = Vec3.Dot(values[i], axis);
                m_order[i] = (Byte)i;
            }            

            // stable sort using them
            for (int i = 0; i < dps.Length; ++i)
            {
                for (int j = i; j > 0 && dps[j] < dps[j - 1]; --j)
                {
                    dps.SwapElements(j, j - 1);
                    m_order.SwapElements(j,j - 1);
                }
            }            

            // copy the ordering and weight all the points
            var unweighted = m_colours.Points;
            var weights = m_colours.Weights;

            for (int i = 0; i < count; ++i)
            {
                int j = m_order[i];

                m_weights[i] = weights[j];
                m_unweighted[i] = new Vec4(unweighted[j], 1);

                m_weighted[i] = m_unweighted[i] * m_weights[i];                
            }

            return true;
        }

        protected override void Compress3(BlockWindow block)
        {
            // prepare an ordering using the principle axis
            ConstructOrdering(m_principle);

            // declare variables
            int count = m_colours.Count;

            Vec4 beststart = Vec4.Zero;
            Vec4 bestend = Vec4.Zero;

            var besterror = float.MaxValue;            
            var bestbesterror = float.MaxValue;

            // check all possible clusters for this total order
            var indices = new Byte[16];
            var bestindices = new Byte[16];

            // first cluster [0,i) is at the start
            for (int m = 0; m < count; ++m)
            {
                indices[m] = 0;
                m_alpha[m] = m_weights[m];
                m_beta[m] = 0;
            }

            for (int i = count; i >= 0; --i)
            {
                // second cluster [i,j) is half along
                for (int m = i; m < count; ++m)
                {
                    indices[m] = 2;
                    m_alpha[m] = m_beta[m] = 0.5f * m_weights[m];
                }

                for (int j = count; j > i; --j)
                {
                    // last cluster [j,k) is at the end
                    if (j < count)
                    {
                        indices[j] = 1;
                        m_alpha[j] = 0;
                        m_beta[j] = m_weights[j];
                    }

                    // solve a least squares problem to place the endpoints                    
                    var error = SolveLeastSquares(out Vec4 start, out Vec4 end);

                    // keep the solution if it wins
                    if (error < besterror)
                    {
                        beststart = start;
                        bestend = end;
                        indices.CopyTo(bestindices, 0);
                        besterror = error;
                    }
                }
            }

            // save the block if necessary
            if (besterror < bestbesterror)
            {
                // remap the indices
                var unordered = new Byte[16];
                for (int i = 0; i < count; ++i) unordered[m_order[i]] = bestindices[i];
                m_colours.RemapIndices(unordered, bestindices);

                // save the block
                block.WriteColourBlock3(beststart.GetVec3(), bestend.GetVec3(), bestindices);

                // save the error
                bestbesterror = besterror;
            }
        }

        protected override void Compress4(BlockWindow block)
        {
            // prepare an ordering using the principle axis
            ConstructOrdering(m_principle);            

            // declare variables
            int count = m_colours.Count;

            var beststart = Vec4.Zero;
            var bestend = Vec4.Zero;
            var besterror = float.MaxValue;
            var bestbesterror = float.MaxValue;

            const float twothirds = 2.0f / 3.0f;
            const float onethird = 1.0f / 3.0f;

            // check all possible clusters for this total order
            var indices = new Byte[16];
            var bestindices = new Byte[16];

            // first cluster [0,i) is at the start
            for (int m = 0; m < count; ++m)
            {
                indices[m] = 0;
                m_alpha[m] = m_weights[m];
                m_beta[m] = 0;
            }

            for (int i = count; i >= 0; --i)
            {
                // second cluster [i,j) is one third along
                for (int m = i; m < count; ++m)
                {
                    indices[m] = 2;
                    m_alpha[m] = twothirds * m_weights[m];
                    m_beta[m] = onethird * m_weights[m];
                }

                for (int j = count; j >= i; --j)
                {
                    // third cluster [j,k) is two thirds along
                    for (int m = j; m < count; ++m)
                    {
                        indices[m] = 3;
                        m_alpha[m] = onethird * m_weights[m];
                        m_beta[m] = twothirds * m_weights[m];
                    }

                    for (int k = count; k >= j; --k)
                    {
                        if (j + k == 0) continue;

                        // last cluster [k,n) is at the end
                        if (k < count)
                        {
                            indices[k] = 1;
                            m_alpha[k] = 0;
                            m_beta[k] = m_weights[k];
                        }

                        // solve a least squares problem to place the endpoints                        
                        var error = SolveLeastSquares(out Vec4 start, out Vec4 end);

                        // keep the solution if it wins
                        if (error < besterror)

                        {
                            beststart = start;
                            bestend = end;
                            indices.CopyTo(bestindices, 0);
                            besterror = error;
                        }
                    }
                }
            }

            // save the block if necessary
            if (besterror < bestbesterror)
            {
                // remap the indices
                var unordered = new Byte[16];
                for (int i = 0; i < count; ++i) unordered[m_order[i]] = bestindices[i];
                m_colours.RemapIndices(unordered, bestindices);

                // save the block
                block.WriteColourBlock4(beststart.GetVec3(), bestend.GetVec3(), bestindices);

                // save the error
                bestbesterror = besterror;
            }
        }

        private float SolveLeastSquares(out Vec4 start, out Vec4 end)
        {
            // accumulate all the quantities we need
            int count = m_colours.Count;

            float alpha2_sum = 0;
            float beta2_sum = 0;
            float alphabeta_sum = 0;

            var alphax_sum = Vec4.Zero;
            var betax_sum = Vec4.Zero;

            for (int i = 0; i < count; ++i)
            {
                var alpha = m_alpha[i];
                var beta = m_beta[i];
                var x = m_weighted[i];

                alpha2_sum += alpha * alpha;
                beta2_sum += beta * beta;
                alphabeta_sum += alpha * beta;
                alphax_sum += alpha * x;
                betax_sum += beta * x;
            }            

            // zero where non-determinate
            Vec4 a, b;
            if (beta2_sum == 0)
            {
                a = alphax_sum / alpha2_sum;
                b = Vec4.Zero;
            }
            else if (alpha2_sum == 0)
            {
                a = Vec4.Zero;
                b = betax_sum / beta2_sum;
            }
            else
            {
                var factor = 1.0f / (alpha2_sum * beta2_sum - alphabeta_sum * alphabeta_sum);

                a = (alphax_sum * beta2_sum - betax_sum * alphabeta_sum) * factor;
                b = (betax_sum * alpha2_sum - alphax_sum * alphabeta_sum) * factor;
            }

            // clamp the output to [0, 1]            
            a = a.Clamp(Vec4.Zero, Vec4.One);
            b = b.Clamp(Vec4.Zero, Vec4.One);

            // clamp to the grid            
            a = GRID.MultiplyAdd(a, HALF).Truncate() * GRIDRCP;
            b = GRID.MultiplyAdd(b, HALF).Truncate() * GRIDRCP;

            // compute the error
            var e1 = a * a * alpha2_sum + b * b * beta2_sum /*+ m_xxsum*/
                + 2 * (a * b * alphabeta_sum - a * alphax_sum - b * betax_sum);

            // apply the metric to the error term
            float error = Vec4.Dot(e1, m_metricSqr);            

            // save the start and end
            start = a;
            end = b;
            return error;
        }
    }

}