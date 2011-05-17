Shader "Bunny/Foliage"
{
	Properties 
	{
_Color("_Color", Color) = (1,1,1,1)
_AlphaCutoff("_AlphaCutoff", Float) = 0.5
_Diffuse("_Diffuse", 2D) = "black" {}

	}
	
	SubShader 
	{
		Tags
		{
"Queue"="Transparent"
"IgnoreProjector"="False"
"RenderType"="Transparent"

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
float4 Invert0= float4(1.0, 1.0, 1.0, 1.0) - light;
float4 Multiply1=float4( 0.25,0.25,0.25,1) * Invert0;
float4 Add0=Multiply1 + light;
float4 Multiply2=float4( s.Albedo.x, s.Albedo.y, s.Albedo.z, 1.0 ) * Add0;
float4 Multiply0=Multiply2 * s.Alpha.xxxx;
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
				float2 uv_Diffuse;

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
				
float4 Tex2D0=tex2D(_Diffuse,(IN.uv_Diffuse.xyxy).xy);
float4 Multiply1=_Color * Tex2D0;
float4 SplatAlpha0=_Color.w;
float4 Subtract0=Tex2D0.aaaa - _AlphaCutoff.xxxx;
float4 Master0_1_NoInput = float4(0,0,1,1);
float4 Master0_2_NoInput = float4(0,0,0,0);
float4 Master0_3_NoInput = float4(0,0,0,0);
float4 Master0_4_NoInput = float4(0,0,0,0);
float4 Master0_7_NoInput = float4(0,0,0,0);
clip( Subtract0 );
o.Albedo = Multiply1;
o.Alpha = SplatAlpha0;

				o.Normal = normalize(o.Normal);
			}
		ENDCG
	}
	Fallback ""
}