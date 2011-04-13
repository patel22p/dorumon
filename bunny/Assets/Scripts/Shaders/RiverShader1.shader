Shader "Bunny/River"
{
	Properties 
	{
_Water("_Water", 2D) = "white" {}
_Foam("_Foam", 2D) = "white" {}
_Foliage("_Foliage", 2D) = "white" {}
_RimlightSpread("_RimlightSpread", Range(0,3) ) = 2
_RimlightColor("_RimlightColor", Color) = (1,1,1,1)
_WaterSpeed("_WaterSpeed", Range(-10,10) ) = 1
_Color("_Color", Color) = (1,1,1,1)
_Lighting("_Lighting", Color) = (1,1,1,1)

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


sampler2D _Water;
sampler2D _Foam;
sampler2D _Foliage;
float _RimlightSpread;
float4 _RimlightColor;
float _WaterSpeed;
float4 _Color;
float4 _Lighting;

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
float4 Multiply1=float4( s.Albedo.x, s.Albedo.y, s.Albedo.z, 1.0 ) * s.Alpha.xxxx;
float4 Multiply0=Multiply1 * _Lighting;
return Multiply0;

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
				float2 uv_Water;
float4 color : COLOR;
float2 uv_Foliage;
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
				
float4 Multiply3=_Time * _WaterSpeed.xxxx;
float4 UV_Pan0=float4((IN.uv_Water.xyxy).x + Multiply3.x,(IN.uv_Water.xyxy).y,(IN.uv_Water.xyxy).z,(IN.uv_Water.xyxy).w);
float4 Tex2D1=tex2D(_Water,UV_Pan0.xy);
float4 Multiply1=(IN.uv_Water.xyxy) * float4( 2,2,2,2 );
float4 UV_Pan1=float4(Multiply1.x + Multiply3.x,Multiply1.y,Multiply1.z,Multiply1.w);
float4 Tex2D2=tex2D(_Foam,UV_Pan1.xy);
float4 Splat1=IN.color.y;
float4 Multiply11=Tex2D2.aaaa * Splat1;
float4 Lerp2=lerp(Tex2D1,Tex2D2.aaaa,Multiply11);
float4 Tex2D3=tex2D(_Foliage,(IN.uv_Foliage.xyxy).xy);
float4 Splat2=IN.color.w;
float4 Multiply0=Tex2D3.aaaa * Splat2;
float4 Lerp1=lerp(Lerp2,Tex2D3,Multiply0);
float4 Multiply4=_Color * Lerp1;
float4 Multiply5=Multiply4 * Multiply4;
float4 SplatAlpha0=_RimlightColor.w;
float4 Multiply6=SplatAlpha0 * float4( 10,10,10,10 );
float4 Fresnel1_1_NoInput = float4(0,0,1,1);
float4 Fresnel1=(1.0 - dot( normalize( float4( IN.viewDir.x, IN.viewDir.y,IN.viewDir.z,1.0 ).xyz), normalize( Fresnel1_1_NoInput.xyz ) )).xxxx;
float4 Pow2=pow(Fresnel1,_RimlightSpread.xxxx);
float4 Multiply9=_RimlightColor * Pow2;
float4 Multiply10=Multiply6 * Multiply9;
float4 SplatAlpha1=_Color.w;
float4 SplatAlpha2=Multiply0.w;
float4 Add1=SplatAlpha1 + SplatAlpha2;
float4 Add0=Add1 + Multiply11;
float4 Saturate0=saturate(Add0);
float4 Master0_1_NoInput = float4(0,0,1,1);
float4 Master0_3_NoInput = float4(0,0,0,0);
float4 Master0_4_NoInput = float4(0,0,0,0);
float4 Master0_7_NoInput = float4(0,0,0,0);
float4 Master0_6_NoInput = float4(1,1,1,1);
o.Albedo = Multiply5;
o.Emission = Multiply10;
o.Alpha = Saturate0;

				o.Normal = normalize(o.Normal);
			}
		ENDCG
	}
	Fallback "Diffuse"
}