using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DefferedPipeline
{
    public class DrawSkyboxPass : RenderPass
    {
        public static readonly ProfilingSampler drawSkybox = new ProfilingSampler("SkyBoxPass");
        private Material _deferredLitMat = new Material(Shader.Find("PreviewBlit"));
        
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
                if (renderingData.camera.cameraType == CameraType.Preview)
                {
                    Matrix4x4 matrix = new Matrix4x4(
                        new Vector4(1.00000f, 0.00000f, 0.00000f, 0.00000f),
                        new Vector4(0.00000f, 1.00000f, 0.00000f, 0.00000f),
                        new Vector4(0.00000f, 0.00000f, 1.00000f, 0.00000f),
                        new Vector4(0.00000f, 0.00000f, 1.00000f, 1.00000f)
                    );
                    Matrix4x4 originalViewMatrix = renderingData.camera.worldToCameraMatrix;
                    Matrix4x4 originalProjMatrix = renderingData.camera.projectionMatrix;
                    cmd.SetViewProjectionMatrices(Matrix4x4.identity, matrix);
                    cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, _deferredLitMat, 0, 0);
                    cmd.SetViewProjectionMatrices(originalViewMatrix, originalProjMatrix);
                }
                context.DrawSkybox(renderingData.camera);
            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}
