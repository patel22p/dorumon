Shader "Ropector/Tron"
{
	Properties 
	{
_Color("_Color", Color) = (0,0,0,1)
_SecondaryColor("_SecondaryColor", Color) = (1,1,1,1)
_Reflection("_Reflection", Cube) = "black" {}
_ReflectionColor("_ReflectionColor", Color) = (0.2985075,0.2985075,0.2985075,1)
_RimlightColor("_RimlightColor", Color) = (0.3235687,0.505704,0.5223881,0.6431373)
_VC_RedColor("_VC_RedColor", Color) = (1,0.0970149,0.0970149,1)
_VC_GreenColor("_VC_GreenColor", Color) = (0.09803922,0.9960784,0.2047992,1)
_VC_BlueColor("_VC_BlueColor", Color) = (0.09803923,0.3548059,0.9921569,1)

	}
	
	SubShader 
	{
		Tags
		{
"Queue"="Geometry"
"IgnoreProjector"="False"
"RenderType"="Opaque"

		}


Cull Back
ZWrite On
ZTest LEqual
ColorMask RGBA
Fog{
}


		CGPROGRAM
#pragma surface surf BlinnPhongEditor  noambient nolightmap noforwardadd approxview halfasview vertex:vert
#pragma target 2.0


float4 _Color;
float4 _SecondaryColor;
samplerCUBE _Reflection;
float4 _ReflectionColor;
float4 _RimlightColor;
float4 _VC_RedColor;
float4 _VC_GreenColor;
float4 _VC_BlueColor;

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
				float4 color : COLOR;
float3 viewDir;
float3 simpleWorldRefl;

			};

			void vert (inout appdata_full v, out Input o) {
float4 VertexOutputMaster0_0_NoInput = float4(0,0,0,0);
float4 VertexOutputMaster0_1_NoInput = float4(0,0,0,0);
float4 VertexOutputMaster0_2_NoInput = float4(0,0,0,0);
float4 VertexOutputMaster0_3_NoInput = float4(0,0,0,0);

o.simpleWorldRefl = -reflect( normalize(WorldSpaceViewDir(v.vertex)), normalize(mul((float3x3)_Object2World, SCALED_NORMAL)));

			}
			

			void surf (Input IN, inout EditorSurfaceOutput o) {
				o.Normal = float3(0.0,0.0,1.0);
				o.Alpha = 1.0;
				o.Albedo = 0.0;
				o.Emission = 0.0;
				o.Gloss = 0.0;
				o.Specular = 0.0;
				o.Custom = 0.0;
				
float4 SplatAlpha1=IN.color.w;
float4 Lerp3=lerp(_Color,_SecondaryColor,SplatAlpha1);
float4 Fresnel0_1_NoInput = float4(0,0,1,1);
float4 Fresnel0=(1.0 - dot( normalize( float4( IN.viewDir.x, IN.viewDir.y,IN.viewDir.z,1.0 ).xyz), normalize( Fresnel0_1_NoInput.xyz ) )).xxxx;
float4 SplatAlpha0=_RimlightColor.w;
float4 Multiply3=SplatAlpha0 * float4( 4,4,4,4 );
float4 Pow0=pow(Fresnel0,Multiply3);
float4 Multiply2=Pow0 * _RimlightColor;
float4 TexCUBE0=texCUBE(_Reflection,float4( IN.simpleWorldRefl.x, IN.simpleWorldRefl.y,IN.simpleWorldRefl.z,1.0 ));
float4 Add0=TexCUBE0 + TexCUBE0;
float4 Multiply1=Add0 * Add0;
float4 Multiply0=Multiply1 * _ReflectionColor;
float4 Add1=Multiply2 + Multiply0;
float4 Splat0=IN.color.x;
float4 Lerp0=lerp(Add1,_VC_RedColor,Splat0);
float4 Splat1=IN.color.y;
float4 Lerp1=lerp(Lerp0,_VC_GreenColor,Splat1);
float4 Splat2=IN.color.z;
float4 Lerp2=lerp(Lerp1,_VC_BlueColor,Splat2);
float4 Master0_1_NoInput = float4(0,0,1,1);
float4 Master0_3_NoInput = float4(0,0,0,0);
float4 Master0_4_NoInput = float4(0,0,0,0);
float4 Master0_5_NoInput = float4(1,1,1,1);
float4 Master0_7_NoInput = float4(0,0,0,0);
float4 Master0_6_NoInput = float4(1,1,1,1);
o.Albedo = Lerp3;
o.Emission = Lerp2;

				o.Normal = normalize(o.Normal);
			}
		ENDCG
	}
	Fallback "Ropector/Tron_Fallback"
}