using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

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
    
    
    
    // 创建纹理
    

    //==============================================
    
    public void Render (ref ScriptableRenderContext context, ref Camera camera, ref RenderTexture gdepth, ref RenderTexture[] gbuffers, ref RenderTargetIdentifier[] gbufferID) 
    {
        //这两个东西，其他很多函数都要调用，与其写成每个函数都要接受这两个输入，不如直接把他们做成成员变量
        this.context = context;
        this.camera = camera;
        if (!Cull()) {
            return;
        }
        Setup();
        
        buffer.SetRenderTarget(gbufferID, gdepth);
        CameraClearFlags flags = camera.clearFlags;
        buffer.ClearRenderTarget(
            flags <= CameraClearFlags.Depth,
            flags == CameraClearFlags.Color,
            flags == CameraClearFlags.Color ?
                camera.backgroundColor.linear : Color.clear
        );
        ExecuteBuffer();
        
        // RenderTextureDescriptor gbufferdesc = new RenderTextureDescriptor(Screen.width, Screen.height);
        // gbufferdesc.depthBufferBits = 0;//确保没有深度buffer
        // gbufferdesc.stencilFormat = GraphicsFormat.None;//模板缓冲区不指定格式
        // gbufferdesc.graphicsFormat = QualitySettings.activeColorSpace == ColorSpace.Linear
        //     ? GraphicsFormat.R8G8B8A8_SRGB
        //     : GraphicsFormat.R8G8B8A8_UNorm;//根据颜色空间来决定diffusebuffer的RT格式
        // buffer.GetTemporaryRT(GbufferNameIds[0], gbufferdesc);//diffuse
        // gbufferdesc.graphicsFormat = GraphicsFormat.R8G8B8A8_UNorm;
        // buffer.GetTemporaryRT(GbufferNameIds[1], gbufferdesc);//normal+roughness
        // gbufferdesc.graphicsFormat = GraphicsFormat.R8G8B8A8_UNorm;
        // buffer.GetTemporaryRT(GbufferNameIds[2], gbufferdesc);//metal+AO+？+？
        // gbufferdesc.graphicsFormat = GraphicsFormat.R8G8B8A8_UNorm;
        // buffer.GetTemporaryRT(GbufferNameIds[3], gbufferdesc);//暂时不懂干啥了
        // //buffer.SetRenderTarget(GbufferIds, 0);
        
        DrawGBuffer();
        buffer.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
        ExecuteBuffer();
        DrawSkyBox();
        Submit();
    }

    private void DrawGBuffer()
    {
        var sortingSettings = new SortingSettings(camera){
            //调整绘制顺序,按不透明顺序（从近往远）
            criteria = SortingCriteria.CommonOpaque
        };
        var drawingSettings = new DrawingSettings(GbufferShaderTagId, sortingSettings);
        drawingSettings.SetShaderPassName(1, GbufferShaderTagId);//绘制光照着色器
        var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);//渲染不透明队列的物体。
        //核心绘制函数
        context.DrawRenderers(
            cullingResults, ref drawingSettings, ref filteringSettings
        );
    }

    private void Submit()
    {
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
