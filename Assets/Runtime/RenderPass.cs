using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DefferedPipeline
{
    public abstract partial class RenderPass
    {
        public abstract void OnCameraSetup();
        public abstract void OnCameraCleanup();
        public abstract void Execute();
    }
}