using UnityEngine;
using UnityEngine.Rendering;

namespace DefferedPipeline
{
    [CreateAssetMenu(menuName = "Rendering/AHD2 Deffered Pipeline")]
    public class DeferredPipelineAsset : RenderPipelineAsset
    {
        bool UseSRPBatcher = true;
        protected override RenderPipeline CreatePipeline()
        {
            return new DeferredPipeline(UseSRPBatcher);
        }
    }
}

