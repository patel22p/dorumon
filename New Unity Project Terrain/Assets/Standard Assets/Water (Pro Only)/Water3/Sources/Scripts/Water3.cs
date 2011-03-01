using UnityEngine;
using System.Collections;

[ExecuteInEditMode]

public class Water3 : MonoBehaviour
{
	public enum WaterMode {
		Indie = 0,
		FastAndNoRefraction = 1,
		Optimized = 2,
		Everything = 3,
	};
				
	// the main water complexity, "Indie" is the only
	// mode that can run with unity free
	public WaterMode m_WaterMode = WaterMode.Everything;
	
	public Water3Manager m_WaterManager = null;
	
	// realtime reflection settings
	public bool m_DisablePixelLights = true;
	public int m_TextureSize = 256;
	public float m_ClipPlaneOffset = 0.07f;
	public LayerMask m_ReflectLayers = -1;
	private Hashtable m_ReflectionCameras = new Hashtable(); // Camera -> Camera table
	private RenderTexture m_ReflectionTexture = null;
	private WaterMode m_HardwareWaterSupport = WaterMode.Everything;
	private int m_OldReflectionTextureSize = 0;
	
	
	// prevent recursive function entering below
	private static bool s_InsideWater = false;
		
	// shader compilation parameters
	public bool realtime2DReflection = true;
	public bool autoEdgeBlend = true;
	public bool waterDisplacement = true;
	public bool refractionMask = false;
	
	// shader parameters
	public float m_Shininess = 100.0F;
	public float m_WaveScale = 0.04F;
		
	public Transform lightTransform;	
	
	public Vector3 m_FoamWaveParams;	
	
	public Vector2 m_WaveSpeedBumpA;
	public Vector2 m_WaveSpeedBumpB;	
	
	public Vector3 m_DistortParams = new Vector3(0.18F,0.8F,2.0F);
	
	public float m_FadeExp;// = 1.0F;
	public float m_InvFade;// = 1.0F;
	public float m_InvFadeFoam;// = 1.0F;
	public float m_InvFadeDepthFade;// = 1.0F;	
	
	public Vector2 m_ShoreTilingBumpA;
	public Vector2 m_ShoreTilingBumpB;
	
	public float m_UnderwaterCheckOffset = 0.001F;
	public Material m_Water3Material;
	
	private bool m_IsDirty = false;
	
	public void Start() 
	{
		if(m_Water3Material)
			renderer.sharedMaterial = m_Water3Material;
		m_IsDirty = false;
		m_WaterManager = Water3Manager.Instance();
	}
	
	public Mesh GetMesh() {
		if(Application.isPlaying)
			return ((MeshFilter)GetComponent(typeof(MeshFilter))).mesh;
		else
			return ((MeshFilter)GetComponent(typeof(MeshFilter))).sharedMesh;			
	}
	
	public bool IsUnderwater(Camera cam) {
		return cam.transform.position.y + m_UnderwaterCheckOffset < transform.position.y ? true : false;	
	}
	
	// This is called when it's known that the object will be rendered by some
	// camera. We render reflections / refractions and do other updates here.
	
	// Because the script executes in edit mode, reflections for the scene view
	// camera will just work!
	
	public void OnWillRenderObject()
	{
		if(!enabled)
			return;
		
		if(!renderer.sharedMaterial)
			return;
		
		Camera cam = Camera.current;
		if( !cam )
			return;
	
		// Safeguard from recursive water reflections.		
		if( s_InsideWater )
			return;
		s_InsideWater = true;
		
		bool underWater = false;
				
		// Actual water rendering mode depends on both the current setting AND
		// the hardware support. There's no point in rendering refraction textures
		// if they won't be visible in the end.
		m_HardwareWaterSupport = FindHardwareWaterSupport();
		WaterMode mode = GetWaterMode();
		
		Camera reflectionCamera;
		CreateWaterObjects(cam, out reflectionCamera);
		
		// find out the reflection plane: position and normal in world space
		Vector3 pos = transform.position;
		Vector3 normal = transform.up;
		
		// Optionally disable pixel lights for reflection/refraction
		int oldPixelLightCount = QualitySettings.pixelLightCount;
		if( m_DisablePixelLights )
			QualitySettings.pixelLightCount = 0;
					
		UpdateCameraModes(cam, reflectionCamera);
				
		// Render reflection if needed
		if(mode >= WaterMode.FastAndNoRefraction)
		{
			// this should actually be accompanied along with a 
			// special image effect, blurring out the scene etc.
			if(IsUnderwater(cam)) 
			{
				/*
				reflectionCamera.depthTextureMode = DepthTextureMode.None;
				reflectionCamera.renderingPath = RenderingPath.Forward;
						
				reflectionCamera.transform.position = cam.transform.position;
				reflectionCamera.transform.rotation = cam.transform.rotation;
				reflectionCamera.worldToCameraMatrix = cam.worldToCameraMatrix;
				reflectionCamera.targetTexture = m_ReflectionTexture;
				reflectionCamera.cullingMask = ~(1<<4) & m_ReflectLayers.value; // never render water layer
				reflectionCamera.projectionMatrix = cam.projectionMatrix;
								
				reflectionCamera.Render();
				*/
				
				if(Camera.main == cam && !cam.gameObject.GetComponent(typeof(Water3UnderwaterEffect)) ) {
					cam.gameObject.AddComponent(typeof(Water3UnderwaterEffect));	
				}
				
				Water3UnderwaterEffect effect = (Water3UnderwaterEffect)cam.gameObject.GetComponent(typeof(Water3UnderwaterEffect));				
				if(effect) {
					effect.enabled = true;
					effect.m_Water = this;
				}
				
				underWater = true;
			}
			else 
			{
				Water3UnderwaterEffect effect = (Water3UnderwaterEffect)cam.gameObject.GetComponent(typeof(Water3UnderwaterEffect));
				if(effect && effect.enabled) {
					effect.enabled = false;
				}	
				
				if(realtime2DReflection) 
				{
					// we want realtime reflection (need an extra camera render)
					Shader.EnableKeyword("RT_REFLECTION_ON");
					Shader.DisableKeyword("RT_REFLECTION_OFF");							
						
					// Reflect camera around reflection plane
					float d = -Vector3.Dot (normal, pos) - m_ClipPlaneOffset;
					Vector4 reflectionPlane = new Vector4 (normal.x, normal.y, normal.z, d);
				
					Matrix4x4 reflection = Matrix4x4.zero;
					CalculateReflectionMatrix (ref reflection, reflectionPlane);
					Vector3 oldpos = cam.transform.position;
					Vector3 newpos = reflection.MultiplyPoint( oldpos );
					reflectionCamera.worldToCameraMatrix = cam.worldToCameraMatrix * reflection;
				
					// Setup oblique projection matrix so that near plane is our reflection
					// plane. This way we clip everything below/above it for free.
					Vector4 clipPlane = CameraSpacePlane( reflectionCamera, pos, normal, 1.0f );
					Matrix4x4 projection = cam.projectionMatrix;
					CalculateObliqueMatrix (ref projection, clipPlane);
					reflectionCamera.projectionMatrix = projection;
					
					reflectionCamera.depthTextureMode = DepthTextureMode.None;
					reflectionCamera.renderingPath = RenderingPath.Forward;
					
					reflectionCamera.cullingMask = ~(1<<4) & m_ReflectLayers.value; // never render water layer
					reflectionCamera.targetTexture = m_ReflectionTexture;
					GL.SetRevertBackfacing (true);
					reflectionCamera.transform.position = newpos;
					Vector3 euler = cam.transform.eulerAngles;
					reflectionCamera.transform.eulerAngles = new Vector3(-euler.x, euler.y, euler.z);
					reflectionCamera.Render();
					reflectionCamera.transform.position = oldpos;
					GL.SetRevertBackfacing (false);
					renderer.sharedMaterial.SetTexture("_ReflectionTex", m_ReflectionTexture );
				} 
				else
				{
					// we "just" want a static cubemap reflection
					// NOTE: this can actually look better if there is a 
					// lot of displacement going on ...
					
					Shader.EnableKeyword("RT_REFLECTION_OFF");
					Shader.DisableKeyword("RT_REFLECTION_ON");					
				}
			}
		}
		
		// restore pixel light count
		if(m_DisablePixelLights)
			QualitySettings.pixelLightCount = oldPixelLightCount;
		
		if(lightTransform)
			renderer.sharedMaterial.SetVector("_WorldLightDir", lightTransform.forward);
								
		if(!m_DepthTexturesSupported)
			autoEdgeBlend = false;
		
		if(waterDisplacement) {
			Shader.EnableKeyword("WATER_DISPLACEMENT_ON");
			Shader.DisableKeyword("WATER_DISPLACEMENT_OFF"); 
		}
		else { 
			Shader.DisableKeyword("WATER_DISPLACEMENT_ON"); 
			Shader.EnableKeyword("WATER_DISPLACEMENT_OFF"); 
		}
		
		if(autoEdgeBlend) {
			Shader.EnableKeyword("EDGEBLEND_ON"); 
			Shader.DisableKeyword("EDGEBLEND_OFF"); 
		}
		else {
			Shader.DisableKeyword("EDGEBLEND_ON"); 
			Shader.EnableKeyword("EDGEBLEND_OFF"); 
		}	
		
		if(refractionMask) {
			Shader.EnableKeyword("REFRACTION_MASK_ON");
			Shader.DisableKeyword("REFRACTION_MASK_OFF");
		} 
		else {
			Shader.DisableKeyword("REFRACTION_MASK_ON");
			Shader.EnableKeyword("REFRACTION_MASK_OFF");			
		}
		
		if(underWater) 
		{
			// TODO: cleanup underwater implementation
			// we need depth texture support here
			if(cam) 
				cam.depthTextureMode |= DepthTextureMode.Depth;			
			renderer.sharedMaterial.shader.maximumLOD = 50;	
		}
		else if(mode == WaterMode.Indie) 
		{ 			
			renderer.sharedMaterial.shader.maximumLOD = 100;
		} 
		else 
		{ 			
			if(mode ==  WaterMode.Optimized)
				renderer.sharedMaterial.shader.maximumLOD = 400;
			else
				renderer.sharedMaterial.shader.maximumLOD = mode >  WaterMode.FastAndNoRefraction ? 500 : 300;			
		
			if(autoEdgeBlend) 
			{
				// soft water intersection requires depth textures
				if(cam) 
					cam.depthTextureMode |= DepthTextureMode.Depth;
			}
		}
			
		s_InsideWater = false;
	}
	
	
	// Cleanup all the objects we possibly have created
	void OnDisable()
	{
		if( m_ReflectionTexture ) {
			DestroyImmediate( m_ReflectionTexture );
			m_ReflectionTexture = null;
		}
		foreach( DictionaryEntry kvp in m_ReflectionCameras )
        	DestroyImmediate( ((Camera)kvp.Value).gameObject );
        m_ReflectionCameras.Clear();
        
        // reset mesh if displacement happened
        if(m_IsDirty) {
	        Vector3[] vertices = GetMesh().vertices;
	        Vector3[] normals = GetMesh().normals;        
			int i = 0;
	        while (i < vertices.Length) 
	        {
	        	vertices[i] = new Vector3(vertices[i].x, 0.0F, vertices[i].z);
	        	normals[i] = new Vector3(0.0F,1.0F,0.0F);
	            i++;
	        }
	        GetMesh().vertices = vertices;
	        GetMesh().normals = normals;
        }
		
		Shader.DisableKeyword("RT_REFLECTION_OFF");
		Shader.DisableKeyword("RT_REFLECTION_ON");			
		Shader.DisableKeyword("REFRACTION_MASK_ON");
		Shader.DisableKeyword("REFRACTION_MASK_OFF");			
		Shader.DisableKeyword("EDGEBLEND_ON"); 
		Shader.DisableKeyword("EDGEBLEND_OFF"); 	
		Shader.DisableKeyword("WATER_DISPLACEMENT_ON"); 
		Shader.DisableKeyword("WATER_DISPLACEMENT_OFF"); 		
	}
	
	// NOTE:
	// the following 2 functions are for supporting floating objects
	// on the water surface. they just replicate the wave/sine animation
	// that is also being calculated in the water mesh´ vertex shader
	public Vector3 GetNormalAt(Vector3 pos) { return GetNormalAt(pos, 1f); }
	public Vector3 GetNormalAt(Vector3 pos, float scale) 
	{
		Vector3 pointA = GetHeightOffsetAt(pos+new Vector3(-scale,0.0f,0.0f));
		Vector3 pointB = GetHeightOffsetAt(pos+new Vector3(-scale,0.0f,scale));
		Vector3 pointC = GetHeightOffsetAt(pos+new Vector3(0.0f,0.0f,0.0f));
		
		Vector3 baseX = pointA-pointB;
		Vector3 baseY = pointA-pointC;
		Vector3 normal = Vector3.Cross(baseX,baseY);
		normal.Normalize();
		
		return normal;
	}
	
	public Vector3 GetHeightOffsetAt(Vector3 pos) 
	{	
		Vector3 vertex = new Vector3(pos.x,pos.y,pos.z);
		
		// local space vertex
		vertex = transform.InverseTransformPoint(vertex);	
		
		// apply only world positioning so we can patch toegther several water meshes
		float xOfs = transform.position.x/transform.localScale.x; //10.0f;
		float zOfs = transform.position.z/transform.localScale.z;//*10.0f;	
		
		Vector4 displacement4 = Water3Manager.Instance().GetMaterialVector("_Displacement");
		Vector4 displacementXz = Water3Manager.Instance().GetMaterialVector("_DisplacementXz");
		
		vertex.x += xOfs;
		vertex.z += zOfs;		
		
		float tiling = displacement4.x;
		float speed = displacement4.z;

		float tiling2 = displacement4.y;
		float speed2 = displacement4.w;
		
		float valX = Mathf.Sin(tiling * vertex.x + Water3Manager.Instance().m_Timer * speed);
		float valY = Mathf.Sin(tiling2 * vertex.z + Water3Manager.Instance().m_Timer  * speed2);
		
		vertex.y = 0.0F + displacementXz.x/transform.localScale.x * valX + displacementXz.z/transform.localScale.z * valY;
		
		// the displace mesh amount needs to be added, too
		vertex.y += Water3Manager.Instance().GetDisplaceMeshAmountAt(vertex, transform);
		
		vertex.x -= xOfs;
		vertex.z -= zOfs;
		vertex = transform.TransformPoint(vertex); 
				
		return vertex;
	}

	// This just sets up some matrices in the material; for really
	// old cards to make water texture scroll.
	
	void Update()
	{
		if( !renderer )
			return;
			
		Material mat = renderer.sharedMaterial;
		if( !mat )
			return;

		// update time for mesh displacement
		//if(Application.isPlaying) {
			m_IsDirty |= Water3Manager.Instance().DisplaceMesh(GetMesh(), transform);
		//}					
		
		// update material/shader parameters
		
		// OPTIMIZEME: there is not really a big need to set them each frame, 
		// but for the ease of use and realtime edit shader parameters ...
		// let's just leave it for now ...
		
		Vector3 distort = new Vector3(m_DistortParams.x, m_DistortParams.y, m_DistortParams.z);
		mat.SetVector("_DistortParams", distort);	
		mat.SetVector("_InvFadeParemeter", new Vector4(m_InvFade,m_InvFadeFoam,m_InvFadeDepthFade, m_FadeExp));	
		mat.SetVector("_ShoreTiling", new Vector4(m_ShoreTilingBumpA.x,m_ShoreTilingBumpA.y, m_ShoreTilingBumpB.x,m_ShoreTilingBumpB.y));
		mat.SetFloat("_Shininess", m_Shininess);
		mat.SetVector("_FoamWaveParams", new Vector4(m_FoamWaveParams.x,m_FoamWaveParams.y,m_FoamWaveParams.z,0.0F));		
		
		Vector4 waveSpeed = new Vector4( m_WaveSpeedBumpA.x, m_WaveSpeedBumpA.y, m_WaveSpeedBumpB.x, m_WaveSpeedBumpB.y);
		Vector4 waveScale4 = new Vector4(m_WaveScale, m_WaveScale, m_WaveScale * 0.4F, m_WaveScale * 0.45F);
		
		// Time since level load, and do intermediate calculations with doubles
		double t = Time.timeSinceLevelLoad / 20.0;
		Vector4 offsetClamped = new Vector4(
			(float)System.Math.IEEERemainder(waveSpeed.x * waveScale4.x * t, 1.0),
			(float)System.Math.IEEERemainder(waveSpeed.y * waveScale4.y * t, 1.0),
			(float)System.Math.IEEERemainder(waveSpeed.z * waveScale4.z * t, 1.0),
			(float)System.Math.IEEERemainder(waveSpeed.w * waveScale4.w * t, 1.0)
		);
		
		mat.SetVector( "_WaveOffset", offsetClamped );
		mat.SetVector( "_WaveScale4", waveScale4 );
			
		Vector3 waterSize = renderer.bounds.size;		
		Vector3 scale = new Vector3( waterSize.x*waveScale4.x, waterSize.z*waveScale4.y, 1 );
		Matrix4x4 scrollMatrix = Matrix4x4.TRS( new Vector3(offsetClamped.x,offsetClamped.y,0), Quaternion.identity, scale );
		mat.SetMatrix( "_WaveMatrix", scrollMatrix );
				
		scale = new Vector3( waterSize.x*waveScale4.z, waterSize.z*waveScale4.w, 1 );
		scrollMatrix = Matrix4x4.TRS( new Vector3(offsetClamped.z,offsetClamped.w,0), Quaternion.identity, scale );
		mat.SetMatrix( "_WaveMatrix2", scrollMatrix );
	}
	
	private void UpdateCameraModes( Camera src, Camera dest )
	{
		if( dest == null )
			return;
			
		// set water camera to clear the same way as current camera
		dest.clearFlags = src.clearFlags;
		dest.backgroundColor = src.backgroundColor;		
		if( src.clearFlags == CameraClearFlags.Skybox )
		{
			Skybox sky = src.GetComponent(typeof(Skybox)) as Skybox;
			Skybox mysky = dest.GetComponent(typeof(Skybox)) as Skybox;
			if( !sky || !sky.material )
			{
				mysky.enabled = false;
			}
			else
			{
				mysky.enabled = true;
				mysky.material = sky.material;
			}
		}
		
		// update other values to match current camera.
		// even if we are supplying custom camera&projection matrices,
		// some of values are used elsewhere (e.g. skybox uses far plane)
		
		dest.farClipPlane = src.farClipPlane;
		dest.nearClipPlane = src.nearClipPlane;
		dest.orthographic = src.orthographic;
		dest.fieldOfView = src.fieldOfView;
		dest.aspect = src.aspect;
		dest.orthographicSize = src.orthographicSize;
		dest.depthTextureMode = DepthTextureMode.None;
		dest.renderingPath = RenderingPath.Forward; 
	}
	
	// On-demand create any objects we need for water
	private void CreateWaterObjects( Camera currentCamera, out Camera reflectionCamera)
	{
		WaterMode mode = GetWaterMode();
		
		reflectionCamera = null;
		
		if( mode >= WaterMode.FastAndNoRefraction )
		{
			// Reflection render texture
			if( !m_ReflectionTexture || m_OldReflectionTextureSize != m_TextureSize )
			{
				if( m_ReflectionTexture )
					DestroyImmediate( m_ReflectionTexture );
				m_ReflectionTexture = new RenderTexture( m_TextureSize, m_TextureSize, 16 );
				m_ReflectionTexture.name = "__WaterReflection" + GetInstanceID();
				m_ReflectionTexture.isPowerOfTwo = true;
				m_ReflectionTexture.hideFlags = HideFlags.DontSave;
				m_OldReflectionTextureSize = m_TextureSize;
			}
			
			// Camera for reflection
			reflectionCamera = m_ReflectionCameras[currentCamera] as Camera;
			if( !reflectionCamera ) // catch both not-in-dictionary and in-dictionary-but-deleted-GO
			{
				GameObject go = new GameObject( "Water Refl Camera id" + GetInstanceID() + " for " + currentCamera.GetInstanceID(), typeof(Camera), typeof(Skybox) );
				reflectionCamera = go.camera;
				reflectionCamera.enabled = false;
				reflectionCamera.transform.position = transform.position;
				reflectionCamera.transform.rotation = transform.rotation;
				reflectionCamera.gameObject.AddComponent("FlareLayer");
				go.hideFlags = HideFlags.HideAndDontSave;
				m_ReflectionCameras[currentCamera] = reflectionCamera;
			}
		}
	}
	
	private WaterMode GetWaterMode()
	{
		if( m_HardwareWaterSupport < m_WaterMode )
			return m_HardwareWaterSupport;
		else
			return m_WaterMode;
	}
	
	private bool m_DepthTexturesSupported = true;
	private WaterMode FindHardwareWaterSupport()
	{
		if( !SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth) )
			m_DepthTexturesSupported = false;		
		
		if( !SystemInfo.supportsRenderTextures || !renderer )
			return WaterMode.Indie;
		
		Material mat = renderer.sharedMaterial;
		if( !mat )
			return WaterMode.Indie;

		return WaterMode.Everything;
	}
	
	// Extended sign: returns -1, 0 or 1 based on sign of a
	private static float sgn(float a)
	{
        if (a > 0.0f) return 1.0f;
        if (a < 0.0f) return -1.0f;
        return 0.0f;
	}
	
	// Given position/normal of the plane, calculates plane in camera space.
	private Vector4 CameraSpacePlane (Camera cam, Vector3 pos, Vector3 normal, float sideSign)
	{
		Vector3 offsetPos = pos + normal * m_ClipPlaneOffset;
		Matrix4x4 m = cam.worldToCameraMatrix;
		Vector3 cpos = m.MultiplyPoint( offsetPos );
		Vector3 cnormal = m.MultiplyVector( normal ).normalized * sideSign;
		return new Vector4( cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos,cnormal) );
	}
	
	// Adjusts the given projection matrix so that near plane is the given clipPlane
	// clipPlane is given in camera space. See article in Game Programming Gems 5 and
	// http://aras-p.info/texts/obliqueortho.html
	private static void CalculateObliqueMatrix (ref Matrix4x4 projection, Vector4 clipPlane)
	{
		Vector4 q = projection.inverse * new Vector4(
			sgn(clipPlane.x),
			sgn(clipPlane.y),
			1.0f,
			1.0f
		);
		Vector4 c = clipPlane * (2.0F / (Vector4.Dot (clipPlane, q)));
		// third row = clip plane - fourth row
		projection[2] = c.x - projection[3];
		projection[6] = c.y - projection[7];
		projection[10] = c.z - projection[11];
		projection[14] = c.w - projection[15];
	}

	// Calculates reflection matrix around the given plane
	private static void CalculateReflectionMatrix (ref Matrix4x4 reflectionMat, Vector4 plane)
	{
	    reflectionMat.m00 = (1F - 2F*plane[0]*plane[0]);
	    reflectionMat.m01 = (   - 2F*plane[0]*plane[1]);
	    reflectionMat.m02 = (   - 2F*plane[0]*plane[2]);
	    reflectionMat.m03 = (   - 2F*plane[3]*plane[0]);

	    reflectionMat.m10 = (   - 2F*plane[1]*plane[0]);
	    reflectionMat.m11 = (1F - 2F*plane[1]*plane[1]);
	    reflectionMat.m12 = (   - 2F*plane[1]*plane[2]);
	    reflectionMat.m13 = (   - 2F*plane[3]*plane[1]);
	
    	reflectionMat.m20 = (   - 2F*plane[2]*plane[0]);
    	reflectionMat.m21 = (   - 2F*plane[2]*plane[1]);
    	reflectionMat.m22 = (1F - 2F*plane[2]*plane[2]);
    	reflectionMat.m23 = (   - 2F*plane[3]*plane[2]);

    	reflectionMat.m30 = 0F;
    	reflectionMat.m31 = 0F;
    	reflectionMat.m32 = 0F;
    	reflectionMat.m33 = 1F;
	}
}
