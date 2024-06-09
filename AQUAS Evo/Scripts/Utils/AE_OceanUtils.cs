using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AquasEvo
{
    public class AE_OceanUtils
    {
        /// <summary>
        /// Returns a texture with the given resolution that contains a gaussian random number distribution in each color channel 
        /// </summary>
        /// <param name="resolution"></param>
        /// <returns></returns>
        public static Texture2D GaussianRandomTexture(int resolution, Texture2D targetTexture)
        {
            targetTexture = new Texture2D(resolution, resolution, TextureFormat.RGBAFloat, false);

            for (int i = 0; i < resolution; i++)
            {
                for (int j = 0; j < resolution; j++)
                {
                    targetTexture.SetPixel(i, j, new Color(GaussianRandomNumber(0, 1), GaussianRandomNumber(0, 1), GaussianRandomNumber(0, 1), GaussianRandomNumber(0, 1)));
                }
            }

            targetTexture.filterMode = FilterMode.Point;

            targetTexture.Apply();

            return targetTexture;
        }

        /// <summary>
        /// Returns a gaussian random number based on the given mean and standard deviation
        /// </summary>
        /// <param name="mean"></param>
        /// <param name="stddev"></param>
        /// <returns></returns>
        static float GaussianRandomNumber(float mean, float stddev)      //Gaussian Random Number Generator
        {
            float x1 = 1 - Random.value;
            float x2 = 1 - Random.value;
            float r = Mathf.Sqrt(-2 * Mathf.Log(x1)) * Mathf.Cos(2 * Mathf.PI * x2);
            return r * stddev + mean;
        }

        public static Texture2D TwiddleIndicesTexture(int resolution, Texture2D targetTexture)
        {
            int sample = 0;
            int samplesPerBlock = 0;

            targetTexture = new Texture2D((int)Mathf.Log(resolution, 2), resolution, TextureFormat.RGFloat, false);
            targetTexture.filterMode = FilterMode.Point;

            for (int i = 1; i <= Mathf.Log(resolution, 2); i++)
            {
                for (int j = 0; j < resolution; j++)
                {
                    if (j % Mathf.Pow(2, i) < Mathf.Pow(2, i - 1))
                    {
                        sample = 0;
                        targetTexture.SetPixel(i - 1, j, new Color(1, 0, 0, 1));
                    }
                    else
                    {
                        samplesPerBlock = (int)Mathf.Pow(2, i);
                        Vector2 twiddleFactor = TwiddleFactor(samplesPerBlock, sample);
                        targetTexture.SetPixel(i - 1, j, new Color(twiddleFactor.x, twiddleFactor.y, 0, 1));
                        sample++;
                    }
                }
            }

            targetTexture.Apply();

            return targetTexture;
        }

        static Vector2 TwiddleFactor(int N, int n_)
        {
            //int n = n_ + (N / 2);
            return new Vector2(Mathf.Cos((2 * Mathf.PI * n_) / N), Mathf.Sin((2 * Mathf.PI * n_) / N));
        }

        public static Texture2D BitReversedIndicesTexture(int N, Texture2D targetTexture)
        {
            targetTexture = new Texture2D(1, N, TextureFormat.RFloat, false);
            targetTexture.filterMode = FilterMode.Point;

            for (int n = 0; n < N; n++)
                targetTexture.SetPixel(0, n, new Color((float)GetReversedIndex(n, N), 0, 0, 1));

            targetTexture.Apply();

            return targetTexture;
        }

        static int GetReversedIndex(int i, int N)
        {
            int res = 0;

            for (int j = 0; j < Mathf.Log(N, 2); j++)
            {
                res = (res << 1) + (i & 1);
                i >>= 1;
            }
            return res;
        }

        public static RenderTexture CreateBlankComputationTexture(int resolution, RenderTexture target, FilterMode filterMode)
        {
            if (target != null)
            {
                RenderTexture.ReleaseTemporary(target);
            }

            target = RenderTexture.GetTemporary(resolution, resolution, 0, RenderTextureFormat.ARGBFloat);
            //target = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGBFloat);
            target.enableRandomWrite = true;
            target.filterMode = filterMode;
            target.Create();

            return target;
        }

        public static RenderTexture GetStartAmplitudeTexture(float L, float windSpeed, int N, Vector2 windDir, float amplitude, ComputeShader computeShader, Texture2D gaussianRndTexture, RenderTexture targetTexture)
        {
            int kernelHandle = computeShader.FindKernel("CSMain");

            // Set the textures and parameters
            computeShader.SetTexture(kernelHandle, "_GaussRand", gaussianRndTexture);
            computeShader.SetTexture(kernelHandle, "Result", targetTexture);
            computeShader.SetFloat("_L", L);
            computeShader.SetFloat("_windSpeed", windSpeed);
            computeShader.SetInt("_N", N);
            computeShader.SetVector("_windDir", new Vector4(windDir.x, windDir.y, 0, 0));
            computeShader.SetFloat("_A", amplitude);

            // Dispatch the compute shader
            int threadGroupsX = Mathf.CeilToInt(N / 8.0f);
            int threadGroupsY = Mathf.CeilToInt(N / 8.0f);
            computeShader.Dispatch(kernelHandle, threadGroupsX, threadGroupsY, 1);

            return targetTexture;
        }

        public static RenderTexture GetTimeDependentAmplitudeTexture(int N, float L, ComputeShader computeShader, RenderTexture h0Texture, RenderTexture targetTexture)
        {
            int kernelHandle = computeShader.FindKernel("CSMain");

            // Set the textures and parameters
            computeShader.SetTexture(kernelHandle, "_h0Tex", h0Texture);
            computeShader.SetTexture(kernelHandle, "Result", targetTexture);
            computeShader.SetFloat("_N", N);
            computeShader.SetFloat("_L", L);

            // Dispatch the compute shader
            int threadGroupsX = Mathf.CeilToInt(N / 8.0f);
            int threadGroupsY = Mathf.CeilToInt(N / 8.0f);
            computeShader.Dispatch(kernelHandle, threadGroupsX, threadGroupsY, 1);

            return targetTexture;
        }

        public static RenderTexture GetHeightFieldTexture(RenderTexture hktTexture, Texture2D twiddleIndices, Texture2D bitReversedIndices, int N, ComputeShader butterflyCompute, RenderTexture butterFlyTex, ComputeShader fftEvalCompute, RenderTexture targetTexture)
        {
            RenderTexture temp = RenderTexture.GetTemporary(N, N, 0, RenderTextureFormat.ARGBFloat);
            temp.enableRandomWrite = true;
            temp.Create();

            int kernelHandleButterfly = butterflyCompute.FindKernel("CSMain");
            int kernelHandlefftEval = fftEvalCompute.FindKernel("CSMain");

            butterflyCompute.SetTexture(kernelHandleButterfly, "Result", temp);
            butterflyCompute.SetTexture(kernelHandleButterfly, "_TwiddleIndices", twiddleIndices);
            butterflyCompute.SetTexture(kernelHandleButterfly, "_BitReversedIndices", bitReversedIndices);
            butterflyCompute.SetTexture(kernelHandleButterfly, "_hktTex", hktTexture);
            butterflyCompute.SetInt("_N", N);
            butterflyCompute.SetInt("_type", 0);

            fftEvalCompute.SetTexture(kernelHandlefftEval, "Result", targetTexture);
            fftEvalCompute.SetInt("_N", N);
            fftEvalCompute.SetInt("_type", 0);

            for (int direction = 0; direction < 2; direction++)
            {
                butterflyCompute.SetInt("_direction", direction);

                for (int i = 0; i < Mathf.Log(N, 2); i++)
                {
                    butterflyCompute.SetInt("_i", i);

                    if (i == 0)
                    {
                        if (direction == 0)
                        {
                            butterflyCompute.SetTexture(kernelHandleButterfly, "_inputTex", hktTexture);
                        }
                        else
                        {
                            butterflyCompute.SetTexture(kernelHandleButterfly, "_inputTex", butterFlyTex);
                        }
                    }
                    else
                    {
                        butterflyCompute.SetTexture(kernelHandleButterfly, "_inputTex", butterFlyTex);
                    }

                    // Dispatch the compute shader
                    int threadGroupsX = Mathf.CeilToInt(N / 8.0f);
                    int threadGroupsY = Mathf.CeilToInt(N / 8.0f);
                    butterflyCompute.Dispatch(kernelHandleButterfly, threadGroupsX, threadGroupsY, 1);

                    // Blit the result to butterFlyTex
                    Graphics.Blit(temp, butterFlyTex);

                    if (i == Mathf.Log(N, 2) - 1 && direction == 1)
                    {
                        fftEvalCompute.SetTexture(kernelHandlefftEval, "_inputTex", butterFlyTex);

                        // Dispatch the fft evaluation compute shader
                        int threadGroupsfftevalX = Mathf.CeilToInt(N / 8.0f);
                        int threadGroupsfftevalY = Mathf.CeilToInt(N / 8.0f);
                        fftEvalCompute.Dispatch(kernelHandlefftEval, threadGroupsfftevalX, threadGroupsfftevalY, 1);
                    }
                }
            }

            //return temp;

            RenderTexture.ReleaseTemporary(temp);

            return targetTexture;
        }
    }
}