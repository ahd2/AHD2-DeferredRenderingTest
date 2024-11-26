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
            for (int i = 0; i < cameras.Length; i++) {
                _pipelineAsset.renderer.Render(ref context, ref cameras[i]);
            }
        }

        protected override void Dispose(bool disposing)
        {
            Debug.Log("管线被销毁了");
            base.Dispose(disposing);
        }
    }
}

