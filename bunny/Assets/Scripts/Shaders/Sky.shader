Shader "Bunny/Sky"
{
	Properties 
	{
_SkyColor("_SkyColor", Color) = (0.5895523,0.8105626,1,1)
_CloudsCube("_CloudsCube", Cube) = "black" {}
_Clouds("Clouds", Color) = (1,1,1,1)
_WindSpeed("WindSpeed", Float) = 0

	}
	
	SubShader 
	{
		Tags
		{
"Queue"="Geometry"
"IgnoreProjector"="False"
"RenderType"="Opaque"

		}

		
Cull Off
ZWrite Off
ZTest LEqual
ColorMask RGBA
Fog{
Mode Linear
Range 0,2000
}


		CGPROGRAM
#pragma surface surf BlinnPhongEditor  vertex:vert
#pragma target 2.0


float4 _SkyColor;
samplerCUBE _CloudsCube;
float4 _Clouds;
float _WindSpeed;

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
				float4 meshUV;

			};

			void vert (inout appdata_full v, out Input o) {
float4 VertexOutputMaster0_0_NoInput = float4(0,0,0,0);
float4 VertexOutputMaster0_1_NoInput = float4(0,0,0,0);
float4 VertexOutputMaster0_2_NoInput = float4(0,0,0,0);
float4 VertexOutputMaster0_3_NoInput = float4(0,0,0,0);

o.meshUV.xy = v.texcoord.xy;
o.meshUV.zw = v.texcoord1.xy;

			}
			

			void surf (Input IN, inout EditorSurfaceOutput o) {
				o.Normal = float3(0.0,0.0,1.0);
				o.Alpha = 1.0;
				o.Albedo = 0.0;
				o.Emission = 0.0;
				o.Gloss = 0.0;
				o.Specular = 0.0;
				o.Custom = 0.0;
				
float4 SplatAlpha0=_SkyColor.w;
float4 Multiply0=SplatAlpha0 * float4( 10,10,10,10 );
float4 Multiply5=_WindSpeed.xxxx * _Time;
float4 Add1=(IN.meshUV.xyxy) + Multiply5;
float4 TexCUBE0=texCUBE(_CloudsCube,Add1);
float4 Multiply2=TexCUBE0.aaaa * _Clouds;
float4 SplatAlpha1=_Clouds.w;
float4 Multiply3=SplatAlpha1 * float4( 10,10,10,10 );
float4 Multiply4=Multiply2 * Multiply3;
float4 Add0=_SkyColor + Multiply4;
float4 Multiply1=Multiply0 * Add0;
float4 Master0_0_NoInput = float4(0,0,0,0);
float4 Master0_1_NoInput = float4(0,0,1,1);
float4 Master0_3_NoInput = float4(0,0,0,0);
float4 Master0_4_NoInput = float4(0,0,0,0);
float4 Master0_5_NoInput = float4(1,1,1,1);
float4 Master0_7_NoInput = float4(0,0,0,0);
float4 Master0_6_NoInput = float4(1,1,1,1);
o.Emission = Multiply1;

				o.Normal = normalize(o.Normal);
			}
		ENDCG
	}
	Fallback "Diffuse"
}