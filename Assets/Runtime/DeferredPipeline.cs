using UnityEngine;
using UnityEngine.Rendering;

namespace DefferedPipeline
{
    public class DeferredPipeline : RenderPipeline
    {
        public DeferredPipeline(bool useSRPBatcher)
        {
            GraphicsSettings.useScriptableRenderPipelineBatching = useSRPBatcher;
        }
        CameraRenderer renderer = new CameraRenderer();//用renderer来管理一整个渲染管线
        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            for (int i = 0; i < cameras.Length; i++) {
                renderer.Render(ref context, ref cameras[i]);
            }
        }
    }
}

