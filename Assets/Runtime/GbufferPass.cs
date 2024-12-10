using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace DefferedPipeline
{
    public class GbufferPass : RenderPass
    {
        static ShaderTagId GbufferShaderTagId = new ShaderTagId("Gbuffer");

        static int[] GbufferNameIds = new int[]
        {
            Shader.PropertyToID("Gbuffer0"),
            Shader.PropertyToID("Gbuffer1"),
            Shader.PropertyToID("Gbuffer2"),
            Shader.PropertyToID("Gbuffer3"),
        };

        //这样其实是不安全的，如果GbufferIds在GbufferNameIds之前初始化，那么就会有错误了。但是据说C#中是按顺序初始化的
        public static RenderTargetIdentifier[] GbufferIds = new RenderTargetIdentifier[]
        {
            new RenderTargetIdentifier(GbufferNameIds[0]),
            new RenderTargetIdentifier(GbufferNameIds[1]),
            new RenderTargetIdentifier(GbufferNameIds[2]),
            new RenderTargetIdentifier(GbufferNameIds[3])
        };
        
        public static readonly ProfilingSampler drawGbuffer = new ProfilingSampler("GbufferPass");
        
        public override void OnCameraSetup(CommandBuffer cmd)
        {
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            using (new ProfilingScope(cmd, drawGbuffer))
            {
                cmd.ReleaseTemporaryRT(GbufferNameIds[0]);
                cmd.ReleaseTemporaryRT(GbufferNameIds[1]);
                cmd.ReleaseTemporaryRT(GbufferNameIds[2]);
                cmd.ReleaseTemporaryRT(GbufferNameIds[3]);
            }
        }

        public override void Execute(ScriptableRenderContext context, RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, drawGbuffer))
            {
                RenderTextureDescriptor gbufferdesc =
                    new RenderTextureDescriptor(renderingData.camera.scaledPixelWidth, renderingData.camera.scaledPixelHeight);
                gbufferdesc.depthBufferBits = 0; //确保没有深度buffer
                gbufferdesc.stencilFormat = GraphicsFormat.None; //模板缓冲区不指定格式
                gbufferdesc.graphicsFormat = QualitySettings.activeColorSpace == ColorSpace.Linear
                    ? GraphicsFormat.R8G8B8A8_SRGB
                    : GraphicsFormat.R8G8B8A8_UNorm; //根据颜色空间来决定diffusebuffer的RT格式
                cmd.GetTemporaryRT(GbufferNameIds[0], gbufferdesc); //Albedo
                gbufferdesc.graphicsFormat = GraphicsFormat.R8G8B8A8_UNorm;
                cmd.GetTemporaryRT(GbufferNameIds[1], gbufferdesc); //normal
                gbufferdesc.graphicsFormat = GraphicsFormat.R8G8B8A8_UNorm;
                cmd.GetTemporaryRT(GbufferNameIds[2], gbufferdesc); //metal+AO+？+？
                gbufferdesc.graphicsFormat = GraphicsFormat.R8G8B8A8_UNorm;
                cmd.GetTemporaryRT(GbufferNameIds[3], gbufferdesc); //暂时不懂干啥了

                cmd.SetRenderTarget(GbufferIds, renderingData.cameraDepthAttachment);
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();//提交完记得清空

                var sortingSettings = new SortingSettings(renderingData.camera)
                {
                    //调整绘制顺序,按不透明顺序（从近往远）
                    criteria = SortingCriteria.CommonOpaque
                };
                var drawingSettings = new DrawingSettings(GbufferShaderTagId, sortingSettings);
                drawingSettings.SetShaderPassName(1, GbufferShaderTagId); //绘制光照着色器
                var filteringSettings = new FilteringSettings(RenderQueueRange.opaque); //渲染不透明队列的物体。
                //核心绘制函数
                context.DrawRenderers(
                    renderingData.cullingResults, ref drawingSettings, ref filteringSettings
                );

                //设为全局texture（绘制前设置也能有效，但是保险起见还是绘制后设置。
                for (int i = 0; i < 4; i++)
                    cmd.SetGlobalTexture("_GT" + i, GbufferNameIds[i]);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}
