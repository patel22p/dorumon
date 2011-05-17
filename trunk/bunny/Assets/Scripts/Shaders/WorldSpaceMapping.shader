Shader "WordlSpaceMapping"
{
	Properties 
	{
_TexX("_TexX", 2D) = "black" {}
_TexX_Tiling("_TexX_Tiling", Float) = 0.1
_TexY("_TexY", 2D) = "white" {}
_TexY_Tiling("_TexY_Tiling", Float) = 0.1
_TexZ("_TexZ", 2D) = "gray" {}
_TexZ_Tiling("_TexZ_Tiling", Float) = 0.1

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


sampler2D _TexX;
float _TexX_Tiling;
sampler2D _TexY;
float _TexY_Tiling;
sampler2D _TexZ;
float _TexZ_Tiling;

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
				
float4 Multiply0=float4( 1,1,1,0) * float4( IN.worldPos.x, IN.worldPos.y,IN.worldPos.z,1.0 );
float4 Multiply4=_TexX_Tiling.xxxx * Multiply0;
float4 Tex2D0=tex2D(_TexX,Multiply4.xy);
float4 Swizzle0=float4(float4( IN.worldPos.x, IN.worldPos.y,IN.worldPos.z,1.0 ).z, float4( IN.worldPos.x, IN.worldPos.y,IN.worldPos.z,1.0 ).y, float4( IN.worldPos.x, IN.worldPos.y,IN.worldPos.z,1.0 ).x, float4( IN.worldPos.x, IN.worldPos.y,IN.worldPos.z,1.0 ).w);
float4 Multiply1=Swizzle0 * float4( 1,1,1,0);
float4 Multiply5=_TexY_Tiling.xxxx * Multiply1;
float4 Tex2D1=tex2D(_TexY,Multiply5.xy);
float4 Splat0=float4( IN.sWorldNormal.x, IN.sWorldNormal.y,IN.sWorldNormal.z,1.0 ).x;
float4 Negative0= -Splat0; 
 float4 Saturate0=saturate(Negative0);
float4 Saturate1=saturate(Splat0);
float4 Add0=Saturate0 + Saturate1;
float4 Lerp0=lerp(Tex2D0,Tex2D1,Add0);
float4 Swizzle1=float4(float4( IN.worldPos.x, IN.worldPos.y,IN.worldPos.z,1.0 ).x, float4( IN.worldPos.x, IN.worldPos.y,IN.worldPos.z,1.0 ).z, float4( IN.worldPos.x, IN.worldPos.y,IN.worldPos.z,1.0 ).y, float4( IN.worldPos.x, IN.worldPos.y,IN.worldPos.z,1.0 ).w);
float4 Multiply2=Swizzle1 * float4( 1,1,1,0);
float4 Multiply6=_TexZ_Tiling.xxxx * Multiply2;
float4 Tex2D2=tex2D(_TexZ,Multiply6.xy);
float4 Splat1=float4( IN.sWorldNormal.x, IN.sWorldNormal.y,IN.sWorldNormal.z,1.0 ).y;
float4 Negative1= -Splat1; 
 float4 Saturate3=saturate(Negative1);
float4 Saturate2=saturate(Splat1);
float4 Add1=Saturate3 + Saturate2;
float4 Lerp1=lerp(Lerp0,Tex2D2,Add1);
float4 Multiply3=Lerp1 * Lerp1;
float4 Master0_1_NoInput = float4(0,0,1,1);
float4 Master0_2_NoInput = float4(0,0,0,0);
float4 Master0_3_NoInput = float4(0,0,0,0);
float4 Master0_4_NoInput = float4(0,0,0,0);
float4 Master0_5_NoInput = float4(1,1,1,1);
float4 Master0_7_NoInput = float4(0,0,0,0);
float4 Master0_6_NoInput = float4(1,1,1,1);
o.Albedo = Multiply3;

				o.Normal = normalize(o.Normal);
			}
		ENDCG
	}
	Fallback "Diffuse"
}