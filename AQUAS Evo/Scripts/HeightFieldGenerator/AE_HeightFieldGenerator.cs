using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AquasEvo
{
    [ExecuteInEditMode]
    public class AE_HeightFieldGenerator
    {
        #region public variables
        public AE_OceanController m_oceanControllerDEBUG;

        public FFTSize m_fftSize = FFTSize.Large256x256;

        public int m_N = 256;               // FFT grid resolution
        public float m_L = 200;              // Periodical length and width of the height field in meters
        public float m_A = 20;              // Amplitude
        public float m_windDirection = 0;   // Wind direction in degrees

        Vector2 m_windDir;                  // Wind direction as a horizontal vector
        public float m_windSpeed = 30;      // Wind speed in m/s
        #endregion

        #region private variables
        Texture2D m_gaussianRandomTexture;
        Texture2D m_twiddleIndicesTexture;
        Texture2D m_bitReverseIndicesTexture;

        RenderTexture m_h0Texture;
        ComputeShader m_h0Compute;

        RenderTexture m_hktTexture;
        ComputeShader m_hktCompute;

        RenderTexture m_butterflyTexture;
        ComputeShader m_butterflyCompute;

        RenderTexture m_heightFieldTexture;
        ComputeShader m_fftEvalCompute;
        #endregion

        public void Init()
        {
            // Set FFT resolution based on the desired FFT grid size
            switch (m_fftSize)
            {
                case FFTSize.Tiny32x32:
                    m_N = 32;
                    break;

                case FFTSize.Small64x64:
                    m_N = 64;
                    break;

                case FFTSize.Medium128x128:
                    m_N = 128;
                    break;

                case FFTSize.Large256x256:
                    m_N = 256;
                    break;

                case FFTSize.VeryLarge512x512:
                    m_N = 512;
                    break;

                default:
                    break;
            }

            // Generate a texture containing a gaussian random number distribution
            if (m_gaussianRandomTexture == null)
                m_gaussianRandomTexture = AE_OceanUtils.GaussianRandomTexture(m_N, m_gaussianRandomTexture);

            // Generate a texture containing the twiddle indices for the butterfly operations
            m_twiddleIndicesTexture = AE_OceanUtils.TwiddleIndicesTexture(m_N, m_twiddleIndicesTexture);

            // Generate a Texture that contains the bit-reversed indices for the butterfly operation
            m_bitReverseIndicesTexture = AE_OceanUtils.BitReversedIndicesTexture(m_N, m_bitReverseIndicesTexture);

            //Create the Rendertexture that will contain the time-dependent amplitudes and the material for the hkt computation
            m_hktTexture = AE_OceanUtils.CreateBlankComputationTexture(m_N, m_hktTexture, FilterMode.Point);
            m_hktCompute = Resources.Load<ComputeShader>("ComputeShaders/hktCompute");

            //Create the Rendertexture that will contain the result of the butterfly operation
            m_butterflyTexture = AE_OceanUtils.CreateBlankComputationTexture(m_N, m_butterflyTexture, FilterMode.Point);
            m_butterflyCompute = Resources.Load<ComputeShader>("ComputeShaders/butterflyCompute");

            //Create the Rendertexture that will contain the heightfield and the material that turns the raw butterfly texture into a heightfield
            m_heightFieldTexture = AE_OceanUtils.CreateBlankComputationTexture(m_N, m_heightFieldTexture, FilterMode.Bilinear);
            m_fftEvalCompute = Resources.Load<ComputeShader>("ComputeShaders/fftEvalCompute");

            // Step 1: Create the h0 RenderTexture
            m_h0Texture = AE_OceanUtils.CreateBlankComputationTexture(m_N, m_h0Texture, FilterMode.Point);

            //Step 2: Load the Compute shader
            m_h0Compute = Resources.Load<ComputeShader>("ComputeShaders/h0Compute");
            m_oceanControllerDEBUG.m_shader = m_fftEvalCompute;

            // Step 3: Get the horizontal wind vector from the wind direction in degrees
            m_windDir = new Vector2(Mathf.Cos(m_windDirection * (Mathf.PI / 180)), Mathf.Sin(m_windDirection * (Mathf.PI / 180)));

            // Step 4: Call the method that runs the h0 shader and store the result in the RenderTexture
            m_h0Texture = AE_OceanUtils.GetStartAmplitudeTexture(m_L, m_windSpeed, m_N, m_windDir, m_A, m_h0Compute, m_gaussianRandomTexture, m_h0Texture);
        }

        public void Update()
        {
            if (m_gaussianRandomTexture == null ||
                m_twiddleIndicesTexture == null ||
                m_bitReverseIndicesTexture == null ||
                m_h0Texture == null ||
                m_h0Compute == null ||
                m_hktTexture == null ||
                m_hktCompute == null ||
                m_butterflyTexture == null ||
                m_butterflyCompute == null ||
                m_heightFieldTexture == null ||
                m_fftEvalCompute == null)
            {
                Init();
                return;
            }

            m_hktTexture = AE_OceanUtils.GetTimeDependentAmplitudeTexture(m_N, m_L, m_hktCompute, m_h0Texture, m_hktTexture);

            m_heightFieldTexture = AE_OceanUtils.GetHeightFieldTexture(m_hktTexture, m_twiddleIndicesTexture, m_bitReverseIndicesTexture, m_N, m_butterflyCompute, m_butterflyTexture, m_fftEvalCompute, m_heightFieldTexture);

            m_oceanControllerDEBUG.m_debugRenderTex = m_heightFieldTexture;
            m_oceanControllerDEBUG.m_debugTex = m_gaussianRandomTexture;
        }
    }
}