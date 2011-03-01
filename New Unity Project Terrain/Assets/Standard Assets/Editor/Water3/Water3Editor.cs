using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor (typeof(Water3))]

public class Water3Editor : Editor 
{	
	public  SerializedObject m_SerObj;	
	public SerializedProperty m_AutoEdgeBlend;
	public SerializedProperty m_WaterDisplacement;
	public SerializedProperty m_RefractionMask;
	public SerializedProperty m_Realtime2DReflection;
	 
	public SerializedProperty m_WaterMode;

	public SerializedProperty m_DisablePixelLights;
	public SerializedProperty m_TextureSize;
	public SerializedProperty m_ClipPlaneOffset;
	public SerializedProperty m_ReflectLayers;
	
	public SerializedProperty m_LightTransform;
	public SerializedProperty m_Shininess;
	
	public SerializedProperty m_WaveScale;
	public SerializedProperty m_WaveSpeedBumpA;
	public SerializedProperty m_WaveSpeedBumpB;

	public SerializedProperty m_DistortParams;
	
	public SerializedProperty m_FadeExp;// = 1.0F;
	public SerializedProperty m_InvFade;// = 1.0F;
	public SerializedProperty m_InvFadeFoam;// = 1.0F;
	public SerializedProperty m_InvFadeDepthFade;// = 1.0F;
	
	public SerializedProperty m_ShoreTilingBumpA;// = new Vector4(10.0F, 10.0F, 10.0F, 10.0F);
	public SerializedProperty m_ShoreTilingBumpB;// = new Vector4(10.0F, 10.0F, 10.0F, 10.0F);

	public SerializedProperty m_FoamWaveParams;
	public SerializedProperty m_UnderwaterCheckOffset;
	
	public SerializedProperty m_WaterManager;
		
	Texture m_NormalMap;
	Texture m_FoamMap;
	Texture m_CubeFallback;
	Texture m_Fallback;
	
	Texture m_Displacement;
	Texture m_2ndDisplacement;
	
	public bool m_ShowReflectionSettings = false;
		
    public void OnEnable() 
	{		
		m_SerObj = new SerializedObject (target);

		m_Realtime2DReflection = m_SerObj.FindProperty("realtime2DReflection");
		m_AutoEdgeBlend = m_SerObj.FindProperty("autoEdgeBlend");
		m_WaterDisplacement = m_SerObj.FindProperty("waterDisplacement");
		m_RefractionMask = m_SerObj.FindProperty("refractionMask");
		
		m_UnderwaterCheckOffset = m_SerObj.FindProperty("m_UnderwaterCheckOffset");
						
		m_WaterMode = m_SerObj.FindProperty("m_WaterMode");
		m_DisablePixelLights = m_SerObj.FindProperty("m_DisablePixelLights");
		m_TextureSize = m_SerObj.FindProperty("m_TextureSize");
		m_ClipPlaneOffset = m_SerObj.FindProperty("m_ClipPlaneOffset");
		m_ReflectLayers = m_SerObj.FindProperty("m_ReflectLayers");
		
		m_LightTransform = m_SerObj.FindProperty("lightTransform");
		m_Shininess = m_SerObj.FindProperty("m_Shininess"); 
		
		m_WaveScale = m_SerObj.FindProperty("m_WaveScale");
		m_WaveSpeedBumpA = m_SerObj.FindProperty("m_WaveSpeedBumpA");
		m_WaveSpeedBumpB = m_SerObj.FindProperty("m_WaveSpeedBumpB");
		
		m_DistortParams = m_SerObj.FindProperty("m_DistortParams");
		
		m_FadeExp = m_SerObj.FindProperty("m_FadeExp");
		m_InvFade = m_SerObj.FindProperty("m_InvFade");
		m_InvFadeFoam = m_SerObj.FindProperty("m_InvFadeFoam");
		m_InvFadeDepthFade = m_SerObj.FindProperty("m_InvFadeDepthFade");
		
		m_ShoreTilingBumpA = m_SerObj.FindProperty("m_ShoreTilingBumpA");
		m_ShoreTilingBumpB = m_SerObj.FindProperty("m_ShoreTilingBumpB");

		m_FoamWaveParams = m_SerObj.FindProperty("m_FoamWaveParams");	
		
		m_WaterManager = m_SerObj.FindProperty("m_WaterManager");
		
		m_Displacement = Water3Manager.Instance().GetMaterialTexture("_DisplacementHeightMap");
		m_2ndDisplacement = Water3Manager.Instance().GetMaterialTexture("_SecondDisplacementHeightMap");
		
		m_NormalMap = Water3Manager.Instance().GetMaterialTexture("_BumpMap");
		m_FoamMap = Water3Manager.Instance().GetMaterialTexture("_ShoreTex");
		m_CubeFallback = Water3Manager.Instance().GetMaterialTexture("_ReflectiveColorCube");
		m_Fallback = Water3Manager.Instance().GetMaterialTexture("_MainTex");
	} 

    public override void OnInspectorGUI() 
	{		
		m_SerObj.Update();
			
		EditorGUILayout.Separator ();	
		
		EditorGUILayout.PropertyField (m_WaterMode, new GUIContent("Water mode", "Choose overall water material and shader complexity"));
		EditorGUILayout.Separator ();		
		

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PropertyField (m_Realtime2DReflection, new GUIContent("Realtime 2D reflect"));
			EditorGUILayout.PropertyField (m_AutoEdgeBlend, new GUIContent("Edge blending"));
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PropertyField (m_WaterDisplacement, new GUIContent("Displacement"));
			EditorGUILayout.PropertyField (m_RefractionMask, new GUIContent("Refraction fix"));
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Separator ();
		
		
		GUILayout.Label("01. Small Waves (\"Bump\")", EditorStyles.boldLabel);
		m_WaveScale.floatValue = EditorGUILayout.Slider("Scale", m_WaveScale.floatValue, 0.001F, 0.2F);
		
		EditorGUILayout.BeginHorizontal();
		m_WaveSpeedBumpA.vector2Value = EditorGUILayout.Vector2Field(("Speed A"),m_WaveSpeedBumpA.vector2Value);
		m_WaveSpeedBumpB.vector2Value = EditorGUILayout.Vector2Field(("Speed B"),m_WaveSpeedBumpB.vector2Value);
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.Separator ();		
		
		GUILayout.Label("02. Shore and Foam", EditorStyles.boldLabel);
		if(m_AutoEdgeBlend.boolValue) 
		{
			EditorGUILayout.BeginHorizontal();
			m_ShoreTilingBumpA.vector2Value = EditorGUILayout.Vector2Field("Foam texture tiling A", m_ShoreTilingBumpA.vector2Value);
			m_ShoreTilingBumpB.vector2Value = EditorGUILayout.Vector2Field("Foam texture tiling B", m_ShoreTilingBumpB.vector2Value);
			EditorGUILayout.EndHorizontal();
			float foamTempY;
			float foamTempX;
			float foamTempZ;
			foamTempY = EditorGUILayout.Slider("Wave caps", m_FoamWaveParams.vector3Value.y, 0.0F, 3.0F);
			foamTempX = EditorGUILayout.Slider("Foam distort", m_FoamWaveParams.vector3Value.x, -1.5F, 1.5F);
			foamTempZ = EditorGUILayout.Slider("Wave power", m_FoamWaveParams.vector3Value.z, 0.0F, 20.0F);			
			m_FoamWaveParams.vector3Value = new Vector3(foamTempX, foamTempY, foamTempZ);			
		} 
		else 
		{
			GUILayout.Label("Enable Edge blending in compilation settings");
		}
				
		EditorGUILayout.Separator ();

		GUILayout.Label("03. Bump and Fresnel Settings", EditorStyles.boldLabel);

		float refrX = EditorGUILayout.FloatField("Intensity", m_DistortParams.vector3Value.x);
		float refrY = EditorGUILayout.FloatField(new GUIContent("Texture Bump", "Used when realtime reflections are enabled (2D reflection plane and realtime refraction)"), m_DistortParams.vector3Value.y);
		float fresnel = EditorGUILayout.Slider("Fresnel power", m_DistortParams.vector3Value.z, 0.0F, 10.0F);
		m_DistortParams.vector3Value = new Vector3(refrX,refrY,fresnel);
		
		EditorGUILayout.Separator ();
		
		GUILayout.Label(new GUIContent("02. Fading & Blending", "Parameters to control when water edges should fade out or foam textures fade in."), EditorStyles.boldLabel);
		if(m_AutoEdgeBlend.boolValue) {
			m_FadeExp.floatValue = EditorGUILayout.Slider("Exponent", m_FadeExp.floatValue, 0.0F, 5.0F);
			m_InvFade.floatValue = EditorGUILayout.Slider("Edges", m_InvFade.floatValue, 0.035F, 5.0F);
			m_InvFadeDepthFade.floatValue = EditorGUILayout.Slider("Depth", m_InvFadeDepthFade.floatValue, 0.0F, 1.0F);
			m_InvFadeFoam.floatValue = EditorGUILayout.Slider("Foam & Shores", m_InvFadeFoam.floatValue, 0.035F, 5.0F);
		}
		
		EditorGUILayout.Separator ();
		 
		GUILayout.Label("04. Chose Colors", EditorStyles.boldLabel  );		
		
		Water3Manager.Instance().SetMaterialColor(
			"_RefrColorDepth", 
			EditorGUILayout.ColorField("Depth Fade", Water3Manager.Instance().GetMaterialColor("_RefrColorDepth")) );
		Water3Manager.Instance().SetMaterialColor(
			"_SpecularColor", 
			EditorGUILayout.ColorField("Specular", Water3Manager.Instance().GetMaterialColor("_SpecularColor")) );
		Water3Manager.Instance().SetMaterialColor(
			"_RefrColor", 
			EditorGUILayout.ColorField("Fallback", Water3Manager.Instance().GetMaterialColor("_RefrColor")) );
			//_UnderwaterColor
		Water3Manager.Instance().SetMaterialColor(
			"_UnderwaterColor", 
			EditorGUILayout.ColorField("Underwater", Water3Manager.Instance().GetMaterialColor("_UnderwaterColor")) );
					
		EditorGUILayout.Separator ();
		
		GUILayout.Label(new GUIContent("05. Sun Lighting?", "Create specular highlights from a sun direction"), EditorStyles.boldLabel);	
		EditorGUILayout.PropertyField (m_LightTransform, new GUIContent("Sun"));
		m_Shininess.floatValue = EditorGUILayout.Slider("Shininess", m_Shininess.floatValue, 0.1F, 240.0F);
	
		
		EditorGUILayout.Separator (); 
		
		GUILayout.Label(new GUIContent("06. Mesh Vertex Displacement", "Displace the mesh based on specified vertex colors and mesh tesselation"), EditorStyles.boldLabel );	
		if(m_WaterDisplacement.boolValue) {
			
			Water3Manager.Instance().m_CpuDisplacementModel = (Water3Manager.CpuDisplacementModel)EditorGUILayout.EnumPopup("CPU based", ((Water3Manager.CpuDisplacementModel)Water3Manager.Instance().m_CpuDisplacementModel));
			EditorGUILayout.BeginHorizontal();
			m_Displacement = (Texture2D) EditorGUILayout.ObjectField("Height map A", m_Displacement as Object, typeof(Texture2D));
			m_2ndDisplacement = (Texture2D) EditorGUILayout.ObjectField("Height map B", m_2ndDisplacement as Object, typeof(Texture2D));
			
			Water3Manager.Instance().SetDisplacementHeightMap(m_Displacement as Texture2D, 0);
			Water3Manager.Instance().SetMaterialTexture("_DisplacementHeightMap",m_Displacement as Texture2D);
			Water3Manager.Instance().SetDisplacementHeightMap(m_2ndDisplacement as Texture2D, 1);
			Water3Manager.Instance().SetMaterialTexture("_SecondDisplacementHeightMap",m_2ndDisplacement as Texture2D);


			EditorGUILayout.EndHorizontal();

			// CPU mesh displacement
			EditorGUILayout.BeginHorizontal();
			Water3Manager.Instance().m_DisplacementTiling = EditorGUILayout.FloatField(new GUIContent("Tiling"),Water3Manager.Instance().m_DisplacementTiling);
			Water3Manager.Instance().m_SmallWavesSpeed = EditorGUILayout.FloatField(new GUIContent("Speed"),Water3Manager.Instance().m_SmallWavesSpeed);	
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			Water3Manager.Instance().m_HeightDisplacement = EditorGUILayout.FloatField(new GUIContent("Height"),Water3Manager.Instance().m_HeightDisplacement);
			Water3Manager.Instance().m_NormalsDisplacement = EditorGUILayout.FloatField(new GUIContent("Normals Strength"),Water3Manager.Instance().m_NormalsDisplacement);			
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.Separator (); 
			
			// GPU mesh displacement (simple sin waves)
			/*
			m_SharedWaterMaterial.SetVector("_DisplacementXz", m_DisplacementXz);	
				
			// reorder parameter for performance: tiling, tiling2, speed, speed2*
			m_SharedWaterMaterial.SetVector(
				"_Displacement",
				new Vector4(m_DisplacementParams.z,m_SecondDisplacementParams.z,m_DisplacementParams.x,m_SecondDisplacementParams.x));			
			*/
			Vector4 displacementXz = Water3Manager.Instance().GetMaterialVector("_DisplacementXz");
			Vector4 displacement4 = Water3Manager.Instance().GetMaterialVector("_Displacement");
			
			GUILayout.Label("GPU based (applied in vertex shader)");
			
			displacementXz.x = EditorGUILayout.FloatField("Wave X",displacementXz.x);							
			displacement4.z = EditorGUILayout.Slider("Speed", displacement4.z, -3.0F, 3.0F);
			displacement4.x = EditorGUILayout.Slider("Tiling",displacement4.x, 0.0F, 100.0F);
			
			EditorGUILayout.Separator (); 
			
			displacementXz.z = EditorGUILayout.FloatField("Wave Z", displacementXz.z);			
			displacement4.w = EditorGUILayout.Slider("Speed", displacement4.w, -3.0F, 3.0F);
			displacement4.y = EditorGUILayout.Slider("Tiling", displacement4.y, 0.0F, 100.0F);
						
			Water3Manager.Instance().SetMaterialVector("_DisplacementXz", displacementXz);
			Water3Manager.Instance().SetMaterialVector("_Displacement", displacement4);
			
			//float tempX = EditorGUILayout.Slider("Affect normals", m_FoamWaveParams.vector2Value.x, -1.0F, 1.0F);
			//m_FoamWaveParams.vector2Value = new Vector2(tempX, m_FoamWaveParams.vector2Value.y);				
		}
		
		EditorGUILayout.Separator ();
		/*
		// test
		if( GUILayout.Button("Smooth noise") ) 
		{
			Mesh mesh = ((MeshFilter)m_TargetGo.GetComponent(typeof(MeshFilter))).sharedMesh;
			Color[] colors =  mesh.colors;			
			int[] tris = mesh.triangles;
			
			for(int i = 2; i < tris.Length-3; i += 3) {
				Color col0 = colors[tris[i-2]];
				Color colA = colors[tris[i-1]];
				Color colB = colors[tris[i]];
				Color colC = colors[tris[i+1]];
				Color colD = colors[tris[i+2]];
				
				col0 = col0 + colA + colB + colC + colD;
				col0 = col0 / 5.0F;
				
				colors[tris[i]] = col0;
			} 
			
			mesh.colors = colors;
		}	
		*/	
		
		/*
		EditorGUILayout.BeginHorizontal();
		if( GUILayout.Button("Animation noise") ) {
			Mesh mesh = ((MeshFilter)m_TargetGo.GetComponent(typeof(MeshFilter))).sharedMesh;
			Color[] colors =  mesh.colors;
			for(int i = 0; i < colors.Length; i++) {
				colors[i].r = Random.Range(0.0F, 1.0F);
				colors[i].g = Random.Range(0.0F, 1.0F);
				colors[i].b = Random.Range(0.0F, 1.0F);
			} 
			mesh.colors = colors;
		}		
		
		if( GUILayout.Button("Smooth noise") ) 
		{
			Mesh mesh = ((MeshFilter)m_TargetGo.GetComponent(typeof(MeshFilter))).sharedMesh;
			Color[] colors =  mesh.colors;			
			int[] tris = mesh.triangles;
			
			for(int i = 2; i < tris.Length-3; i += 3) {
				Color col0 = colors[tris[i-2]];
				Color colA = colors[tris[i-1]];
				Color colB = colors[tris[i]];
				Color colC = colors[tris[i+1]];
				Color colD = colors[tris[i+2]];
				
				col0 = col0 + colA + colB + colC + colD;
				col0 = col0 / 5.0F;
				
				colors[tris[i]] = col0;
			} 
			
			mesh.colors = colors;
		}
		EditorGUILayout.EndHorizontal();		
		
		EditorGUILayout.BeginHorizontal();
		if( GUILayout.Button("Directional animation") ) 
		{
			Mesh mesh = ((MeshFilter)m_TargetGo.GetComponent(typeof(MeshFilter))).sharedMesh;
			Color[] colors =  mesh.colors;			
			int[] tris = mesh.triangles;
			Vector3[] verts = mesh.vertices;
			
			for(int i = 0; i < tris.Length; i ++) 
			{
				int index = tris[i];
				Vector3 vert = verts[index];
				
				float seedVal = vert.x * 10.1F;
				
				colors[index].r = Mathf.PingPong(seedVal, 1.0F);
				colors[index].g = Mathf.PingPong(seedVal, 1.0F);
				colors[index].b = Mathf.PingPong(seedVal, 1.0F);
			} 
			mesh.colors = colors;
			
		}		
		EditorGUILayout.EndHorizontal();	
		EditorGUILayout.Separator ();

		*/
		
		GUILayout.Label(new GUIContent("06. Textures", "Specify textures for advanced and fallback rendering"), EditorStyles.boldLabel );	
		EditorGUILayout.BeginHorizontal();
		m_NormalMap = EditorGUILayout.ObjectField("Bump", m_NormalMap as Object, typeof(Texture2D)) as Texture2D;
		Water3Manager.Instance().SetMaterialTexture("_BumpMap",m_NormalMap);
		m_FoamMap = EditorGUILayout.ObjectField("Foam", m_FoamMap as Object, typeof(Texture2D)) as Texture2D;
		Water3Manager.Instance().SetMaterialTexture("_ShoreTex",m_FoamMap);
		EditorGUILayout.EndHorizontal();	
		EditorGUILayout.BeginHorizontal();
		m_CubeFallback = EditorGUILayout.ObjectField("Static cubemap", m_CubeFallback as Object, typeof(Cubemap)) as Cubemap;
		Water3Manager.Instance().SetMaterialTexture("_ReflectiveColorCube",m_CubeFallback);
		m_Fallback = EditorGUILayout.ObjectField("Fallback", m_Fallback as Object, typeof(Texture2D)) as Texture2D;
		Water3Manager.Instance().SetMaterialTexture("_MainTex",m_Fallback);
		EditorGUILayout.EndHorizontal();	
		
		EditorGUILayout.Separator ();
		
		GUILayout.Label("Other settings", EditorStyles.boldLabel);
		EditorGUILayout.PropertyField(m_UnderwaterCheckOffset, new GUIContent("Underwater offset"));
		EditorGUILayout.Separator ();
		m_ShowReflectionSettings = EditorGUILayout.Foldout (m_ShowReflectionSettings, "RT Reflection settings");
		if(m_ShowReflectionSettings) {
			EditorGUILayout.PropertyField (m_DisablePixelLights, new GUIContent("No pixel lights"));
			EditorGUILayout.PropertyField (m_TextureSize, new GUIContent("Texture size"));
			if(m_TextureSize.intValue < 2)
				m_TextureSize.intValue = 2;
			else
				m_TextureSize.intValue = Mathf.FloorToInt(Mathf.Pow(2.0F, Mathf.Floor(Mathf.Log((float)m_TextureSize.intValue, 2.0F))));
			EditorGUILayout.PropertyField (m_ClipPlaneOffset, new GUIContent("Clip plane ofs"));
			EditorGUILayout.PropertyField (m_ReflectLayers, new GUIContent("Reflect layers"));
		}
		
		EditorGUILayout.Separator ();

		//if (GUI.changed)
		
		m_SerObj.ApplyModifiedProperties();		
	} 
		
}