using UnityEngine;
using UnityEngine.Rendering;

namespace DefferedPipeline
{
    [CreateAssetMenu(menuName = "Rendering/AHD2 Deffered Pipeline")]
    public class DeferredPipelineAsset : RenderPipelineAsset
    {
        protected override RenderPipeline CreatePipeline()
        {
            return new DeferredPipeline();
        }
    }
}

