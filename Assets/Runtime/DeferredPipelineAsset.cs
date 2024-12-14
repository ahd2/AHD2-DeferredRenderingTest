using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace DefferedPipeline
{
    [CreateAssetMenu(menuName = "Rendering/AHD2 Deffered Pipeline")]
    public class DeferredPipelineAsset : RenderPipelineAsset
    {
        public bool UseSRPBatcher = true;
        public CameraRenderer renderer;//用renderer来管理一整个渲染管线
        [SerializeField]
        //bool allowHDR = true;
        protected override RenderPipeline CreatePipeline()
        {
            Debug.Log("创建pipeline");
            renderer = new CameraRenderer();
            renderer.iblBrdfLutTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Textures/IBL_BRDF_LUT.png");
            return new DeferredPipeline(this);
        }
    }
}

