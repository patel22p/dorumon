Shader "Bunny/Opaque"
{
	Properties 
	{
_Color("_Color", Color) = (1,1,1,1)
_Color_BLACK("_Color_BLACK", Color) = (1,1,1,1)
_BLACK("_BLACK", 2D) = "white" {}
_RED("_RED", 2D) = "white" {}
_GREEN("_GREEN", 2D) = "white" {}
_BLUE("_BLUE", 2D) = "white" {}
_ALPHA("_ALPHA", 2D) = "white" {}
_Cavity("_Cavity", 2D) = "white" {}
_RimlightColor("_RimlightColor", Color) = (0,0,0,1)
_RimlightSpread("_RimlightSpread", Float) = 2

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
#pragma surface surf BlinnPhongEditor  noforwardadd approxview halfasview vertex:vert
#pragma target 2.0


float4 _Color;
float4 _Color_BLACK;
sampler2D _BLACK;
sampler2D _RED;
sampler2D _GREEN;
sampler2D _BLUE;
sampler2D _ALPHA;
sampler2D _Cavity;
float4 _RimlightColor;
float _RimlightSpread;

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
				float2 uv_BLACK;
float2 uv_RED;
float4 color : COLOR;
float2 uv_GREEN;
float2 uv_BLUE;
float2 uv_ALPHA;
float2 uv_Cavity;
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
				
float4 SplatAlpha1=_Color.w;
float4 Multiply13=SplatAlpha1 * float4( 10,10,10,10 );
float4 Tex2D0=tex2D(_BLACK,(IN.uv_BLACK.xyxy).xy);
float4 Tex2D1=tex2D(_RED,(IN.uv_RED.xyxy).xy);
float4 Splat0=IN.color.x;
float4 Lerp0=lerp(Tex2D0,Tex2D1,Splat0);
float4 Tex2D2=tex2D(_GREEN,(IN.uv_GREEN.xyxy).xy);
float4 Splat1=IN.color.y;
float4 Lerp2=lerp(Lerp0,Tex2D2,Splat1);
float4 Tex2D5=tex2D(_BLUE,(IN.uv_BLUE.xyxy).xy);
float4 Splat3=IN.color.z;
float4 Lerp1=lerp(Lerp2,Tex2D5,Splat3);
float4 Tex2D3=tex2D(_ALPHA,(IN.uv_ALPHA.xyxy).xy);
float4 Splat2=IN.color.w;
float4 Multiply2=Tex2D3.aaaa * Splat2;
float4 Lerp4=lerp(Lerp1,Tex2D3,Multiply2);
float4 Multiply9=Multiply13 * Lerp4;
float4 Tex2D4=tex2D(_Cavity,(IN.uv_Cavity.xyxy).xy);
float4 Multiply0=Multiply9 * Tex2D4.aaaa;
float4 Multiply4=_Color * Multiply0;
float4 Fresnel0_1_NoInput = float4(0,0,1,1);
float4 Fresnel0=(1.0 - dot( normalize( float4( IN.viewDir.x, IN.viewDir.y,IN.viewDir.z,1.0 ).xyz), normalize( Fresnel0_1_NoInput.xyz ) )).xxxx;
float4 Pow0=pow(Fresnel0,_RimlightSpread.xxxx);
float4 Multiply3=Pow0 * _RimlightColor;
float4 SplatAlpha0=_RimlightColor.w;
float4 Multiply11=SplatAlpha0 * float4( 10,10,10,10 );
float4 Multiply12=Multiply3 * Multiply11;
float4 Add0=Multiply4 + Multiply12;
float4 Master0_1_NoInput = float4(0,0,1,1);
float4 Master0_2_NoInput = float4(0,0,0,0);
float4 Master0_3_NoInput = float4(0,0,0,0);
float4 Master0_4_NoInput = float4(0,0,0,0);
float4 Master0_5_NoInput = float4(1,1,1,1);
float4 Master0_7_NoInput = float4(0,0,0,0);
float4 Master0_6_NoInput = float4(1,1,1,1);
o.Albedo = Add0;

				o.Normal = normalize(o.Normal);
			}
		ENDCG
	}
	Fallback "Diffuse"
}