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
        //非pass部分
        public static readonly ProfilingSampler setup = new ProfilingSampler($"{bufferName}.{nameof(Setup)}");
        public static readonly ProfilingSampler submit = new ProfilingSampler($"{bufferName}.{nameof(Submit)}");

        //pass部分
        public static readonly ProfilingSampler drawGbuffer =
            new ProfilingSampler($"{bufferName}.{nameof(DrawGBuffer)}");

        public static readonly ProfilingSampler finalBlit = new ProfilingSampler($"{bufferName}.{nameof(FinalBlit)}");
    }
}