using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace DefferedPipeline
{
    public abstract partial class RenderPass
    {
        public abstract void OnCameraSetup(CommandBuffer cmd);
        public abstract void OnCameraCleanup(CommandBuffer cmd);
        public abstract void Execute(ScriptableRenderContext context, RenderingData renderingData);
    }
}