Shader "Bunny/Xtra/Foliage_Wind"
{
	Properties 
	{
_Color("_Color", Color) = (1,1,1,1)
_AlphaCutoff("_AlphaCutoff", Float) = 0.5
_Diffuse("_Diffuse", 2D) = "black" {}
_Frequency("_Frequency", Float) = 1
_Scale("_Scale", Float) = 0.5
_Fade("_Fade", Float) = 0.5
_Distance("_Distance", Float) = 0

	}
	
	SubShader 
	{
		Tags
		{
"Queue"="Transparent+100"
"IgnoreProjector"="False"
"RenderType"="Opaque"

		}

		
Cull Off
ZWrite On
ZTest LEqual
ColorMask RGBA
Blend SrcAlpha OneMinusSrcAlpha
Fog{
}


		CGPROGRAM
#pragma surface surf BlinnPhongEditor  vertex:vert
#pragma target 2.0


float4 _Color;
float _AlphaCutoff;
sampler2D _Diffuse;
float _Frequency;
float _Scale;
float _Fade;
float _Distance;
sampler2D _CameraDepthTexture;

			struct EditorSurfaceOutput {
				half3 Albedo;
				half3 Normal;
				half3 Emission;
				half3 Gloss;
				half Specular;
				half Alpha;
				half4 Custom;
			};
			
			inline half4 LightingBlinnPhongEditor_PrePass (EditorSurfaceOutput s, half4 light)
			{
half3 spec = light.a * s.Gloss;
half4 c;
c.rgb = (s.Albedo * light.rgb + light.rgb * spec);
c.a = s.Alpha;
return c;

			}

			inline half4 LightingBlinnPhongEditor (EditorSurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
			{
				half3 h = normalize (lightDir + viewDir);
				
				half diff = max (0, dot ( lightDir, s.Normal ));
				
				float nh = max (0, dot (s.Normal, h));
				float spec = pow (nh, s.Specular*128.0);
				
				half4 res;
				res.rgb = _LightColor0.rgb * diff;
				res.w = spec * Luminance (_LightColor0.rgb);
				res *= atten * 2.0;

				return LightingBlinnPhongEditor_PrePass( s, res );
			}
			
			struct Input {
				float2 uv_Diffuse;
float4 screenPos;

			};

			void vert (inout appdata_full v, out Input o) {
float4 Splat1=v.color.y;
float4 Add0=v.vertex + _Time;
float4 Splat0=Add0.y;
float4 Multiply0=Splat0 * _Frequency.xxxx;
float4 Sin0=sin(Multiply0);
float4 Mask0=float4(Sin0.x,0.0,Sin0.z,0.0);
float4 Multiply1=_Scale.xxxx * Mask0;
float4 Multiply2=Splat1 * Multiply1;
float4 Add1=Multiply2 + v.vertex;
float4 VertexOutputMaster0_1_NoInput = float4(0,0,0,0);
float4 VertexOutputMaster0_2_NoInput = float4(0,0,0,0);
float4 VertexOutputMaster0_3_NoInput = float4(0,0,0,0);
v.vertex = Add1;


			}
			

			void surf (Input IN, inout EditorSurfaceOutput o) {
				o.Normal = float3(0.0,0.0,1.0);
				o.Alpha = 1.0;
				o.Albedo = 0.0;
				o.Emission = 0.0;
				o.Gloss = 0.0;
				o.Specular = 0.0;
				o.Custom = 0.0;
				
float4 Multiply2=_Color * float4( 3,3,3,3 );
float4 Tex2D0=tex2D(_Diffuse,(IN.uv_Diffuse.xyxy).xy);
float4 Multiply1=Multiply2 * Tex2D0;
float4 Add1=Multiply1 + Tex2D0;
float4 ScreenDepthDiff0= LinearEyeDepth (tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(IN.screenPos)).r) - IN.screenPos.z;
float4 Pow0=pow(_Fade.xxxx,ScreenDepthDiff0);
float4 Saturate0=saturate(Pow0);
float4 Invert0= float4(1.0, 1.0, 1.0, 1.0) - Saturate0;
float4 ScreenDepth0= LinearEyeDepth (tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD( IN.screenPos)).r);
float4 Pow1=pow(_Distance.xxxx,ScreenDepth0);
float4 Multiply0=Invert0 * Pow1;
float4 Subtract0=Tex2D0.aaaa - _AlphaCutoff.xxxx;
float4 Master0_1_NoInput = float4(0,0,1,1);
float4 Master0_2_NoInput = float4(0,0,0,0);
float4 Master0_3_NoInput = float4(0,0,0,0);
float4 Master0_4_NoInput = float4(0,0,0,0);
float4 Master0_7_NoInput = float4(0,0,0,0);
clip( Subtract0 );
o.Albedo = Add1;
o.Alpha = Multiply0;

				o.Normal = normalize(o.Normal);
			}
		ENDCG
	}
	Fallback ""
}