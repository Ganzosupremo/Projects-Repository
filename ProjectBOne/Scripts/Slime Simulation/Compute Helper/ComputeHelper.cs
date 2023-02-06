using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace ComputeShader
{
    public static class ComputeHelper
    {
        public const FilterMode defaultFilterMode = FilterMode.Bilinear;
        public const GraphicsFormat defaultGraphicsFormat = GraphicsFormat.R16G16B16A16_SFloat;

        private static UnityEngine.ComputeShader normalizeTextureCompute;
        private static UnityEngine.ComputeShader clearTextureCompute;
        private static UnityEngine.ComputeShader swizzleTextureCompute;


        // Subscribe to this event to be notified when buffers created in edit mode should be released
        // (i.e before script compilation occurs, and when exitting edit mode)
        public static event Action ShouldReleaseEditModeBuffers;

        /// <summary>
        /// Convenience method for dispatching a compute shader.
        /// It calculates the number of thread groups based on the number of iterations needed. 
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="numIterationsX"></param>
        /// <param name="numIterationsY"></param>
        /// <param name="numIterationsZ"></param>
        /// <param name="kernelIndex"></param>
        public static void Dispatch(UnityEngine.ComputeShader cs, int numIterationsX, int numIterationsY = 1, int numIterationsZ = 1, int kernelIndex = 0)
        {
            Vector3Int threadGroupSizes = GetThreadGroupSizes(cs, kernelIndex);
            int numGroupsX = Mathf.CeilToInt(numIterationsX / (float)threadGroupSizes.x);
            int numGroupsY = Mathf.CeilToInt(numIterationsY / (float)threadGroupSizes.y);
            int numGroupsZ = Mathf.CeilToInt(numIterationsZ / (float)threadGroupSizes.y);
            cs.Dispatch(kernelIndex, numGroupsX, numGroupsY, numGroupsZ);
        }

        public static void CreateStructuredBuffer<T>(ref ComputeBuffer buffer, int count)
        {
            int stride = System.Runtime.InteropServices.Marshal.SizeOf(typeof(T));
            bool createNewBuffer = buffer == null || !buffer.IsValid() || buffer.count != count || buffer.stride != stride;
            if (createNewBuffer)
            {
                Release(buffer);
                buffer = new ComputeBuffer(count, stride);
            }
        }

        public static void CreateStructuredBuffer<T>(ref ComputeBuffer buffer, T[] data)
        {
            CreateStructuredBuffer<T>(ref buffer, data.Length);
            buffer.SetData(data);
        }

        public static ComputeBuffer CreateAndSetBuffer<T>(T[] data, UnityEngine.ComputeShader cs, string nameID, int kernelIndex = 0)
        {
            ComputeBuffer buffer = null;
            CreateAndSetBuffer<T>(ref buffer, data, cs, nameID, kernelIndex);
            return buffer;
        }

        public static void CreateAndSetBuffer<T>(ref ComputeBuffer buffer, T[] data, UnityEngine.ComputeShader cs, string nameID, int kernelIndex = 0)
        {
            int stride = System.Runtime.InteropServices.Marshal.SizeOf(typeof(T));
            CreateStructuredBuffer<T>(ref buffer, data.Length);
            buffer.SetData(data);
            cs.SetBuffer(kernelIndex, nameID, buffer);
        }

        public static ComputeBuffer CreateAndSetBuffer<T>(int length, UnityEngine.ComputeShader cs, string nameID, int kernelIndex = 0)
        {
            ComputeBuffer buffer = null;
            CreateAndSetBuffer<T>(ref buffer, length, cs, nameID, kernelIndex);
            return buffer;
        }

        public static void CreateAndSetBuffer<T>(ref ComputeBuffer buffer, int length, UnityEngine.ComputeShader cs, string nameID, int kernelIndex = 0)
        {
            CreateStructuredBuffer<T>(ref buffer, length);
            cs.SetBuffer(kernelIndex, nameID, buffer);
        }

        /// <summary>
        /// Releases supplied buffer/s if not null
        /// </summary>
        /// <param name="buffers"></param>
        public static void Release(params ComputeBuffer[] buffers)
        {
            for (int i = 0; i < buffers.Length; i++)
            {
                buffers[i]?.Release();
            }
        }

        public static Vector3Int GetThreadGroupSizes(UnityEngine.ComputeShader compute, int kernelIndex = 0)
        {
            uint x, y, z;
            compute.GetKernelThreadGroupSizes(kernelIndex, out x, out y, out z);
            return new Vector3Int((int)x, (int)y, (int)z);
        }

        #region Texture Helpers
        public static void CreateRenderTexture(ref RenderTexture texture, int width, int height)
        {
            CreateRenderTexture(ref texture, width, height, defaultFilterMode, defaultGraphicsFormat);
        }

        public static void CreateRenderTexture(ref RenderTexture texture, int width, int height, FilterMode filterMode, GraphicsFormat format)
        {
            if (texture == null || !texture.IsCreated() || texture.width != width || texture.height != height || texture.graphicsFormat != format)
            {
                if (texture != null)
                {
                    texture.Release();
                }
                texture = new RenderTexture(width, height, 0);
                texture.graphicsFormat = format;
                texture.enableRandomWrite = true;

                texture.autoGenerateMips = false;
                texture.Create();
            }
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = filterMode;
        }
 
        /// <summary>
        /// Copy the contents of one render texture into another. Assumes textures are the same size.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
		public static void CopyRenderTexture(Texture source, RenderTexture target)
        {
            Graphics.Blit(source, target);
        }

        /// <summary>
        /// Swap channels of texture, or set to zero. For example, if inputs are: (green, red, zero, zero)
        /// then red and green channels will be swapped, and blue and alpha channels will be set to zero.
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="w"></param>
		public static void SwizzleTexture(Texture texture, Channel x, Channel y, Channel z, Channel w)
        {
            if (swizzleTextureCompute == null)
            {
                swizzleTextureCompute = (UnityEngine.ComputeShader)Resources.Load("Swizzle");
            }

            swizzleTextureCompute.SetInt("width", texture.width);
            swizzleTextureCompute.SetInt("height", texture.height);
            swizzleTextureCompute.SetTexture(0, "Source", texture);
            swizzleTextureCompute.SetVector("x", ChannelToMask(x));
            swizzleTextureCompute.SetVector("y", ChannelToMask(y));
            swizzleTextureCompute.SetVector("z", ChannelToMask(z));
            swizzleTextureCompute.SetVector("w", ChannelToMask(w));
            Dispatch(swizzleTextureCompute, texture.width, texture.height, 1, 0);
        }

        /// <summary>
        /// Sets all pixels of supplied texture to 0
        /// </summary>
        /// <param name="source"></param>
		public static void ClearRenderTexture(RenderTexture source)
        {
            if (clearTextureCompute == null)
            {
                clearTextureCompute = (UnityEngine.ComputeShader)Resources.Load("ClearTexture");
            }
            clearTextureCompute.SetInt("width", source.width);
            clearTextureCompute.SetInt("height", source.height);
            clearTextureCompute.SetTexture(0, "Source", source);
            Dispatch(clearTextureCompute, source.width, source.height, 1, 0);
        }

        /// <summary>
        /// Work in progress, currently only works with one channel and very slow
        /// </summary>
        /// <param name="source"></param>
		public static void NormalizeRenderTexture(RenderTexture source)
        {
            if (normalizeTextureCompute == null)
            {
                normalizeTextureCompute = (UnityEngine.ComputeShader)Resources.Load("NormalizeTexture");
            }

            normalizeTextureCompute.SetInt("width", source.width);
            normalizeTextureCompute.SetInt("height", source.height);
            normalizeTextureCompute.SetTexture(0, "Source", source);
            normalizeTextureCompute.SetTexture(1, "Source", source);

            ComputeBuffer minMaxBuffer = CreateAndSetBuffer<int>(new int[] { int.MaxValue, 0 }, normalizeTextureCompute, "minMaxBuffer", 0);
            normalizeTextureCompute.SetBuffer(1, "minMaxBuffer", minMaxBuffer);

            Dispatch(normalizeTextureCompute, source.width, source.height, 1, 0);
            Dispatch(normalizeTextureCompute, source.width, source.height, 1, 1);

            //int[] data = new int[2];
            //minMaxBuffer.GetData(data);
            //Debug.Log(data[0] + "   " + data[1]);

            Release(minMaxBuffer);
        }
        #endregion

        // https://cmwdexint.com/2017/12/04/computeshader-setfloats/
        public static float[] PackFloats(params float[] values)
        {
            float[] packed = new float[values.Length * 4];
            for (int i = 0; i < values.Length; i++)
            {
                packed[i * 4] = values[i];
            }
            return values;
        }

        /// <summary>
        /// Only run compute shaders if this is true.
        /// This is only relevant for compute shaders that run outside of playmode.
        /// </summary>
        public static bool CanRunEditModeCompute
        {
            get
            {
                return CheckIfCanRunInEditMode();
            }
        }

        /// <summary>
        /// Set all values from settings object on the shader. Note, variable names must be an exact match in the shader.
        /// Settings object can be any class/struct containing vectors/ints/floats/bools
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="shader"></param>
        /// <param name="variableNamePrefix"></param>
        /// <param name="variableNameSuffix"></param>
        public static void SetParams(System.Object settings, UnityEngine.ComputeShader shader, string variableNamePrefix = "", string variableNameSuffix = "")
        {
            var fields = settings.GetType().GetFields();
            foreach (var field in fields)
            {
                var fieldType = field.FieldType;
                string shaderVariableName = variableNamePrefix + field.Name + variableNameSuffix;

                if (fieldType == typeof(UnityEngine.Vector4) || fieldType == typeof(Vector3) || fieldType == typeof(Vector2))
                {
                    shader.SetVector(shaderVariableName, (Vector4)field.GetValue(settings));
                }
                else if (fieldType == typeof(int))
                {
                    shader.SetInt(shaderVariableName, (int)field.GetValue(settings));
                }
                else if (fieldType == typeof(float))
                {
                    shader.SetFloat(shaderVariableName, (float)field.GetValue(settings));
                }
                else if (fieldType == typeof(bool))
                {
                    shader.SetBool(shaderVariableName, (bool)field.GetValue(settings));
                }
                else
                {
                    Debug.Log($"Type {fieldType} not implemented");
                }
            }
        }

        private static Vector4 ChannelToMask(Channel channel)
        {
            switch (channel)
            {
                case Channel.Red:
                    return new Vector4(1, 0, 0, 0);
                case Channel.Green:
                    return new Vector4(0, 1, 0, 0);
                case Channel.Blue:
                    return new Vector4(0, 0, 1, 0);
                case Channel.Alpha:
                    return new Vector4(0, 0, 0, 1);
                case Channel.Zero:
                    return new Vector4(0, 0, 0, 0);
                default:
                    return Vector4.zero;
            }
        }

        #region Editor Helpers
#if UNITY_EDITOR
        static UnityEditor.PlayModeStateChange playModeState;

        static ComputeHelper()
        {
            // Monitor play mode state
            UnityEditor.EditorApplication.playModeStateChanged -= MonitorPlayModeState;
            UnityEditor.EditorApplication.playModeStateChanged += MonitorPlayModeState;
            // Monitor script compilation
            UnityEditor.Compilation.CompilationPipeline.compilationStarted -= OnCompilationStarted;
            UnityEditor.Compilation.CompilationPipeline.compilationStarted += OnCompilationStarted;
        }

        static void MonitorPlayModeState(UnityEditor.PlayModeStateChange state)
        {
            playModeState = state;
            if (state == UnityEditor.PlayModeStateChange.ExitingEditMode)
            {
                ShouldReleaseEditModeBuffers?.Invoke(); //
            }
        }

        static void OnCompilationStarted(System.Object obj)
        {
            ShouldReleaseEditModeBuffers?.Invoke();
        }
#endif

        static bool CheckIfCanRunInEditMode()
        {
            bool isCompilingOrExitingEditMode = false;
#if UNITY_EDITOR
            isCompilingOrExitingEditMode |= UnityEditor.EditorApplication.isCompiling;
            isCompilingOrExitingEditMode |= playModeState == UnityEditor.PlayModeStateChange.ExitingEditMode;
#endif
            bool canRun = !isCompilingOrExitingEditMode;
            return canRun;
        }
        #endregion Editor Helpers
    }
}
