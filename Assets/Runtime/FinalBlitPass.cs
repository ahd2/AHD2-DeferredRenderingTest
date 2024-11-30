using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace DefferedPipeline
{
    public class FinalBlitPass : RenderPass
    {
        public static readonly ProfilingSampler finalBlit = new ProfilingSampler("FinalBlitPass");
        
        public override void OnCameraSetup(CommandBuffer cmd)
        {
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
        }

        public override void Execute(ScriptableRenderContext context, RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, finalBlit))
            {
                cmd.Blit(renderingData.cameraColorAttachment, BuiltinRenderTextureType.CameraTarget);
            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}
