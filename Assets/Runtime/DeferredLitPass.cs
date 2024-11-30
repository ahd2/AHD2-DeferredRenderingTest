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
                context.DrawSkybox(renderingData.camera);
            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}
