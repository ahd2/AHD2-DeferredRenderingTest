//存储Shader中的一些常用的输入数据
#ifndef CUSTOM_UNITY_INPUT_INCLUDED
#define CUSTOM_UNITY_INPUT_INCLUDED
CBUFFER_START(UnityPerDraw)
float4x4 unity_ObjectToWorld;
float4x4 unity_WorldToObject;
float4 unity_LODFade;
real4 unity_WorldTransformParams;
float4 unity_SpecCube0_HDR;
// SH lighting environment
half4 unity_SHAr;
half4 unity_SHAg;
half4 unity_SHAb;
half4 unity_SHBr;
half4 unity_SHBg;
half4 unity_SHBb;
half4 unity_SHC;
CBUFFER_END

float4x4 unity_MatrixVP;
float4x4 unity_MatrixV;
float4x4 glstate_matrix_projection;
float3 _WorldSpaceCameraPos;

#endif