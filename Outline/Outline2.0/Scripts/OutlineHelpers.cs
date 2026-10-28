using Unity.Mathematics;
using UnityEngine;

namespace UTools.Outline
{
    public static class OutlineHelpers
    {
        private const int MaxWidth = 32;
        // The blur shader reads this many pair taps at most (OutlineShader, _BlurWeights and _BlurOffsets).
        public const int MaxBlurTaps = MaxWidth / 2;

        private static BlurTaps[] m_BlurTaps;

        /// <summary>
        /// One side of a symmetric Gauss blur read with bilinear filtering: the centre weight, then pairs of neighbouring
        /// texels merged into one tap each - the summed weight at the offset between the two that bilinear filtering
        /// weighs them by. The same sum as a tap per texel with half the reads.
        /// </summary>
        public sealed class BlurTaps
        {
            public float CenterWeight;
            public readonly float[] Weights = new float[MaxBlurTaps];
            public readonly float[] Offsets = new float[MaxBlurTaps];
            public int Count;
        }

        /// <summary>
        /// Gets cached blur taps for the specified blur <paramref name="width"/>: the texels 1 to width - 1 of each side,
        /// weighed by the Gauss function with StdDev width / 2.
        /// </summary>
        public static BlurTaps GetBlurTaps(int width)
        {
            var index = Mathf.Clamp(width, 1, MaxWidth) - 1;
            m_BlurTaps ??= new BlurTaps[MaxWidth];
            return m_BlurTaps[index] ??= CreateBlurTaps(index + 1);
        }

        private static BlurTaps CreateBlurTaps(int width)
        {
            // NOTE: According to '3 sigma' rule there is no reason to have StdDev less then width / 3.
            // In practice blur looks best when StdDev is within range [width / 3,  width / 2].
            var stdDev = width * 0.5f;
            var taps = new BlurTaps { CenterWeight = Gauss(0, stdDev) };
            for (var texel = 1; texel < width; texel += 2)
            {
                var weight = Gauss(texel, stdDev);
                var next = texel + 1 < width ? Gauss(texel + 1, stdDev) : 0;
                taps.Weights[taps.Count] = weight + next;
                taps.Offsets[taps.Count] = (texel * weight + (texel + 1) * next) / (weight + next);
                taps.Count++;
            }

            return taps;
        }

        /// <summary>
        /// Calculates value of Gauss function for the specified <paramref name="x"/> and <paramref name="stdDev"/> values.
        /// </summary>
        /// <seealso href="https://en.wikipedia.org/wiki/Gaussian_blur"/>
        /// <seealso href="https://en.wikipedia.org/wiki/Normal_distribution"/>
        private static float Gauss(float x, float stdDev)
        {
            var stdDev2 = stdDev * stdDev * 2;
            var a = 1 / math.sqrt(math.PI * stdDev2);
            var gauss = a * math.pow(math.E, -x * x / stdDev2);

            return gauss;
        }
    }
}
