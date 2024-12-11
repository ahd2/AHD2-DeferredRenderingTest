using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DefferedPipeline
{
    public class DrawTransparentPass : RenderPass
    {
        public static readonly ProfilingSampler transparentPass = new ProfilingSampler("TransparentPass");
        private static ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");
        
        public override void OnCameraSetup(CommandBuffer cmd)
        {
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
        }

        public override void Execute(ScriptableRenderContext context, RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, transparentPass))
            {
                //决定物体绘制顺序是正交排序还是基于深度排序的配置
                var sortingSettings = new SortingSettings(renderingData.camera)
                {
                    criteria = SortingCriteria.CommonTransparent
                };
                var drawingSettings = new DrawingSettings(unlitShaderTagId, sortingSettings);
                var filteringSettings = new FilteringSettings(RenderQueueRange.transparent);
                context.DrawRenderers(renderingData.cullingResults, ref drawingSettings, ref filteringSettings);
            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}
