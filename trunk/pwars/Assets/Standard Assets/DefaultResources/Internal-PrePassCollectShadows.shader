// Collects cascaded shadows into screen space buffer ready for blurring
Shader "Hidden/Internal-PrePassCollectShadows" {
Properties {
	_ShadowMapTexture ("", any) = "" {}
}
SubShader {
Pass {
	ZWrite Off ZTest Always Cull Off Fog { Mode Off }

CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma multi_compile_shadowcollector

#include "UnityCG.cginc"
struct appdata {
	float4 vertex : POSITION;
	float2 texcoord : TEXCOORD0;
	float3 texcoord1 : TEXCOORD1;
};

struct v2f {
	float4 pos : SV_POSITION;
	float2 uv : TEXCOORD0;
	float3 ray : TEXCOORD1;
};

v2f vert (appdata v)
{
	v2f o;
	o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
	o.uv = v.texcoord;
	o.ray = v.texcoord1;
	return o;
}
sampler2D _CameraDepthTexture;
float4 unity_LightmapFade;

float4x4 _CameraToWorld;
float4x4 unity_World2Shadow;
float4x4 unity_World2Shadow1;
float4x4 unity_World2Shadow2;
float4x4 unity_World2Shadow3;
float4 _LightSplitsNear;
float4 _LightSplitsFar;
float4 _LightShadowData;
sampler2D _ShadowMapTexture;

inline half unitySampleShadow (float4 wpos, float z)
{
	float3 sc0 = mul (unity_World2Shadow, wpos).xyz;
	float3 sc1 = mul (unity_World2Shadow1, wpos).xyz;
	float3 sc2 = mul (unity_World2Shadow2, wpos).xyz;
	float3 sc3 = mul (unity_World2Shadow3, wpos).xyz;
	
	float4 near = float4( z >= _LightSplitsNear );
	float4 far = float4( z < _LightSplitsFar );
	float4 weights = near * far;
	float4 coord = float4(sc0 * weights[0] + sc1 * weights[1] + sc2 * weights[2] + sc3 * weights[3], 1);
	#if defined (SHADOWS_NATIVE) && !defined (SHADER_API_OPENGL)
	half shadow = tex2Dproj (_ShadowMapTexture, UNITY_PROJ_COORD(coord)).r;
	shadow = _LightShadowData.r + shadow * (1-_LightShadowData.r);
	#else
	half shadow = tex2D (_ShadowMapTexture, coord.xy).r < coord.z ? _LightShadowData.r : 1.0;
	#endif
	return shadow;
}

half4 frag (v2f i) : COLOR
{
	float depth = tex2D (_CameraDepthTexture, i.uv).r;
	depth = Linear01Depth (depth);
	float4 vpos = float4(i.ray * depth,1);
	float4 wpos = mul (_CameraToWorld, vpos);	
	half shadow = unitySampleShadow (wpos, vpos.z);
	float4 res;
	res.x = shadow;
	res.y = 1.0;
	res.zw = EncodeFloatRG (1 - depth);
	return res;	
}

ENDCG
}

}
Fallback Off
}
