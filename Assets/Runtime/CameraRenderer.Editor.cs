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
        public static readonly ProfilingSampler execute = new ProfilingSampler($"{bufferName}.{nameof(Execute)}");
        public static readonly ProfilingSampler startRendering = new ProfilingSampler($"{bufferName}.{nameof(StartRendering)}");
        public static readonly ProfilingSampler finishRendering = new ProfilingSampler($"{bufferName}.{nameof(FinishRendering)}");

        //pass部分
        

        public static readonly ProfilingSampler finalBlit = new ProfilingSampler($"{bufferName}.{nameof(FinalBlit)}");
    }
}