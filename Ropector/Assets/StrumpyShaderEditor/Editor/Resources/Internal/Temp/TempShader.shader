Shader "ShaderEditor/EditorShaderCache"
{
	Properties 
	{
_Color("_Color", Color) = (0,0,0,1)
_RimlightColor("_RimlightColor", Color) = (0.3235687,0.505704,0.5223881,0.6431373)
_Forcefield("_Forcefield", 2D) = "gray" {}
_Forcefield_Tiling("_Forcefield_Tiling", Vector) = (1,1,1,1)
_Forcegrain_Speed("_Forcegrain_Speed", Float) = 0
_Forcegrain_Brightness("_Forcegrain_Brightness", Float) = 1
_EditorTime("_EditorTime",Vector) = (0.0,0.0,0.0,0.0)

	}
	
	SubShader 
	{
		Tags
		{
"Queue"="Transparent"
"IgnoreProjector"="False"
"RenderType"="Transparent"

		}

		
Cull Back
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
float4 _RimlightColor;
sampler2D _Forcefield;
float4 _Forcefield_Tiling;
float _Forcegrain_Speed;
float _Forcegrain_Brightness;
float4 _EditorTime;

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
				float3 viewDir;
float3 worldPos;
float3 sWorldNormal;

			};

			void vert (inout appdata_full v, out Input o) {
float4 VertexOutputMaster0_0_NoInput = float4(0,0,0,0);
float4 VertexOutputMaster0_1_NoInput = float4(0,0,0,0);
float4 VertexOutputMaster0_2_NoInput = float4(0,0,0,0);
float4 VertexOutputMaster0_3_NoInput = float4(0,0,0,0);

o.sWorldNormal = mul((float3x3)_Object2World, SCALED_NORMAL);

			}
			

			void surf (Input IN, inout EditorSurfaceOutput o) {
				o.Normal = float3(0.0,0.0,1.0);
				o.Alpha = 1.0;
				o.Albedo = 0.0;
				o.Emission = 0.0;
				o.Gloss = 0.0;
				o.Specular = 0.0;
				o.Custom = 0.0;
				
float4 Fresnel0_1_NoInput = float4(0,0,1,1);
float4 Fresnel0=(1.0 - dot( normalize( float4( IN.viewDir.x, IN.viewDir.y,IN.viewDir.z,1.0 ).xyz), normalize( Fresnel0_1_NoInput.xyz ) )).xxxx;
float4 SplatAlpha0=_RimlightColor.w;
float4 Multiply3=SplatAlpha0 * float4( 4,4,4,4 );
float4 Pow0=pow(Fresnel0,Multiply3);
float4 Multiply2=Pow0 * _RimlightColor;
float4 Multiply1=float4( IN.worldPos.x, IN.worldPos.y,IN.worldPos.z,1.0 ) * _Forcefield_Tiling;
float4 Multiply5=_Forcegrain_Speed.xxxx * _EditorTime;
float4 Add3=Multiply1 + Multiply5;
float4 Tex2D0=tex2D(_Forcefield,Add3.xy);
float4 Swizzle0=float4(Multiply1.z, Multiply1.y, Multiply1.x, Multiply1.w);
float4 Add2=Swizzle0 + Multiply5;
float4 Tex2D1=tex2D(_Forcefield,Add2.xy);
float4 Splat0=float4( IN.sWorldNormal.x, IN.sWorldNormal.y,IN.sWorldNormal.z,1.0 ).x;
float4 Lerp0=lerp(Tex2D0.aaaa,Tex2D1,Splat0);
float4 Swizzle1=float4(Multiply1.x, Multiply1.z, Multiply1.y, Multiply1.w);
float4 Add1=Swizzle1 + Multiply5;
float4 Tex2D2=tex2D(_Forcefield,Add1.xy);
float4 Splat1=float4( IN.sWorldNormal.x, IN.sWorldNormal.y,IN.sWorldNormal.z,1.0 ).y;
float4 Lerp1=lerp(Lerp0,Tex2D2,Splat1);
float4 Multiply9=float4( 1.5,1.5,1.5,1.5 ) * _Forcefield_Tiling;
float4 Multiply6=float4( IN.worldPos.x, IN.worldPos.y,IN.worldPos.z,1.0 ) * Multiply9;
float4 Multiply8=_Forcegrain_Speed.xxxx * float4( 1.5,1.5,1.5,1.5 );
float4 Multiply7=Multiply8 * _EditorTime;
float4 Add6=Multiply6 + Multiply7;
float4 Tex2D3=tex2D(_Forcefield,Add6.xy);
float4 Swizzle2=float4(Multiply6.z, Multiply6.y, Multiply6.x, Multiply6.w);
float4 Add5=Swizzle2 + Multiply7;
float4 Tex2D5=tex2D(_Forcefield,Add5.xy);
float4 Splat2=float4( IN.sWorldNormal.x, IN.sWorldNormal.y,IN.sWorldNormal.z,1.0 ).x;
float4 Lerp3=lerp(Tex2D3.aaaa,Tex2D5,Splat2);
float4 Swizzle3=float4(Multiply6.x, Multiply6.z, Multiply6.y, Multiply6.w);
float4 Add4=Swizzle3 + Multiply7;
float4 Tex2D4=tex2D(_Forcefield,Add4.xy);
float4 Splat3=float4( IN.sWorldNormal.x, IN.sWorldNormal.y,IN.sWorldNormal.z,1.0 ).y;
float4 Lerp4=lerp(Lerp3,Tex2D4,Splat3);
float4 Add7=Lerp1 + Lerp4;
float4 Multiply10=_Forcegrain_Brightness.xxxx * Add7;
float4 Multiply11=Multiply10 * Add7;
float4 Add0=Multiply2 + Multiply11;
float4 Multiply4=Add0 * Multiply2;
float4 SplatAlpha1=_Color.w;
float4 Multiply0=SplatAlpha1 * Add7;
float4 Master0_1_NoInput = float4(0,0,1,1);
float4 Master0_3_NoInput = float4(0,0,0,0);
float4 Master0_4_NoInput = float4(0,0,0,0);
float4 Master0_7_NoInput = float4(0,0,0,0);
float4 Master0_6_NoInput = float4(1,1,1,1);
o.Albedo = _Color;
o.Emission = Multiply4;
o.Alpha = Multiply0;

				o.Normal = normalize(o.Normal);
			}
		ENDCG
	}
	Fallback "Diffuse"
}