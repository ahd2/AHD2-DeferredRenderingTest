#ifndef GBUFFER_PASS_INCLUDED
#define GBUFFER_PASS_INCLUDED
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/EntityLighting.hlsl"
#include "Assets/ShaderLibrary/BRDF.hlsl"
#include "Assets/ShaderLibrary/Lighting.hlsl"
struct appdata
{
    float4 positionOS : POSITION;
    float2 uv : TEXCOORD0;
    float3 normalOS : NORMAL;
    float3 tangentOS : TANGENT;
};

struct v2f
{
    float2 uv : TEXCOORD0;
    float4 positionCS : SV_POSITION;
    float3 positionWS : TEXCOORD1;
    float3 normalWS  : TEXCOORD2;
    float3 tangentWS  : TEXCOORD3;
    float3 bitangentWS  : TEXCOORD4;
    float3 vertexSH : TEXCOORD5;
};

struct output
{
    half4 GT0 : SV_Target0;
    half4 GT1 : SV_Target1;
    half4 GT2 : SV_Target2;
    half4 GT3 : SV_Target3;
};

// Samples SH L0, L1 and L2 terms
half3 SampleSH(half3 normalWS)
{
    // LPPV is not supported in Ligthweight Pipeline
    real4 SHCoefficients[7];
    SHCoefficients[0] = unity_SHAr;
    SHCoefficients[1] = unity_SHAg;
    SHCoefficients[2] = unity_SHAb;
    SHCoefficients[3] = unity_SHBr;
    SHCoefficients[4] = unity_SHBg;
    SHCoefficients[5] = unity_SHBb;
    SHCoefficients[6] = unity_SHC;

    return max(half3(0, 0, 0), SampleSH9(SHCoefficients, normalWS));
}

v2f vert (appdata v)
{
    v2f o;
    o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
    o.tangentWS = TransformObjectToWorldDir(v.tangentOS);
    o.normalWS = TransformObjectToWorldNormal(v.normalOS);//向量记得在片元归一化
    o.bitangentWS = cross(o.normalWS, o.tangentWS);
    o.positionWS = TransformObjectToWorld(v.positionOS.xyz);
    o.vertexSH = SampleSH(o.normalWS);
    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
    return o;
}

output frag (v2f i)
{
    i.normalWS = normalize(i.normalWS);
    i.tangentWS = normalize(i.tangentWS);
    i.bitangentWS = normalize(i.bitangentWS);
    half4 Albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
    //采样法线贴图(后续封装
    half4 NormalMap = SAMPLE_TEXTURE2D(_BumpMap, sampler_MainTex, i.uv);
    NormalMap.xyz = UnpackNormal(NormalMap);

    half3x3 TBN = half3x3(i.tangentWS, i.bitangentWS, i.normalWS);
    half3 normalWS = TransformTangentToWorld(NormalMap.xyz, TBN);
    
    half4 MAODSMap = SAMPLE_TEXTURE2D(_MetallicGlossMap, sampler_MainTex, i.uv);

    half3 viewDirWS = normalize(_WorldSpaceCameraPos.xyz - i.positionWS.xyz);
    half3 reflectDirWS = reflect( -viewDirWS, normalWS);
    half3 specular = DecodeHDREnvironment(SAMPLE_TEXTURECUBE_LOD(unity_SpecCube0, samplerunity_SpecCube0, reflectDirWS, (1 - MAODSMap.a) * 7), unity_SpecCube0_HDR);

    half NoV = saturate(dot(normalWS, viewDirWS));
    half3 GI = AmbientLighting(i.vertexSH, specular, Albedo.xyz, MAODSMap, NoV);
    //Gbuffer输出
    output output;
    output.GT0 = Albedo;
    output.GT1 = half4(normalWS * 0.5 + 0.5, 1); //
    output.GT2 = MAODSMap; //Metallic/AO/DetailMap/Smothness
    output.GT3 = half4(GI, 1);
    return output;
}
#endif