Shader "Hidden/Nature/Tree Creator Leaves Optimized" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_TranslucencyColor ("Translucency Color", Color) = (0.73,0.85,0.41,1) // (187,219,106,255)
	_Cutoff ("Alpha cutoff", Range(0,1)) = 0.3
	_TranslucencyViewDependency ("View dependency", Range(0,1)) = 0.7
	_ShadowStrength("Shadow Strength", Range(0,1)) = 0.8
	_ShadowOffsetScale ("Shadow Offset Scale", Float) = 1
	
	_MainTex ("Base (RGB) Alpha (A)", 2D) = "white" {}
	_ShadowTex ("Shadow (RGB)", 2D) = "white" {}
	_BumpSpecMap ("Normalmap (GA) Spec (R) Shadow Offset (B)", 2D) = "bump" {}
	_TranslucencyMap ("Trans (B) Gloss(A)", 2D) = "white" {}

	// These are here only to provide default values
	_Scale ("Scale", Vector) = (1,1,1,1)
	_SquashAmount ("Squash", Float) = 1
}

SubShader { 
	Tags {
		"IgnoreProjector"="True"
		"RenderType"="TreeLeaf"
	}
	LOD 200
	
CGPROGRAM
#pragma surface surf TreeLeaf alphatest:_Cutoff vertex:TreeVertLeaf nolightmap
#include "Tree.cginc"

sampler2D _MainTex;
sampler2D _BumpSpecMap;
sampler2D _TranslucencyMap;

struct Input {
	float2 uv_MainTex;
	float4 color : COLOR; // color.a = AO
};

void surf (Input IN, inout LeafSurfaceOutput o) {
	half4 c = tex2D(_MainTex, IN.uv_MainTex);
	o.Albedo = c.rgb * _Color.rgb * IN.color.a;
	
	half4 trngls = tex2D (_TranslucencyMap, IN.uv_MainTex);
	o.Translucency = trngls.b;
	o.Gloss = trngls.a * _Color.r;
	o.Alpha = c.a;
	
	half4 norspc = tex2D (_BumpSpecMap, IN.uv_MainTex);
	o.Specular = norspc.r;
	o.ShadowOffset = norspc.b;
	o.Normal = UnpackNormal(norspc);
}
ENDCG

	// Pass to render object as a shadow caster
	Pass {
		Name "ShadowCaster"
		Tags { "LightMode" = "ShadowCaster" }
		
		Fog {Mode Off}
		ZWrite On ZTest Less Cull Off
		Offset 1, 1

CGPROGRAM
#pragma vertex vert_surf
#pragma fragment frag_surf
#pragma fragmentoption ARB_precision_hint_fastest
#pragma multi_compile_shadowcaster
#include "HLSLSupport.cginc"
#include "UnityCG.cginc"
#include "Lighting.cginc"

#define INTERNAL_DATA
#define WorldReflectionVector(data,normal) data.worldRefl
#line 22

#include "Tree.cginc"

sampler2D _ShadowTex;

struct Input {
	float2 uv_MainTex;
};

struct v2f_surf {
	V2F_SHADOW_CASTER;
	float2 hip_pack0 : TEXCOORD1;
};
float4 _ShadowTex_ST;
v2f_surf vert_surf (appdata_full v) {
	v2f_surf o;
	TreeVertLeaf (v);
	o.hip_pack0.xy = TRANSFORM_TEX(v.texcoord, _ShadowTex);
	TRANSFER_SHADOW_CASTER(o)
	return o;
}
float _Cutoff;
half4 frag_surf (v2f_surf IN) : COLOR {
	half alpha = tex2D(_ShadowTex, IN.hip_pack0.xy).r;
	clip (alpha - _Cutoff);
	SHADOW_CASTER_FRAGMENT(IN)
}

ENDCG

	}
	
	// Pass to render object as a shadow collector
	Pass {
		Name "ShadowCollector"
		Tags { "LightMode" = "ShadowCollector" }
		
		Fog {Mode Off}
		ZWrite On ZTest Less

CGPROGRAM
#pragma vertex vert_surf
#pragma fragment frag_surf
#pragma fragmentoption ARB_precision_hint_fastest
#pragma multi_compile_shadowcollector
#include "HLSLSupport.cginc"
#define SHADOW_COLLECTOR_PASS
#include "UnityCG.cginc"
#include "Lighting.cginc"

#define INTERNAL_DATA
#define WorldReflectionVector(data,normal) data.worldRefl
#line 22

#include "Tree.cginc"

sampler2D _MainTex;
sampler2D _BumpSpecMap;
sampler2D _TranslucencyMap;
float _ShadowOffsetScale;

struct Input {
	float2 uv_MainTex;
};

void surf (Input IN, inout LeafSurfaceOutput o) {
	half4 c = tex2D(_MainTex, IN.uv_MainTex);
	o.Alpha = c.a;
	
	half4 norspc = tex2D (_BumpSpecMap, IN.uv_MainTex);
	o.ShadowOffset = norspc.b;
}
struct v2f_surf {
	V2F_SHADOW_COLLECTOR;
	float2 hip_pack0 : TEXCOORD5;
	float3 normal : TEXCOORD6;
};
float4 _MainTex_ST;
v2f_surf vert_surf (appdata_full v) {
	v2f_surf o;
	TreeVertLeaf (v);
	o.hip_pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
	
	float3 worldN = mul((float3x3)_Object2World, SCALED_NORMAL);
	o.normal = mul(_World2Shadow, half4(worldN, 0)).xyz;

	TRANSFER_SHADOW_COLLECTOR(o)
	return o;
}
float _Cutoff;
half4 frag_surf (v2f_surf IN) : COLOR {
	half alpha = tex2D(_MainTex, IN.hip_pack0.xy).a;

	float3 shadowOffset = _ShadowOffsetScale * IN.normal * tex2D (_BumpSpecMap, IN.hip_pack0.xy).b;
	clip (alpha - _Cutoff);

	IN._ShadowCoord0 += shadowOffset;
	IN._ShadowCoord1 += shadowOffset;
	IN._ShadowCoord2 += shadowOffset;
	IN._ShadowCoord3 += shadowOffset;

	SHADOW_COLLECTOR_FRAGMENT(IN)
}

ENDCG

	}

}

SubShader {
	Tags {
		"IgnoreProjector"="True"
		"RenderType"="TreeLeaf"
	}
	
	ColorMask RGB
	Lighting On
	
	Pass {
		CGPROGRAM
		#pragma vertex TreeVertLit
		#pragma exclude_renderers gles xbox360 ps3
		#include "UnityCG.cginc"
		#include "Tree.cginc"
		struct v2f {
			float4 pos : SV_POSITION;
			float4 color : COLOR;
			float4 uv : TEXCOORD0;
		};
		v2f TreeVertLit (appdata_full v) {
			v2f o;
			TreeVertLeaf(v);

			o.color.rgb = ShadeVertexLights (v.vertex, v.normal);
				
			o.pos = mul (UNITY_MATRIX_MVP, v.vertex);	
			o.uv = v.texcoord;
			o.color.a = 1.0f;
			return o;
		}
		ENDCG

		AlphaTest Greater [_Cutoff]
		SetTexture [_MainTex] { combine texture * primary DOUBLE, texture }
		SetTexture [_MainTex] {
			ConstantColor [_Color]
			Combine previous * constant, previous
		} 
	}
}

SubShader {
	Tags { "RenderType"="TreeLeaf" }
	Pass {
		ColorMask RGB
		
		Material {
			Diffuse (1,1,1,1)
			Ambient (1,1,1,1)
		}
		Lighting On
		
		AlphaTest Greater [_Cutoff]
		SetTexture [_MainTex] { Combine texture * primary DOUBLE, texture }
		SetTexture [_MainTex] {
			ConstantColor [_Color]
			Combine previous * constant, previous
		} 
	}
} 

Dependency "BillboardShader" = "Hidden/Nature/Tree Creator Leaves Rendertex"
}
