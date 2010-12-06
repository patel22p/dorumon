Shader "RenderFX/Skybox Cubed" {
Properties {
	_Tint ("Tint Color", Color) = (.5, .5, .5, .5)
	_Tex ("Cubemap", Cube) = "white" {}
}

SubShader {
	Tags { "Queue"="Background" "RenderType"="Background" }
	Cull Off ZWrite Off Fog { Mode Off }

	Pass {
		
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma fragmentoption ARB_precision_hint_fastest

		#include "UnityCG.cginc"

		samplerCUBE _Tex;
		float4 _Tint;
		
		struct appdata_t {
			float4 vertex : POSITION;
			float3 texcoord : TEXCOORD0;
		};

		struct v2f {
			float4 vertex : POSITION;
			float3 texcoord : TEXCOORD0;
		};

		v2f vert (appdata_t v)
		{
			v2f o;
			o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
			o.texcoord = v.texcoord;
			return o;
		}

		half4 frag (v2f i) : COLOR
		{
			half4 tex = texCUBE (_Tex, i.texcoord);
			half4 col;
			col.rgb = tex.rgb + _Tint.rgb - 0.5;
			col.a = tex.a * _Tint.a;
			return col;
		}
		ENDCG 
	}
} 	


SubShader {
	Tags { "Queue"="Background" "RenderType"="Background" }
	Cull Off ZWrite Off Fog { Mode Off }
	Color [_Tint]
	Pass {
		SetTexture [_Tex] { combine texture +- primary, texture * primary }
	}
}

Fallback Off

}
