using UnityEngine;
using UnityEngine.Rendering;

namespace DefferedPipeline
{
    public class DeferredPipeline : RenderPipeline
    {
        private DeferredPipelineAsset _pipelineAsset;
        public DeferredPipeline(DeferredPipelineAsset asset)
        {
            _pipelineAsset = asset;
            GraphicsSettings.useScriptableRenderPipelineBatching = asset.UseSRPBatcher;
        }
        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            using var profScope = new ProfilingScope(null, ProfilingSampler.Get(URPProfileId.UniversalRenderTotal));
            for (int i = 0; i < cameras.Length; i++) {
                _pipelineAsset.renderer.Render(ref context, ref cameras[i]);
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
        internal enum URPProfileId
        {
            // CPU
            UniversalRenderTotal,
            UpdateVolumeFramework,
            RenderCameraStack,

            // GPU
            AdditionalLightsShadow,
            ColorGradingLUT,
            CopyColor,
            CopyDepth,
            DepthNormalPrepass,
            DepthPrepass,

            // DrawObjectsPass
            DrawOpaqueObjects,
            DrawTransparentObjects,

            // RenderObjectsPass
            //RenderObjects,

            LightCookies,

            MainLightShadow,
            ResolveShadows,
            SSAO,

            // PostProcessPass
            StopNaNs,
            SMAA,
            GaussianDepthOfField,
            BokehDepthOfField,
            MotionBlur,
            PaniniProjection,
            UberPostProcess,
            Bloom,
            LensFlareDataDriven,
            MotionVectors,

            FinalBlit
        }
    }
    
    /// <summary>
    /// 渲染信息，由pipeline传给pass用的
    /// </summary>
    public struct RenderingData
    {
        public Camera camera;
        public CullingResults cullingResults;
        public RenderTargetIdentifier cameraColorAttachment;//颜色buffer
        public RenderTargetIdentifier cameraDepthAttachment;//深度buffer
    }
}

