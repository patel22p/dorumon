Shader "Bunny/Transparent_OneSided"
{
	Properties 
	{
_RimlightSpread("_RimlightSpread", Range(0,3) ) = 1.095238
_Color("_Color", Color) = (1,1,1,0.4313726)
_AlphaCutoff("_AlphaCutoff", Range(0,1) ) = 0
_Rimlight_Strength("_Rimlight_Strength", Float) = 1
_MainTex("_MainTex", 2D) = "gray" {}
_Rimlight_Color("_Rimlight_Color", Color) = (1,0.03529412,1,0.9411765)

	}
	
	SubShader 
	{
		Tags
		{
"Queue"="Transparent+10"
"IgnoreProjector"="False"
"RenderType"="Transparent"

		}

		
Cull Back
ZWrite On
ZTest LEqual
ColorMask RGBA
Blend One One
Fog{
}


		CGPROGRAM
#pragma surface surf BlinnPhongEditor  vertex:vert
#pragma target 2.0


float _RimlightSpread;
float4 _Color;
float _AlphaCutoff;
float _Rimlight_Strength;
sampler2D _MainTex;
float4 _Rimlight_Color;

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
return float4( s.Albedo.x, s.Albedo.y, s.Albedo.z, 1.0 );

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
				float2 uv_MainTex;
float3 viewDir;

			};

			void vert (inout appdata_full v, out Input o) {
float4 VertexOutputMaster0_0_NoInput = float4(0,0,0,0);
float4 VertexOutputMaster0_1_NoInput = float4(0,0,0,0);
float4 VertexOutputMaster0_2_NoInput = float4(0,0,0,0);
float4 VertexOutputMaster0_3_NoInput = float4(0,0,0,0);


			}
			

			void surf (Input IN, inout EditorSurfaceOutput o) {
				o.Normal = float3(0.0,0.0,1.0);
				o.Alpha = 1.0;
				o.Albedo = 0.0;
				o.Emission = 0.0;
				o.Gloss = 0.0;
				o.Specular = 0.0;
				o.Custom = 0.0;
				
float4 Tex2D0=tex2D(_MainTex,(IN.uv_MainTex.xyxy).xy);
float4 Multiply0=_Color * Tex2D0;
float4 SplatAlpha1=_Rimlight_Color.w;
float4 Multiply4=float4( 10,10,10,10 ) * SplatAlpha1;
float4 Fresnel0_1_NoInput = float4(0,0,1,1);
float4 Fresnel0=(1.0 - dot( normalize( float4( IN.viewDir.x, IN.viewDir.y,IN.viewDir.z,1.0 ).xyz), normalize( Fresnel0_1_NoInput.xyz ) )).xxxx;
float4 Pow0=pow(Fresnel0,_RimlightSpread.xxxx);
float4 Multiply3=Pow0 * _Rimlight_Color;
float4 Multiply2=Multiply4 * Multiply3;
float4 SplatAlpha0=_Color.w;
float4 Multiply1=SplatAlpha0 * Tex2D0.aaaa;
float4 Add0=Multiply2 + Multiply1;
float4 Saturate0=saturate(Add0);
float4 Subtract0=Tex2D0.aaaa - _AlphaCutoff.xxxx;
float4 Master0_1_NoInput = float4(0,0,1,1);
float4 Master0_3_NoInput = float4(0,0,0,0);
float4 Master0_4_NoInput = float4(0,0,0,0);
float4 Master0_7_NoInput = float4(0,0,0,0);
clip( Subtract0 );
o.Albedo = Multiply0;
o.Emission = Multiply2;
o.Alpha = Saturate0;

				o.Normal = normalize(o.Normal);
			}
		ENDCG
	}
	Fallback "Diffuse"
}