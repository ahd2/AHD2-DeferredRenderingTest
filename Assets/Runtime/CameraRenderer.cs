using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using ProfilingScope = Unity.VisualScripting.ProfilingScope;

public class CameraRenderer
{
    ScriptableRenderContext context;
    private Camera camera;
    CullingResults cullingResults;//全局使用
    
    //==============================================
    
    const string bufferName = "AHD2 Render Camera";
#if UNITY_EDITOR
    string SampleName { get; set; }
#else
    string SampleName => bufferName;
#endif
    
    ProfilingSampler profilingSampler = new ProfilingSampler("profilingSampler");
	
    CommandBuffer buffer = new CommandBuffer {
        name = bufferName
    };
    
    //==============================================
    
    static ShaderTagId GbufferShaderTagId = new ShaderTagId("Gbuffer");
    static int[] GbufferNameIds = new int[]
    {
        Shader.PropertyToID("Gbuffer0"),
        Shader.PropertyToID("Gbuffer1"),
        Shader.PropertyToID("Gbuffer2"),
        Shader.PropertyToID("Gbuffer3"),
    };
    //这样其实是不安全的，如果GbufferIds在GbufferNameIds之前初始化，那么就会有错误了。但是据说C#中是按顺序初始化的
    static RenderTargetIdentifier[] GbufferIds = new RenderTargetIdentifier[]
    {
        new RenderTargetIdentifier(GbufferNameIds[0]),
        new RenderTargetIdentifier(GbufferNameIds[1]),
        new RenderTargetIdentifier(GbufferNameIds[2]),
        new RenderTargetIdentifier(GbufferNameIds[3])
    };

    static int DepthId = Shader.PropertyToID("GbufferDepth");

    const string GbufferPass = "GbufferPass";
    const string DeferredPass = "DeferredPass";
    
    // 创建纹理
    

    //==============================================
    
    public void Render (ref ScriptableRenderContext context, ref Camera camera) 
    {
        //这两个东西，其他很多函数都要调用，与其写成每个函数都要接受这两个输入，不如直接把他们做成成员变量
        this.context = context;
        this.camera = camera;
        if (!Cull()) {
            return;
        }
        Setup();
        DrawGBuffer();
        DeferredLit();
        
        DrawSkyBox();
        Submit();
    }

    private void DeferredLit()
    {
        buffer.name = DeferredPass;
        using (new UnityEngine.Rendering.ProfilingScope(buffer, profilingSampler))
        {
            //输入逻辑
        }
        buffer.name = bufferName;
        ExecuteBuffer();
    }

    private void DrawGBuffer()
    {
        buffer.name = GbufferPass;
        using (new UnityEngine.Rendering.ProfilingScope(buffer, profilingSampler))
        {
            RenderTextureDescriptor gbufferdesc =
                new RenderTextureDescriptor(camera.scaledPixelWidth, camera.scaledPixelHeight);
            gbufferdesc.depthBufferBits = 0; //确保没有深度buffer
            gbufferdesc.stencilFormat = GraphicsFormat.None; //模板缓冲区不指定格式
            gbufferdesc.graphicsFormat = QualitySettings.activeColorSpace == ColorSpace.Linear
                ? GraphicsFormat.R8G8B8A8_SRGB
                : GraphicsFormat.R8G8B8A8_UNorm; //根据颜色空间来决定diffusebuffer的RT格式
            buffer.GetTemporaryRT(GbufferNameIds[0], gbufferdesc); //Albedo
            gbufferdesc.graphicsFormat = GraphicsFormat.R8G8B8A8_UNorm;
            buffer.GetTemporaryRT(GbufferNameIds[1], gbufferdesc); //normal+roughness
            gbufferdesc.graphicsFormat = GraphicsFormat.R8G8B8A8_UNorm;
            buffer.GetTemporaryRT(GbufferNameIds[2], gbufferdesc); //metal+AO+？+？
            gbufferdesc.graphicsFormat = GraphicsFormat.R8G8B8A8_UNorm;
            buffer.GetTemporaryRT(GbufferNameIds[3], gbufferdesc); //暂时不懂干啥了
            buffer.GetTemporaryRT(DepthId, camera.scaledPixelWidth, camera.scaledPixelHeight, 32, FilterMode.Point,
                RenderTextureFormat.Depth, RenderTextureReadWrite.Linear);
            //buffer.SetRenderTarget(GbufferIds, new RenderTargetIdentifier(DepthId));
            buffer.SetRenderTarget(GbufferIds, BuiltinRenderTextureType.CameraTarget);
            ExecuteBuffer();

            var sortingSettings = new SortingSettings(camera)
            {
                //调整绘制顺序,按不透明顺序（从近往远）
                criteria = SortingCriteria.CommonOpaque
            };
            var drawingSettings = new DrawingSettings(GbufferShaderTagId, sortingSettings);
            drawingSettings.SetShaderPassName(1, GbufferShaderTagId); //绘制光照着色器
            var filteringSettings = new FilteringSettings(RenderQueueRange.opaque); //渲染不透明队列的物体。
            //核心绘制函数
            context.DrawRenderers(
                cullingResults, ref drawingSettings, ref filteringSettings
            );

            //绘制完后切换rendertarget
            buffer.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);

            //设为全局texture（绘制前设置也能有效，但是保险起见还是绘制后设置。
            for (int i = 0; i < 4; i++)
                buffer.SetGlobalTexture("_GT" + i, GbufferNameIds[i]);
            buffer.SetGlobalTexture(DepthId, DepthId);
        }
        buffer.name = bufferName;
        ExecuteBuffer();
    }

    private void BeginSampler(int gbufferPassId)
    {
        
    }

    private void Submit()
    {
        buffer.ReleaseTemporaryRT(GbufferNameIds[0]);
        buffer.ReleaseTemporaryRT(GbufferNameIds[1]);
        buffer.ReleaseTemporaryRT(GbufferNameIds[2]);
        buffer.ReleaseTemporaryRT(GbufferNameIds[3]);
        buffer.ReleaseTemporaryRT(DepthId);
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
        
        CameraClearFlags flags = camera.clearFlags;
        buffer.ClearRenderTarget(
            flags <= CameraClearFlags.Depth,
            flags == CameraClearFlags.Color,
            flags == CameraClearFlags.Color ?
                camera.backgroundColor.linear : Color.clear
        );
        buffer.BeginSample(SampleName);
        //Debug.Log("BeginSample名字是："+bufferName + "而实际buffername是：" + buffer.name);
        ExecuteBuffer();
    }
    
    void ExecuteBuffer () {
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }
    
    bool Cull () {
        if (camera.TryGetCullingParameters(out ScriptableCullingParameters p)) {
            cullingResults = context.Cull(ref p);
            return true;
        }
        return false;
    }
    
    
}
