using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace DefferedPipeline
{
    public class DeferredLitPass : RenderPass
    {
        public static readonly ProfilingSampler deferredLit = new ProfilingSampler("DeferredLitPass");
        //这样也是不合理的，具体还是可以参考URP的做法。用一个类和SO来封装。
        private Material _deferredLitMat = new Material(Shader.Find("DefferedrLighting"));
        
        public override void OnCameraSetup(CommandBuffer cmd)
        {
            
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
        }

        public override void Execute(ScriptableRenderContext context, RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, deferredLit))
            {
                cmd.Blit(GbufferPass.GbufferIds[0], renderingData.cameraColorAttachment, _deferredLitMat, 0);
                //绘制完后切换rendertarget
                cmd.SetRenderTarget(renderingData.cameraColorAttachment, renderingData.cameraDepthAttachment);
            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}
