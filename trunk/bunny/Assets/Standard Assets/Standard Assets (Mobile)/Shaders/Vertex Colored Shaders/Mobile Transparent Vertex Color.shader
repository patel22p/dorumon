Shader "Mobile/Transparent/Vertex Color" {
Properties {
	_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
}

Category {
	Tags {"Queue"="Geometry" "IgnoreProjector"="True" "RenderType"="Opaque"}
	ZWrite Off
	Alphatest Greater 0
	Cull Off
	Blend SrcAlpha OneMinusSrcAlpha 
	SubShader {
		Pass {
			Fog { Mode Off }
			Lighting Off
        	SetTexture [_MainTex] {
            Combine texture
        }
		}
	} 
}
}