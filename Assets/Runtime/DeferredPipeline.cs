using UnityEngine;
using UnityEngine.Rendering;

namespace DefferedPipeline
{
    public class DeferredPipeline : RenderPipeline
    {
        RenderTexture gdepth;                                               // depth attachment
        RenderTexture[] gbuffers = new RenderTexture[4];                    // color attachments 
        RenderTargetIdentifier[] gbufferID = new RenderTargetIdentifier[4]; // tex ID 
        public DeferredPipeline(bool useSRPBatcher)
        {
            gdepth  = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.Depth, RenderTextureReadWrite.Linear);
            gbuffers[0] = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            gbuffers[1] = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB2101010, RenderTextureReadWrite.Linear);
            gbuffers[2] = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB64, RenderTextureReadWrite.Linear);
            gbuffers[3] = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            // 给纹理 ID 赋值
            for(int i=0; i<4; i++)
                gbufferID[i] = gbuffers[i];
            GraphicsSettings.useScriptableRenderPipelineBatching = useSRPBatcher;
        }
        CameraRenderer renderer = new CameraRenderer();//用renderer来管理一整个渲染管线
        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            for (int i = 0; i < cameras.Length; i++) {
                renderer.Render(ref context, ref cameras[i], ref gdepth, ref gbuffers, ref gbufferID);
            }
        }
    }
}

