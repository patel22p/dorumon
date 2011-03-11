Shader "FX/Water3" { 
Properties {
	_BumpMap ("Normalmap ", 2D) = "bump" {}
	_ShoreTex ("Shoremap ", 2D) = "" {}
	_ReflectiveColorCube ("Reflective color cube fallback (RGB)", Cube) = "" { TexGen CubeReflect }
	_MainTex ("Fallback texture", 2D) = "" {}
	_ReflectionTex ("Internal Reflection", 2D) = "" {}
		
	_DisplacementXz ("Displacement vector", Vector) = (0.0 ,0.0, 0.0, 0.0)	
	_Displacement ("Displacement parameter", Vector) = (0.0 ,0.0, 0.0, 0.0)	
		
	_ShoreTiling ("Foam tiling", Vector) = (10.0 ,10.0, 10.0, 10.0)

	_RefrColor ("Refraction fallback color", COLOR)  = ( .34, .85, .92, 1)	
	_RefrColorDepth ("Refraction depth color", COLOR)  = ( .34, .85, .92, 1)	
	_SpecularColor ("Specular color", COLOR)  = ( .72, .72, .72, 1)
	_UnderwaterColor ("Underwater color", COLOR)  = ( .782, .92, .82, 1)
	
	_Shininess ("Specular shininess", Range (0.1, 140.0)) = 20.0	
	
	_DisplacementHeightMap ("1st displacement", 2D) = "" {}
	_SecondDisplacementHeightMap ("2nd displacement", 2D) = "" {} 
}

CGINCLUDE


#include "UnityCG.cginc"

struct appdata 
{
	float4 vertex : POSITION;
	float3 normal : NORMAL;
};

struct v2f 
{
	float4 pos : SV_POSITION;
	float4 ref : TEXCOORD0;
	float4 bumpuv01 : TEXCOORD1;
	float4 vtxNormalWorld : TEXCOORD2; 	
	float4 viewDirS : TEXCOORD3;
	float4 shoreUv : TEXCOORD4;	
	float4 special : TEXCOORD5;	
};

sampler2D _BumpMap;
sampler2D _GrabTexture;
sampler2D _ReflectionTex;
sampler2D _ShoreTex;
samplerCUBE _ReflectiveColorCube;
sampler2D _CameraDepthTexture;

uniform float4 _WaveScale4;
uniform float4 _WaveOffset;
uniform float4 _DistortParams;
uniform float4 _RefrColorDepth;
uniform float4 _RefrColor;
uniform float4 _SpecularColor;
uniform float4 _UnderwaterColor;
uniform float4 _Displacement; 
uniform float _NoiseTime;
uniform float4 _InvFadeParemeter;
uniform float _Shininess;
uniform float4 _WorldLightDir;
uniform float4 _ShoreTiling;
uniform float4 _FoamWaveParams;
uniform float4 _DisplacementXz;

#define FOAM_REFRACTION _FoamWaveParams.x
#define WAVE_CAPS_AMOUNT _FoamWaveParams.y
#define WAVE_CAPS_EXP _FoamWaveParams.z

#define DISPLACEMENT_TILING _Displacement.xy
#define DISPLACEMENT_SPEED _Displacement.zw

#define OVERALL_BUMP_STRENGTH _DistortParams.x
#define REALTIME_TEXTURE_BUMP_STRENGTH _DistortParams.y
#define FRESNEL_POWER _DistortParams.z

// calculate simple sin wave based y offsets for vertex animation

half4 vertexOffsetObjectSpace(appdata_full v, float2 worldPos) 
{
	half4 retVal = half4(0,0,0,0);
	
#if defined(WATER_DISPLACEMENT_ON)	

	// simple vertex displacement based on object space coordinates
	// which means you orient the mesh to orient the waves for your
	// exactly for your taste
	
	// we also apply world space translation so that several, aligned
	// water patches are supported

	half3 vtx = v.vertex.xyz;// * half3(1.0,0.0,1.0);
	
	_DisplacementXz.xz *= unity_Scale.w;
	
	retVal.xz = sin(DISPLACEMENT_TILING * (vtx.xz + worldPos.xy) + _NoiseTime * DISPLACEMENT_SPEED);
	retVal.y = dot(_DisplacementXz.xz, retVal.xz);
	retVal.x += retVal.z;
	
	retVal.zw = half2(0,0);
	//retVal.zw = 0.01 * cos(DISPLACEMENT_TILING * (vtx.xz + worldPos.xy) + _NoiseTime * DISPLACEMENT_SPEED);
	//retVal.z = 0;//_Displacement * _DisplacementXz.x * cos(_DisplacementTiling * vtx.x*_DisplacementXz.x + _NoiseTime * _Speed);
	//retVal.w = 0;//_Displacement2 * _DisplacementXz.z * cos(_DisplacementTiling2 * vtx.z*_DisplacementXz.z + _NoiseTime * _Speed2);
	//retVal.y = 0.009 * ( (vtx.x-0.5) + _WorldPos.x *unity_Scale.w);	
#endif

	return retVal;
}

// vertex shader for all
 
v2f vert(appdata_full v)
{
	v2f o;
	
	#if defined(WATER_DISPLACEMENT_ON)
		float2 worldPos = half2(_Object2World[0][3],_Object2World[2][3]) * unity_Scale.w;
	#else
		float2 worldPos = half2(0.0,0.0);
	#endif
	
	half4 vtxOfs = vertexOffsetObjectSpace(v, worldPos);
		
	#if defined(WATER_DISPLACEMENT_ON)
		o.viewDirS.w = max(1.0,saturate(vtxOfs.x));

		// v.normal.xz += vtxOfs.zw*_FoamWaveParams.x;
		v.normal.xyz = normalize(v.normal.xyz);// + half3(vtxOfs.z,1.0,vtxOfs.w));
	#else
		o.vtxNormalWorld.w = 0.0;
		o.viewDirS.w = 1.0;
	#endif
	
	// apply vertex shader displacement
	v.vertex.y += vtxOfs.y;
	
	#if defined(WATER_DISPLACEMENT_ON)
		o.vtxNormalWorld.w = pow(saturate((v.vertex.y / unity_Scale.w) * WAVE_CAPS_AMOUNT), WAVE_CAPS_EXP) * 4.0;
	#endif		
	
	// project diplaced vertex
	o.pos = mul(UNITY_MATRIX_MVP, v.vertex);	
	
	// scrolling uv`s
	float4 temp = (v.vertex.xzxz+worldPos.xyxy) * _WaveScale4 / unity_Scale.w; // / unity_Scale.w + _WaveOffset;
	o.bumpuv01.xyzw = temp.xywz + _WaveOffset;
	o.shoreUv.xyzw = temp.xywz * _ShoreTiling + _WaveOffset; 
	
	// world space view direction (will normalize per pixel)
	o.viewDirS.xyz = -WorldSpaceViewDir(v.vertex);	
	 
	// screen space position for blend parameters
	o.ref = ComputeScreenPos(o.pos); 	
	
	// normal in world space	
	o.vtxNormalWorld.rgb =  mul((float3x3)_Object2World, v.normal.xyz * unity_Scale.w);
	
	o.special.xyz = o.viewDirS.xyz;
	o.special.w = length(o.viewDirS.xyz);//mul(UNITY_MATRIX_MV, v.vertex).z;

	
	return o; 
}

// specular component if sun lighting is enabled

half3 GetSpecularWithReflect(half3 viewDir, half3 worldNormal, half3 reflectVector) {
	half spec =  pow(saturate(dot(_WorldLightDir.xyz, reflectVector)), _Shininess);
	return _SpecularColor.xyz * spec;	
}

half3 GetSpecularWithReflectUw(half3 viewDir, half3 worldNormal, half3 reflectVector) {
	half spec =  pow(saturate(dot(_WorldLightDir.xyz * half3(1,-1,1), reflectVector)), _Shininess);
	return _SpecularColor.xyz * spec;	
}

half3 GetSpecular(half3 viewDir, half3 worldNormal) 
{
	half3 reflectVector = normalize(reflect(-viewDir.xyz, worldNormal.xyz));
	half spec =  pow(saturate(dot(_WorldLightDir.xyz, reflectVector)), _Shininess);
	return _SpecularColor.xyz * spec;	
}

// foam/shore values if edge blending and shore/foam blending is enabled

half4 getFoam(half4 shoreUv, half3 fade3, half waveAmount) 
{ 
	#if defined (EDGEBLEND_ON)		
		half4 shoreTexA = tex2D(_ShoreTex, shoreUv.xy);
		half4 shoreTexB = tex2D(_ShoreTex, shoreUv.zw);
		
		half4 foamOrShore = saturate(shoreTexA * (shoreTexB + 0.1) );
		//foamOrShore.a = saturate((1.0-fade3.y + waveAmount) * Luminance(foamOrShore.rgb*1.35)); 
		foamOrShore.a *= saturate((1.0-fade3.y + waveAmount));// * Luminance(foamOrShore.rgb*1.35)); 
			
		return foamOrShore;
	#else
		return half4(0,0,0,0);
	#endif	
}

half4 getFoamSingle(half4 shoreUv, half3 fade3, half waveAmount) 
{ 
	#if defined (EDGEBLEND_ON)		
		half4 foamOrShore = tex2D(_ShoreTex, shoreUv.xy);
		foamOrShore.a *= saturate((1.0-fade3.y + waveAmount));
		return foamOrShore;
	#else
		return half4(0,0,0,0);
	#endif	
}

half4 GetSimpleWaterFoam(half4 shoreUv, half waveAmount) {
	#if defined (EDGEBLEND_ON)		
		half4 foamOrShore = tex2D(_ShoreTex, shoreUv.xy);
		foamOrShore.a *= waveAmount;		
		return foamOrShore;
	#else
		return half4(0,0,0,0); 
	#endif	
}

half4 GetBump(sampler2D bumpTexture, half4 uv) {
	half4 bumped1 = tex2D(bumpTexture, uv.xy);
	half4 bumped2 = tex2D(bumpTexture, uv.zw);
	half4 bumpMapOffsets = (bumped1.wywy + bumped2.wywy) - 1;
	return bumpMapOffsets;
}

half4 GetBumpSingle(sampler2D bumpTexture, half4 uv) {
	half4 bumped1 = tex2D(bumpTexture, uv.xy);
	half4 bumpMapOffsets = (bumped1.wywy) - 0.5;
	return bumpMapOffsets;
}

// highest quality fragment shader (handles all possible shader settings)

half4 frag( v2f i ) : COLOR
{			
	// view vector normalize
	i.viewDirS.xyz = normalize(i.viewDirS.xyz);
	
	//return half4(i.special.www,1);
		  
	// get 'small' bumps
	half4 bump = GetBump(_BumpMap, i.bumpuv01.xyzw);
	 
	// construct a halfway smart world normal (HSWN) and normalize later
	half3 worldNormal = i.vtxNormalWorld.xyz;
	worldNormal.xz = (worldNormal.xz + bump.xy) * OVERALL_BUMP_STRENGTH;
	
	// normalize the HSWN
	worldNormal = normalize(worldNormal); 	
	
	half4 refl;
	#if defined (RT_REFLECTION_ON)	
		float4 uv1 = i.ref;
		uv1.xy += worldNormal.xz*REALTIME_TEXTURE_BUMP_STRENGTH*i.special.w;
		refl = tex2Dproj(_ReflectionTex, UNITY_PROJ_COORD(uv1));
	#endif
	
	float4 uv2 = i.ref; 
	uv2.xy += worldNormal.xz*REALTIME_TEXTURE_BUMP_STRENGTH*i.special.w;
	half4 refr = tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(uv2));

	// prevent leaking via depth comparison		
	#if defined (EDGEBLEND_ON)
	#if defined (REFRACTION_MASK_ON)
		half drefr = LinearEyeDepth (tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.ref)).r);
		half dref = LinearEyeDepth (tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(uv2)).r);
	
		half4 newRefr = tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(i.ref));
	
		if(dref<drefr)
			refr = newRefr;
	#endif
	#endif 
	 
	// here goes the final color
	half4 color = half4(0,0,0,0);
	
	// crytek style fresnel calculations
	float fcbias = 0.120373;
	float facing =  saturate(1.0-max(dot(-i.viewDirS.xyz, worldNormal), 0.0));	
	float refl2Refr = max(fcbias+(1.0-fcbias) * pow(facing,FRESNEL_POWER), 0);					
	 
	// get reflection vector for specular (and eventually cubemap reflection)
	half3 reflectVector = normalize( reflect(i.viewDirS.xyz, worldNormal.xyz) );
	half3 spec = GetSpecularWithReflect(i.viewDirS.xyz, worldNormal.xyz, -reflectVector.xyz);		
	
	#if defined (RT_REFLECTION_OFF)
		refl = texCUBE(_ReflectiveColorCube,reflectVector);
	#endif
	
	// edgeblend, shore blend
	#if defined (EDGEBLEND_ON)
		// get depth values and soft particle factor
		float sceneZ = LinearEyeDepth(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.ref)).r); 
		float3 fade3 = saturate(_InvFadeParemeter.xyz * (sceneZ-i.ref.z));
		
		// edge blending
		color.a = pow(fade3.x, _InvFadeParemeter.w);
		
		// depth/deep color blending
		refr.rgb = lerp(refr.rgb,_RefrColorDepth.rgb,fade3.z);
	#else
		color.a = 1.0;
	#endif  
		
	color.rgb = lerp(refr.rgb,refl.rgb, saturate(refl2Refr));	
	color.rgb += spec;
	
	// foam needs edge blending and foam blending for advanced modes
	#if defined (EDGEBLEND_ON)
		half4 foamColor = getFoam(i.shoreUv+bump.xyxy*FOAM_REFRACTION, fade3, saturate(i.vtxNormalWorld.w-0.1));
		color.rgb = lerp(color.rgb, foamColor.rgb, foamColor.a);
	#endif		
			
	return color;
}

// all features but optimized & tweaked for performance

half4 fragOptimized( v2f i ) : COLOR
{			
	// view vector normalize
	i.viewDirS.xyz = normalize(i.viewDirS.xyz);
			  
	// get 'small' bumps, this shader only looks up 1 bump map
	half4 bump = GetBumpSingle(_BumpMap, i.bumpuv01.xyzw);
	 
	// construct a halfway smart world normal (HSWN) and normalize later
	half3 worldNormal = i.vtxNormalWorld.xyz;
	worldNormal.xz = (worldNormal.xz + bump.xy) * OVERALL_BUMP_STRENGTH;
	
	// normalize the HSWN
	worldNormal = normalize(worldNormal); 	
	
	half4 refl;

	// reflection
	#if defined (RT_REFLECTION_ON)	
		float4 uv1 = i.ref;
		uv1.xy += worldNormal.xz*REALTIME_TEXTURE_BUMP_STRENGTH*i.special.w;
		refl = tex2Dproj(_ReflectionTex, UNITY_PROJ_COORD(uv1));
	#endif
	
	// refraction
	float4 uv2 = i.ref; 
	uv2.xy += worldNormal.xz*REALTIME_TEXTURE_BUMP_STRENGTH*i.special.w;
	half4 refr = tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(uv2));

	// prevent leaking via depth comparison		
	#if defined (EDGEBLEND_ON)
	#if defined (REFRACTION_MASK_ON)
		half drefr = LinearEyeDepth (tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.ref)).r);
		half dref = LinearEyeDepth (tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(uv2)).r);
	
		half4 newRefr = tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(i.ref));
	
		if(dref<drefr)
			refr = newRefr;
	#endif
	#endif 
	 
	// final color
	half4 color = half4(0,0,0,0);
	
	// crytek style fresnel calculations
	float fcbias = 0.120373;
	float facing =  saturate(1.0-max(dot(-i.viewDirS.xyz, worldNormal), 0.0));	
	float refl2Refr = max(fcbias+(1.0-fcbias) * pow(facing,FRESNEL_POWER), 0);					
	 
	// get reflection vector for specular (and eventually cubemap reflection) 
	half3 reflectVector = /* normalize */ ( reflect(i.viewDirS.xyz, worldNormal.xyz) );
	half3 spec = GetSpecularWithReflect(i.viewDirS.xyz, worldNormal.xyz, -reflectVector.xyz);		
	
	#if defined (RT_REFLECTION_OFF)
		refl = texCUBE(_ReflectiveColorCube,reflectVector);
	#endif
	
	// edgeblend, shore blend
	#if defined (EDGEBLEND_ON)
		// get depth values and soft particle factor
		float sceneZ = LinearEyeDepth(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.ref)).r); 
		float3 fade3 = saturate(_InvFadeParemeter.xyz * (sceneZ-i.ref.z));
		
		// edge blending
		color.a = fade3.x; // pow(fade3.x, _InvFadeParemeter.w);
		
		// depth/deep color blending
		refr.rgb = lerp(refr.rgb,_RefrColorDepth.rgb,fade3.z);
	#else
		color.a = 1.0;
	#endif  
		
	color.rgb = lerp(refr.rgb,refl.rgb, saturate(refl2Refr));	
	color.rgb += spec;
	
	// foam needs edge blending and foam blending for advanced modes
	#if defined (EDGEBLEND_ON)
		half4 foamColor = getFoamSingle(i.shoreUv+bump.xyxy*FOAM_REFRACTION, fade3, i.vtxNormalWorld.w);
		color.rgb = lerp(color.rgb, foamColor.rgb, foamColor.a);
	#endif		
			
	return color;
}

// @TODO: maybe only use the 1 bump approach to make it faster

half4 fragFast( v2f i ) : COLOR 
{ 	
	// view vector normalize
	i.viewDirS.xyz = normalize(i.viewDirS.xyz);
		
	// get 'small' bumps
	half4 bump = GetBump(_BumpMap, i.bumpuv01.xyzw);
	
	// construct a halfway smart world normal (HSWN) and normalize later
	half3 worldNormal = i.vtxNormalWorld.xyz;
	worldNormal.xz = (worldNormal.xz + bump.xy) * OVERALL_BUMP_STRENGTH;
	
	// normalize the HSWN
	worldNormal = normalize(worldNormal); 	
	
	half4 refl;
	#if defined (RT_REFLECTION_ON)	
		float4 uv1 = i.ref; 
		uv1.xy += worldNormal.xz*REALTIME_TEXTURE_BUMP_STRENGTH*i.special.w;;//*i.ref.w;
		refl = tex2Dproj(_ReflectionTex, UNITY_PROJ_COORD(uv1));
	#endif
	
	// refraction here is very simple
	half4 refr = _RefrColor;
		
	half4 color = half4(0,0,0,0);		
	
	// fresnel calculations
	float fcbias = 0.120373;
	float facing =  saturate(1.0-max(dot(-i.viewDirS.xyz, worldNormal), 0.0));	
	float refl2Refr = max(fcbias+(1.0-fcbias) * pow(facing,FRESNEL_POWER), 0);					
	
	// get reflection vector for specular (and eventually cubemap reflection)
	half3 reflectVector = reflect(i.viewDirS.xyz, worldNormal.xyz);
	half3 spec = GetSpecularWithReflect(i.viewDirS.xyz, worldNormal.xyz, -reflectVector.xyz);	
	
	#if defined (RT_REFLECTION_OFF)
		refl = texCUBE(_ReflectiveColorCube,reflectVector);
	#endif	
	
	float3 fade3 = float3(0,0,0);
	
	// edgeblend, shore blend
	#if defined (EDGEBLEND_ON)
		// get depth values and soft particle factor
		float sceneZ = LinearEyeDepth(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.ref)).r); 
		fade3 = saturate(_InvFadeParemeter.xyz * (sceneZ-i.ref.z));
		
		// edge blending
		color.a = dot(fade3.xx, half2(refl2Refr,0.25));
		
		// depth/deep color blending
		refr.rgb = lerp(refr.rgb,_RefrColorDepth.rgb,1.0-fade3.z);
	#else
		color.a = saturate(refl2Refr+0.25);
	#endif  
		
	color.rgb = lerp(refr.rgb,refl.rgb, saturate(refl2Refr));	
	color.rgb += spec;
	
	half4 foamColor = GetSimpleWaterFoam(i.shoreUv+bump.xyxy*FOAM_REFRACTION, (i.vtxNormalWorld.w+1.0-fade3.y));
	color.rgb = lerp(color.rgb, foamColor.rgb, foamColor.a);
		
	return color;
}

// @TODO: maybe only use the 1 bump approach to make it faster

half4 fragSimpleWater( v2f i ) : COLOR 
{	
	// view vector normalize
	i.viewDirS.xyz = normalize(i.viewDirS.xyz);
		
	// get 'small' bumps
	half4 bump = GetBump(_BumpMap, i.bumpuv01.xyzw);
	
	// final color is between refracted and reflected based on fresnel	
	half4 color = half4(0,0,0,0);	
	
	// construct a halfway smart world normal (HSWN) and normalize later
	half3 worldNormal = i.vtxNormalWorld.xyz;
	worldNormal.xz = (worldNormal.xz + bump.xy) * OVERALL_BUMP_STRENGTH;
	
	// normalize the HSWN
	worldNormal = normalize(worldNormal); 	
	
	// get reflection vector for specular (and eventually cubemap reflection)
	half3 reflectVector = reflect(i.viewDirS.xyz, worldNormal.xyz);

	half4 water = texCUBE(_ReflectiveColorCube,reflectVector);// * _HorizonColor.rgba;
	
	// fresnel calculations
	float fcbias = 0.120373; 
	float facing =  saturate(1.0-max(dot(-i.viewDirS.xyz, worldNormal), 0.0));	
	float refl2Refr = max(fcbias+(1.0-fcbias) * pow(facing,FRESNEL_POWER), 0);			

	water.rgb = lerp(_RefrColor.rgb,water.rgb, refl2Refr);	
	water.a = saturate(refl2Refr+0.25);
	
	water.rgb += GetSpecularWithReflect(i.viewDirS.xyz, worldNormal.xyz, -reflectVector.xyz);	 
	
	half4 foamColor = GetSimpleWaterFoam(i.shoreUv+bump.xyxy*FOAM_REFRACTION, i.vtxNormalWorld.w);
	water.rgb = lerp(water.rgb, foamColor.rgb, foamColor.a);	
	
	return water;
}

half4 fragUnderwater( v2f i ) : COLOR 
{	
	// view vector normalize
	i.viewDirS.xyz = normalize(i.viewDirS.xyz);
		
	// get 'small' bumps
	half4 bump = GetBump(_BumpMap, i.bumpuv01.xyzw);
	
	// construct a halfway smart world normal (HSWN) and normalize later
	half3 worldNormal = i.vtxNormalWorld.xyz;
	worldNormal.xz = (worldNormal.xz + bump.xy) * OVERALL_BUMP_STRENGTH;
	
	// normalize the HSWN
	worldNormal = normalize(worldNormal); 	
	
	half4 refl;
		float4 uv1 = i.ref; 
		uv1.xy += worldNormal.xz*REALTIME_TEXTURE_BUMP_STRENGTH*i.special.w;;//*i.ref.w;
		refl = tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(uv1));
	
	//refl = tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(i.ref));
	
	//return refl;
		
	half4 color = refl * _UnderwaterColor;		
				
	
	// get reflection vector for specular (and eventually cubemap reflection)
	half3 reflectVector = reflect(i.viewDirS.xyz, worldNormal.xyz);
	half3 spec = GetSpecularWithReflectUw(i.viewDirS.xyz, worldNormal.xyz, -reflectVector.xyz);	
	
			color.rgb += spec;
						
			color.a = 1.0; // BLOOM
		
	return color;
}

half4 fragUnderwaterDepth( v2f i ) : COLOR 
{	
	half4 color = half4(0,0,0,1);
	return color;
}

ENDCG

Subshader 
{ 
	Tags {"RenderType"="Transparent" "Queue"="Transparent"}
	
	Lod 500
	ColorMask RGB
			
	GrabPass { }

	Pass { 
			Blend SrcAlpha OneMinusSrcAlpha
		
			ColorMask RGB
			ZTest LEqual
			ZWrite Off
			Cull Off
			
			CGPROGRAM
			
			// the fancy shader needs 3.0 target, 
			// refer to RefractionOnly or Simple otherwise
			
			#pragma target 3.0 
			
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest 
			  
			#pragma multi_compile WATER_DISPLACEMENT_ON WATER_DISPLACEMENT_OFF
			#pragma multi_compile EDGEBLEND_ON EDGEBLEND_OFF
			#pragma multi_compile REFRACTION_MASK_ON REFRACTION_MASK_OFF
			#pragma multi_compile RT_REFLECTION_ON RT_REFLECTION_OFF
			
			ENDCG
	}
	
	// write depth into depth channel to prevent skybox and other crap to remove water
}

Subshader 
{ 
	Tags {"RenderType"="Transparent" "Queue"="Transparent"}
	
	Lod 400
	ColorMask RGB
			
	GrabPass { }

	Pass { 
			Blend SrcAlpha OneMinusSrcAlpha
		
			ColorMask RGB
			ZTest LEqual
			ZWrite Off
			Cull Off
			
			CGPROGRAM
			
			// the fancy shader needs 3.0 target, 
			// refer to RefractionOnly or Simple otherwise
			
			#pragma target 3.0 
			
			#pragma vertex vert
			#pragma fragment fragOptimized
			#pragma fragmentoption ARB_precision_hint_fastest 
			  
			#pragma multi_compile WATER_DISPLACEMENT_ON WATER_DISPLACEMENT_OFF
			#pragma multi_compile EDGEBLEND_ON EDGEBLEND_OFF
			#pragma multi_compile REFRACTION_MASK_ON REFRACTION_MASK_OFF
			#pragma multi_compile RT_REFLECTION_ON RT_REFLECTION_OFF
			
			ENDCG
	}
	
	// write depth into depth channel to prevent skybox and other crap to remove water
}

Subshader { 
	Tags {"RenderType"="Transparent" "Queue"="Transparent"}

	Lod 300
	ColorMask RGB 	
		
	Pass {
	
		Blend SrcAlpha OneMinusSrcAlpha
		
		ColorMask RGB
		ZTest LEqual
		ZWrite Off
		Cull Off	
	
		CGPROGRAM
		
		#pragma vertex vert
		#pragma fragment fragFast
		#pragma fragmentoption ARB_precision_hint_fastest 
		
		#pragma multi_compile WATER_DISPLACEMENT_ON WATER_DISPLACEMENT_OFF
		#pragma multi_compile EDGEBLEND_ON EDGEBLEND_OFF
		#pragma multi_compile RT_REFLECTION_ON RT_REFLECTION_OFF

		ENDCG
	}
}


Subshader { 
	Tags {"RenderType"="Transparent" "Queue"="Transparent"}
	
	Lod 100
	ColorMask RGB
			
	Pass {
		
		Blend SrcAlpha OneMinusSrcAlpha
		
		ColorMask RGB
		ZTest LEqual
		ZWrite Off
		Cull Off			
	
		CGPROGRAM
	
		#pragma vertex vert
		#pragma fragment fragSimpleWater
		#pragma fragmentoption ARB_precision_hint_fastest 
	
		#pragma multi_compile WATER_DISPLACEMENT_ON WATER_DISPLACEMENT_OFF
		#pragma multi_compile EDGEBLEND_ON EDGEBLEND_OFF
	
		ENDCG
	}
}

// HACK: underwater test
// TODO: create a common header & new shader for this, once it is proven

Subshader { 
	Tags {"RenderType"="Transparent" "Queue"="Transparent"}
	
	Lod 50 // <- HACK
	
	GrabPass {}
				
	Pass {
		Blend SrcAlpha OneMinusSrcAlpha
		
		ColorMask RGB
		ZTest LEqual
		ZWrite Off
		Cull Off			
	
		CGPROGRAM
	
		#pragma vertex vert
		#pragma fragment fragUnderwater
		#pragma fragmentoption ARB_precision_hint_fastest 
	
		#pragma multi_compile WATER_DISPLACEMENT_ON WATER_DISPLACEMENT_OFF
	
		ENDCG
	}
}

/*
Subshader { 
	Tags {"RenderType"="Transparent" "Queue"="Transparent"}
	
	Lod 10 // <- HACK
	
	GrabPass {}
				
	Pass {
		Blend SrcAlpha OneMinusSrcAlpha
		
		ColorMask A
		ZTest LEqual
		ZWrite Off
		Cull Off			
	
		CGPROGRAM
	
		#pragma vertex vert
		#pragma fragment fragUnderwaterDepth
		#pragma fragmentoption ARB_precision_hint_fastest 
	
		#pragma multi_compile WATER_DISPLACEMENT_ON WATER_DISPLACEMENT_OFF
	
		ENDCG
	}
}
*/

// fixed function, simple support for 
// (very very) old hardware ...

// iPhone/iPad versions coming soon ...

// three texture, cubemaps
Subshader {
	
	Tags {"RenderType"="Transparent"}
	Pass {
		Color (0.5,0.5,0.5,0.5)
		SetTexture [_MainTex] {
			Matrix [_WaveMatrix]
			combine texture * primary
		}
		SetTexture [_MainTex] {
			Matrix [_WaveMatrix2]
			combine texture * primary + previous
		}
		SetTexture [_ReflectiveColorCube] {
			combine texture +- previous, primary
			Matrix [_Reflection]
		}
	}
}

// dual texture, cubemaps
Subshader {
	Tags {"RenderType"="Transparent"}
	Pass {
		Color (0.5,0.5,0.5,0.5)
		SetTexture [_MainTex] {
			Matrix [_WaveMatrix]
			combine texture
		}
		SetTexture [_ReflectiveColorCube] {
			combine texture +- previous, primary
			Matrix [_Reflection]
		}
	}
}

// single texture
Subshader {
	Tags {"RenderType"="Transparent"}
	Pass {
		Color (0.5,0.5,0.5,0)
		SetTexture [_MainTex] {
			Matrix [_WaveMatrix]
			combine texture, primary
		}
	}
}


}
