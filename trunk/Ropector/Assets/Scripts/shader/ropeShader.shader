Shader "Solid" {
	Properties {
        _Color ("Main Color", COLOR) = (1,1,1,1)
    }
    SubShader {
        Pass { 
			//Color _Color 
			Color [_Color]
			Cull Front
        }
        Pass { 
			//Color _Color 
			Color [_Color]
			Cull Back
        }
    }
} 