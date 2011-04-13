// Upgrade NOTE: replaced 'glstate.matrix.mvp' with 'UNITY_MATRIX_MVP'

Shader "Custom/VertexColorsOnly" { 
	SubShader {
		Pass {
			Fog { Mode Off }
		CGPROGRAM
// Upgrade NOTE: excluded shader from Xbox360; has structs without semantics (struct appdata members vertex,color)
#pragma exclude_renderers xbox360
		#pragma vertex vert
		#pragma fragment frag

			// vertex input: position, UV
			struct appdata {
				float4 vertex;
				float4 color;
			};

			struct v2f {
				float4 pos : POSITION;
				float4 color : COLOR;
			};
			
			v2f vert (appdata v) {
				v2f o;
				o.pos = mul( UNITY_MATRIX_MVP, v.vertex );
				o.color = v.color;
				return o;
			}
			
			half4 frag( v2f i ) : COLOR {
				return i.color;
			}
			
			ENDCG
		}
	}
}