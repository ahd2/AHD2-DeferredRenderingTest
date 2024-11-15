using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CameraRenderer
{
    ScriptableRenderContext context;
    private Camera camera;
    
    const string bufferName = "AHD2 Render Camera";
#if UNITY_EDITOR
    string SampleName { get; set; }
#else
    string SampleName => bufferName;
#endif
	
    CommandBuffer buffer = new CommandBuffer {
        name = bufferName
    };
    public void Render (ref ScriptableRenderContext context, ref Camera camera) 
    {
        //这两个东西，其他很多函数都要调用，与其写成每个函数都要接受这两个输入，不如直接把他们做成成员变量
        this.context = context;
        this.camera = camera;

        Setup();

        DrawSkyBox();
        Submit();
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
}
