using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using ProfilingScope = UnityEngine.Rendering.ProfilingScope;

namespace DefferedPipeline
{
    public partial class CameraRenderer
    {

        ScriptableRenderContext context;
        private Camera camera;
        CullingResults cullingResults; //全局使用
        RenderingData renderingData;//全局使用

        //==============================================

        const string bufferName = "AHD2 Render Camera";
        partial void PrepareBuffer();
#if UNITY_EDITOR
        string SampleName { get; set; }
        partial void PrepareBuffer()
        {
            buffer.name = SampleName = camera.name;
        }
#else
    string SampleName => bufferName;

#endif

        CommandBuffer buffer = new CommandBuffer
        {
            name = bufferName
        };

        //创建一个全局RTdesc来描述相机目标RT格式
        private RenderTextureDescriptor cameraRenderTextureDescriptor;

        //维护自己的buffer
        private static int _cameraColorAttachmentId = Shader.PropertyToID("CameraColorAttachment");
        private static int _cameraDepthAttachmentId = Shader.PropertyToID("CameraDepthAttachment");
        private RenderTargetIdentifier _cameraColorAttachment = new RenderTargetIdentifier(_cameraColorAttachmentId);
        private RenderTargetIdentifier _cameraDepthAttachment = new RenderTargetIdentifier(_cameraDepthAttachmentId);

        //==============================================Gbuffer相关


        const string GbufferPass = "GbufferPass";
        const string DeferredPass = "DeferredPass";
        const string FinalBlitPass = "FinalBlitPass";

        //==============================================
        List<RenderPass> m_ActiveRenderPassQueue = new List<RenderPass>(32);

        public CameraRenderer()
        {
            Debug.Log("renderer被构造了");
        }

        public void Render(ref ScriptableRenderContext context, ref Camera camera)
        {
            //这两个东西，其他很多函数都要调用，与其写成每个函数都要接受这两个输入，不如直接把他们做成成员变量
            this.context = context;
            this.camera = camera;
            //为不同相机创建不同名字的buffer（在framedebugger里面可以看到。）

            using (new ProfilingScope(null, setup))
            {
                PrepareBuffer();

                this.cameraRenderTextureDescriptor =
                    new RenderTextureDescriptor(camera.scaledPixelWidth, camera.scaledPixelHeight);
                cameraRenderTextureDescriptor.graphicsFormat = QualitySettings.activeColorSpace == ColorSpace.Linear
                    ? GraphicsFormat.R8G8B8A8_SRGB
                    : GraphicsFormat.R8G8B8A8_UNorm;

                if (!Cull())
                {
                    return;
                }

                //配置相机参数和渲染目标，然后清理渲染目标，然后添加各个Pass
                Setup();
            }

            //调用各个Pass的OnCameraSetup方法
            StartRendering();

            //执行各个Pass的Execute部分
            Execute();

            //DrawGBuffer();

            DeferredLit();

            DrawSkyBox();

            FinalBlit();

            //调用各个Pass的OnCameraCleanup方法
            FinishRendering();

            using (new ProfilingScope(null, submit))
            {
                Submit();
            }
        }
        
        private void StartRendering()
        {
            var cmd = CommandBufferPool.Get();
            using (new ProfilingScope(null, startRendering))
            {
                for (int i = 0; i < m_ActiveRenderPassQueue.Count; i++)
                {
                    m_ActiveRenderPassQueue[i].OnCameraSetup(cmd);
                }
            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        private void Execute()
        {
            using (new ProfilingScope(null, execute))
            {
                for (int i = 0; i < m_ActiveRenderPassQueue.Count; i++)
                {
                    m_ActiveRenderPassQueue[i].Execute(context, renderingData);
                }
            }
        }
        
        private void FinishRendering()
        {
            var cmd = CommandBufferPool.Get();
            using (new ProfilingScope(null, finishRendering))
            {
                for (int i = 0; i < m_ActiveRenderPassQueue.Count; i++)
                {
                    m_ActiveRenderPassQueue[i].OnCameraCleanup(cmd);
                }
            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        private void FinalBlit()
        {
            var cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, finalBlit))
            {
                cmd.Blit(_cameraColorAttachment, BuiltinRenderTextureType.CameraTarget);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        private void DeferredLit()
        {

        }

        private void Submit()
        {
            buffer.ReleaseTemporaryRT(_cameraColorAttachmentId);
            buffer.ReleaseTemporaryRT(_cameraDepthAttachmentId);
            buffer.EndSample(SampleName);
            //Debug.Log("EndSample名字是："+bufferName + "而实际buffername是：" + buffer.name);
            ExecuteBuffer();
            context.Submit();
        }

        private void DrawSkyBox()
        {
            context.DrawSkybox(camera);
        }

        private void Setup()
        {
            //更新相机属性,顺便传framebuffer地址，方便ClearRenderTarget清理屏幕。
            context.SetupCameraProperties(camera);
            //设置自己维护的RenderTarget
            //深度buffer其实可以不设为0然后就用这一个作为RenderTarget
            buffer.GetTemporaryRT(_cameraColorAttachmentId, camera.scaledPixelWidth, camera.scaledPixelHeight, 0,
                FilterMode.Point,
                QualitySettings.activeColorSpace == ColorSpace.Linear
                    ? GraphicsFormat.R8G8B8A8_SRGB
                    : GraphicsFormat.R8G8B8A8_UNorm);
            buffer.GetTemporaryRT(_cameraDepthAttachmentId, camera.scaledPixelWidth, camera.scaledPixelHeight, 32,
                FilterMode.Point,
                RenderTextureFormat.Depth, RenderTextureReadWrite.Linear);
            buffer.SetRenderTarget(_cameraColorAttachment, RenderBufferLoadAction.DontCare,
                RenderBufferStoreAction.Store,
                _cameraDepthAttachment, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
            ExecuteBuffer();
            //清理相机默认RenderTarget
            CameraClearFlags flags = camera.clearFlags;
            buffer.ClearRenderTarget(
                flags <= CameraClearFlags.Depth,
                flags <= CameraClearFlags.Color,
                flags == CameraClearFlags.Color ? camera.backgroundColor.linear : Color.clear
            );
            buffer.BeginSample(SampleName);

            //Debug.Log("BeginSample名字是："+bufferName + "而实际buffername是：" + buffer.name);
            ExecuteBuffer();
            
            //配置RenderingData（模仿URP
            InitializeRenderingData();
            //初始化并添加pass
            GbufferPass gbufferPass = new GbufferPass();
            //m_ActiveRenderPassQueue.Add(gbufferPass);
        }

        private void InitializeRenderingData()
        {
            //后续应该取消camera，cullingResults这些的全局变量，只保留RenderingData
            renderingData.camera = camera;
            renderingData.cullingResults = cullingResults;
            renderingData.cameraColorAttachment = _cameraColorAttachment;
            renderingData.cameraDepthAttachment = _cameraDepthAttachment;
        }

        void ExecuteBuffer()
        {
            context.ExecuteCommandBuffer(buffer);
            buffer.Clear();
        }

        bool Cull()
        {
            if (camera.TryGetCullingParameters(out ScriptableCullingParameters p))
            {
                cullingResults = context.Cull(ref p);
                return true;
            }

            return false;
        }
    }
}