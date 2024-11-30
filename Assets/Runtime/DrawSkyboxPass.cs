using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace DefferedPipeline
{
    public class DrawSkyboxPass : RenderPass
    {
        public static readonly ProfilingSampler drawSkybox = new ProfilingSampler("SkyBoxPass");
        
        public override void OnCameraSetup(CommandBuffer cmd)
        {
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
        }

        public override void Execute(ScriptableRenderContext context, RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, drawSkybox))
            {
                context.DrawSkybox(renderingData.camera);
            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}
