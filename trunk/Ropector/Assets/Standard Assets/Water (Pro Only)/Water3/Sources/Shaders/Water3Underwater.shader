Shader "Hidden/Water3Underwater" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "" {}
	}

	CGINCLUDE
	
	#include "UnityCG.cginc"
	
	struct v2f {
		float4 pos : POSITION;
		float2 uv : TEXCOORD0;

		float4 uv01 : TEXCOORD1;
		float4 uv23 : TEXCOORD2;
		float4 uv45 : TEXCOORD3;
	};
	
	float4 offsets;
	float _UnderwaterColorFade;
	
	sampler2D _MainTex;
	
	uniform float4 _DepthColor;
	uniform float4 _MainTex_TexelSize;
	
	sampler2D _CameraDepthTexture;
		
	v2f vert (appdata_img v) {
		v2f o;
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);

		o.uv.xy = v.texcoord.xy;

		o.uv01 =  v.texcoord.xyxy + offsets.xyxy * float4(1,1, -1,-1) * _MainTex_TexelSize.xyxy;
		o.uv23 =  v.texcoord.xyxy + offsets.xyxy * float4(1,1, -1,-1) * 2.0 * _MainTex_TexelSize.xyxy;
		o.uv45 =  v.texcoord.xyxy + offsets.xyxy * float4(1,1, -1,-1) * 3.0 * _MainTex_TexelSize.xyxy;

		return o;  
	}
		
	half4 frag (v2f i) : COLOR {
		half4 color = float4 (0,0,0,0);

		color +=  tex2D (_MainTex, i.uv) * 2;
		color +=  tex2D (_MainTex, i.uv01.xy);
		color +=  tex2D (_MainTex, i.uv01.zw);
		color +=  tex2D (_MainTex, i.uv23.xy);
		color +=  tex2D (_MainTex, i.uv23.zw);
		color +=  tex2D (_MainTex, i.uv45.xy);
		color +=  tex2D (_MainTex, i.uv45.zw);	
		color /= 8.0;
		
		//float dep = tex2D(_CameraDepthTexture, i.uv).r;
		//dep = Linear01Depth(dep);

		color = lerp(color, _DepthColor, _UnderwaterColorFade);
		color.a = 1.0;
		return color;
	} 

	ENDCG
	
Subshader {
 Pass {
	  ZTest Always Cull Off ZWrite Off
	  Fog { Mode off }      

      CGPROGRAM
      #pragma fragmentoption ARB_precision_hint_fastest
      #pragma vertex vert
      #pragma fragment frag
      ENDCG
  }
}

Fallback off

	
} // shader