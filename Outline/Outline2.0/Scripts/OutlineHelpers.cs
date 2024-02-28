using Unity.Mathematics;
using UnityEngine;

namespace UTools.Outline
{
    public static class OutlineHelpers
    {
        private const int MaxWidth = 32;

        private static float[][] m_GaussSamples;

        /// <summary>
        /// Gets cached gauss samples for the specified outline <paramref name="width"/>.
        /// </summary>
        public static float[] GetGaussSamples(int width)
        {
            var index = Mathf.Clamp(width, 1, MaxWidth) - 1;
            if (m_GaussSamples is null)
                m_GaussSamples = new float[MaxWidth][];

            if (m_GaussSamples[index] is null)
                m_GaussSamples[index] = GetGaussSamples(width, null);

            return m_GaussSamples[index];
        }

        /// <summary>
        /// Samples Gauss function for the specified <paramref name="width"/>.
        /// </summary>
        /// <seealso href="https://en.wikipedia.org/wiki/Normal_distribution"/>
        private static float[] GetGaussSamples(int width, float[] samples)
        {
            // NOTE: According to '3 sigma' rule there is no reason to have StdDev less then width / 3.
            // In practice blur looks best when StdDev is within range [width / 3,  width / 2].
            var stdDev = width * 0.5f;
            if (samples is null)
                samples = new float[MaxWidth];
            for (var i = 0; i < width; i++)
                samples[i] = Gauss(i, stdDev);

            return samples;
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