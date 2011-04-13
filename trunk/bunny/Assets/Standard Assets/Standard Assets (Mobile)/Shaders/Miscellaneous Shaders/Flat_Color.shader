Shader "Mobile/FlatColor" {
	Properties {
		_Color ("Main Color", Color) = (1,1,1,1)
	}
	SubShader {
	
	    Lighting Off
		Material 
		{
			Ambient [_Color]
			Diffuse [_Color]
		}
		
		Pass
		{
		}

	} 
	FallBack "VertexLit", 2
}
