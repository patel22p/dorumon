Shader "Bunny/Transparent_TwoSided"
{
	Properties 
	{
_Color("_Color", Color) = (1,1,1,1)
_MainTex("_MainTex", 2D) = "white" {}
_RimlightSpread("_RimlightSpread", Range(0,3) ) = 0.5
_Rimlight_Color("_Rimlight_Color", Color) = (1,1,1,1)
_Rimlight_Strength("_Rimlight_Strength", Float) = 1
_AlphaCutoff("_AlphaCutoff", Range(0,1) ) = 0.5

	}
	
	SubShader 
	{
		Tags
		{
"Queue"="Transparent+1"
"IgnoreProjector"="False"
"RenderType"="Opaque"

		}

		
Cull Off
ZWrite Off
ZTest LEqual
ColorMask RGBA
Blend SrcAlpha OneMinusSrcAlpha
Fog{
}


		CGPROGRAM
#pragma surface surf BlinnPhongEditor  exclude_path:prepass noambient nolightmap noforwardadd approxview halfasview vertex:vert
#pragma target 2.0


float4 _Color;
sampler2D _MainTex;
float _RimlightSpread;
float4 _Rimlight_Color;
float _Rimlight_Strength;
float _AlphaCutoff;

			struct EditorSurfaceOutput {
				half3 Albedo;
				half3 Normal;
				half3 Emission;
				half3 Gloss;
				half Specular;
				half Alpha;
			};
			
			inline half4 LightingBlinnPhongEditor_PrePass (EditorSurfaceOutput s, half4 light)
			{
float4 Multiply0=float4(s.Albedo, 1.0) * float4(s.Alpha);
return Multiply0;

			}

			inline half4 LightingBlinnPhongEditor (EditorSurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
			{
				half3 h = normalize (lightDir + viewDir);
				
				half diff = max (0, dot (s.Normal, lightDir));
				
				float nh = max (0, dot (s.Normal, h));
				float spec = pow (nh, s.Specular*128.0);
				
				half4 res;
				res.rgb = _LightColor0.rgb * diff * atten * 2.0;
				res.w = spec * Luminance (_LightColor0.rgb);

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
				o.Gloss=0.0;
				o.Specular=0.0;
				
float4 Mask0=float4(_Color.x,_Color.y,_Color.z,0.0);
float4 Tex2D0=tex2D(_MainTex,(IN.uv_MainTex.xyxy).xy);
float4 Multiply0=Mask0 * Tex2D0;
float4 Fresnel0_1_NoInput = float4(0,0,1,1);
float4 Fresnel0=float4( 1.0 - dot( normalize( float4(IN.viewDir, 1.0).xyz), normalize( Fresnel0_1_NoInput.xyz ) ) );
float4 Pow0=pow(Fresnel0,_RimlightSpread.xxxx);
float4 Multiply3=_Rimlight_Color * float4(_Rimlight_Strength);
float4 Multiply2=Pow0 * Multiply3;
float4 Splat0=_Color.w;
float4 Multiply1=Splat0 * float4( Tex2D0.a);
float4 Subtract0=float4( Tex2D0.a) - _AlphaCutoff.xxxx;
float4 Master0_1_NoInput = float4(0,0,1,1);
float4 Master0_3_NoInput = float4(0,0,0,0);
float4 Master0_4_NoInput = float4(0,0,0,0);
clip( Subtract0 );
o.Albedo = Multiply0;
o.Emission = Multiply2;
o.Alpha = Multiply1;

				o.Normal = normalize(o.Normal);
			}
		ENDCG
	}
	Fallback "Diffuse"
}