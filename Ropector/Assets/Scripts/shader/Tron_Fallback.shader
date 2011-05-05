Shader "Ropector/Tron_Fallback"
{
Properties {
    _MainTex ("Texture", 2D) = "white" {}
}

Category {
    Tags { "Queue"="Geometry" }
    Lighting Off
    BindChannels {
        Bind "Color", color
        Bind "Vertex", vertex
        Bind "TexCoord", texcoord
    }
    
    SubShader {
        Pass {
            SetTexture [_MainTex] {
                Combine primary DOUBLE
            }
        }
    }
}
}