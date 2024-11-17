Shader "DefferedrPBR"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BumpMap("Normap Map", 2D) = "bump" {}
        _MetallicGlossMap("Metallic/AO/D/Smothness", 2D) = "" { }
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "LightMode" = "Gbuffer"}
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Inputs.hlsl"
            #include "GbufferPass.hlsl"
            ENDHLSL
        }
    }
}
