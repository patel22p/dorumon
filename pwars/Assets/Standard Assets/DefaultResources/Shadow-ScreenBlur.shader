Shader "Hidden/Shadow-ScreenBlur" {
Properties {
	_MainTex ("Base", 2D) = "white" {}
}
SubShader {
	Pass {
		ZTest Always Cull Off ZWrite Off
		Fog { Mode off }
		
CGPROGRAM
#pragma vertex vert_img
#pragma fragment frag
#pragma fragmentoption ARB_precision_hint_fastest
#include "UnityCG.cginc"

uniform sampler2D _MainTex;

#define BLUR_SAMPLE_COUNT 8

// x,y of each - sample offset for blur
uniform float4 _BlurOffsets[BLUR_SAMPLE_COUNT];

float4 unity_ShadowBlurParams;

half4 frag (v2f_img i) : COLOR
{
	float4 coord = float4(i.uv,0,0);
	half4 mask = tex2D( _MainTex, coord.xy );
	half dist = mask.b + mask.a / 255.0;
	half radius = saturate(unity_ShadowBlurParams.y / (1.0-dist));
	
	half diffTolerance = unity_ShadowBlurParams.x;
	
	mask.xy *= diffTolerance;
	for (int i = 0; i < BLUR_SAMPLE_COUNT; i++)
	{
		half4 sample = tex2D( _MainTex, (coord + radius * _BlurOffsets[i]).xy );
		half sampleDist = sample.b + sample.a / 255.0;
		half diff = dist - sampleDist;
		diff = saturate( diffTolerance - abs(diff) );
		mask.xy += diff * sample.xy;
	}
	half shadow = mask.x / mask.y;
	return shadow;
}
ENDCG
	}	
}

Fallback Off
}
