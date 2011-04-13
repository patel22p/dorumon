Shader "Bunny/Character_SSS"
{
	Properties 
	{
_Cavity("_Cavity", 2D) = "white" {}
_Color("_Color", Color) = (1,1,1,1)
_Detail("_Detail", 2D) = "gray" {}
_DetailStrength("_DetailStrength", Range(0,1) ) = 1
_Selfillumination("_Selfillumination", Range(0,1) ) = 0.5
_RimlightColor("_RimlightColor", Color) = (0.2611941,0.2611941,0.2611941,1)
_RimlightStrength("_RimlightStrength", Range(0,3) ) = 1.903941

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
#pragma surface surf BlinnPhongEditor  vertex:vert
#pragma target 2.0


sampler2D _Cavity;
float4 _Color;
sampler2D _Detail;
float _DetailStrength;
float _Selfillumination;
float4 _RimlightColor;
float _RimlightStrength;
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
float4 Multiply0=float4( s.Albedo.x, s.Albedo.y, s.Albedo.z, 1.0 ) * float4( 1.1,1,1,0);
float4 Lerp0=lerp(float4( s.Albedo.x, s.Albedo.y, s.Albedo.z, 1.0 ),Multiply0,s.Custom);
return Lerp0;

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
				float2 uv_Cavity;
float4 color : COLOR;
float2 uv_Detail;
float3 viewDir;
float4 screenPos;

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
				
float4 Tex2D0=tex2D(_Cavity,(IN.uv_Cavity.xyxy).xy);
float4 Multiply1=Tex2D0.aaaa * IN.color;
float4 Multiply2=_Color * Multiply1;
float4 Tex2D1=tex2D(_Detail,(IN.uv_Detail.xyxy).xy);
float4 Multiply3=Multiply2 * Tex2D1.aaaa;
float4 Lerp0=lerp(Multiply2,Multiply3,_DetailStrength.xxxx);
float4 Fresnel0_1_NoInput = float4(0,0,1,1);
float4 Fresnel0=(1.0 - dot( normalize( float4( IN.viewDir.x, IN.viewDir.y,IN.viewDir.z,1.0 ).xyz), normalize( Fresnel0_1_NoInput.xyz ) )).xxxx;
float4 Pow0=pow(Fresnel0,_RimlightStrength.xxxx);
float4 Multiply0=Pow0 * _RimlightColor;
float4 Add0=Lerp0 + Multiply0;
float4 Lerp1=lerp(Multiply0,Add0,_Selfillumination.xxxx);
float4 ScreenDepthDiff0= LinearEyeDepth (tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(IN.screenPos)).r) - IN.screenPos.z;
float4 ScreenDepth0= LinearEyeDepth (tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD( IN.screenPos)).r);
float4 Dot0=dot( ScreenDepthDiff0.xyz, ScreenDepth0.xyz ).xxxx;
float4 Dot2=dot( Dot0.xyz, ScreenDepth0.xyz ).xxxx;
float4 Dot1=dot( Dot2.xyz, ScreenDepth0.xyz ).xxxx;
float4 Master0_1_NoInput = float4(0,0,1,1);
float4 Master0_3_NoInput = float4(0,0,0,0);
float4 Master0_4_NoInput = float4(0,0,0,0);
float4 Master0_5_NoInput = float4(1,1,1,1);
float4 Master0_6_NoInput = float4(1,1,1,1);
o.Albedo = Lerp0;
o.Emission = Lerp1;
o.Custom = Dot1;

				o.Normal = normalize(o.Normal);
			}
		ENDCG
	}
	Fallback "Diffuse"
}