using UnityEngine;
using System.Collections;
using System;
using System.Threading;

[ExecuteInEditMode]
public class Water3Manager : MonoBehaviour {

	public enum CpuDisplacementModel {
		None = 0,
		NoiseBump = 1,
		FFT = 2,
	};
		
	[HideInInspector]
	public CpuDisplacementModel m_CpuDisplacementModel = CpuDisplacementModel.NoiseBump;	
	[HideInInspector]
	public float m_DisplacementTiling = 0.25F;
	[HideInInspector]
	public float m_NormalsDisplacement = 0.5F;
	[HideInInspector]
	public float m_HeightDisplacement = 3.5F;
	[HideInInspector]
	public float m_SmallWavesSpeed = 0.02F;
	[HideInInspector]
	private Texture2D m_DisplacementHeightMap;
	[HideInInspector]
	private Texture2D m_2ndDisplacementHeightMap;
	[HideInInspector]
	public Material m_SharedWaterMaterial;	
	
	// singleton
	private static Water3Manager s_Instance = null;
	
	// timing
	[HideInInspector]
	public float m_Timer;
	
	public void SetDisplacementHeightMap(Texture2D map, int index) {
		if(!map)
			return;
				
		if(0==index) {
			if(m_DisplacementHeightMap != map) {
				m_DisplacementHeightMap = map;
				FillWithGradiant(m_DisplacementHeightMap);
			}
		} 
		else
		{
			if(m_2ndDisplacementHeightMap != map) {
				m_2ndDisplacementHeightMap = map;
				FillWithGradiant(m_2ndDisplacementHeightMap);	
			}				
		}
	}
	public Texture2D GetDisplacementHeightMap(int index) {
		if(0==index) {
			return m_DisplacementHeightMap;
		} 
		else
		{
			return m_2ndDisplacementHeightMap;
		}		
	}
	
	public static Water3Manager Instance() {
		if(s_Instance==null) {
			s_Instance = FindObjectOfType(typeof (Water3Manager)) as Water3Manager;
		} 
		if(s_Instance==null) {
			GameObject obj = new GameObject("Water3Manager");
			s_Instance = obj.AddComponent(typeof (Water3Manager)) as Water3Manager;
			Material m = (FindObjectOfType(typeof (Water3)) as Water3).m_Water3Material;
			((Water3Manager)obj.GetComponent(typeof (Water3Manager))).m_SharedWaterMaterial = m;
			Debug.Log ("could not locate an Water3Manager object. generated.");		
		}
		 
		return s_Instance;	
	}
	
	public void OnEnable () {
		m_Timer = 0.0F;	
	}
	
	public void Start() {
		SetDisplacementHeightMap( (Texture2D) GetMaterialTexture("_DisplacementHeightMap"), 0);
		SetDisplacementHeightMap( (Texture2D) GetMaterialTexture("_SecondDisplacementHeightMap"), 1);		
	}
	
	public void Update() 
	{
		if(null == m_SharedWaterMaterial)
			return;
		
		// update time for mesh displacement
		if(Application.isPlaying) {
			m_Timer += Time.deltaTime;
		}	
			
		// update time
		m_SharedWaterMaterial.SetFloat("_NoiseTime", m_Timer);
	}
	
	public Color GetMaterialColor(System.String name) {
		return m_SharedWaterMaterial.GetColor(name);	
	}
	public void SetMaterialColor(System.String name, Color color) {
		m_SharedWaterMaterial.SetColor(name, color);	
	}
	public Vector4 GetMaterialVector(System.String name) {
		//if(m_SharedWaterMaterial.HasProperty())
		return m_SharedWaterMaterial.GetVector(name);	
	}
	public void SetMaterialVector(System.String name, Vector4 vector) {
		m_SharedWaterMaterial.SetVector(name, vector);	
	}
	public Texture GetMaterialTexture(System.String theName) {
		return m_SharedWaterMaterial.GetTexture(theName);
	}
	public void SetMaterialTexture(System.String theName, Texture parameter) {
		m_SharedWaterMaterial.SetTexture(theName, parameter);
	}		
		
	public float GetDisplaceMeshAmountAt(Vector3 pos,Transform t) {
		if(m_CpuDisplacementModel == Water3Manager.CpuDisplacementModel.None)
			return 0.0F;
		else
			return DisplaceMeshAmountAt(pos, t);	
	}	
	
	// functions for C# based mesh displacement
	// @TODO: implement proper FFT style displacement
		 
	public void FillWithGradiant(Texture2D tex) 
	{
		for(int i = 0; i < tex.width; i++) {
			for(int j = 0; j < tex.height; j++) {
        		Color displ = m_DisplacementHeightMap.GetPixel(i,j);
        		Color displR = m_DisplacementHeightMap.GetPixel(i-1,j);	
        		Color displU = m_DisplacementHeightMap.GetPixel(i,j-1);							
        		m_DisplacementHeightMap.SetPixel(i,j, new Color((displ.a-displR.a)*0.5F+0.5F,(displ.a-displU.a)*0.5F+0.5F,displ.a,displ.a));
			}	
		}
	}
	
	public bool DisplaceMesh(Mesh m, Transform t) 
	{
		if(m_CpuDisplacementModel == Water3Manager.CpuDisplacementModel.None)
			return false;
		
		if(null == m_DisplacementHeightMap || null == m_2ndDisplacementHeightMap)
			return false;
					
		// apply only world positioning so we can pam_DisplacementHeightMaptch toegther several water meshes
		float xOfs = t.position.x/t.localScale.x; //10.0f;
		float zOfs = t.position.z/t.localScale.z;// *10.0f;
		
		Vector3[] vertices = m.vertices;
		Vector3[] normals =  m.normals;
		
		float timeAdd = m_DisplacementTiling+Time.time*0.1F*m_SmallWavesSpeed;
		float heightDisplace = m_HeightDisplacement/t.localScale.y;
		Vector3 v;
		Color displ;
		Color displ2;
				
        for (int i = 0; i < vertices.Length; i++) 
        {
        	v.x = (vertices[i].x + xOfs)*m_DisplacementTiling+timeAdd;
        	v.z = (vertices[i].z + zOfs)*m_DisplacementTiling+timeAdd;
        	
        	displ = m_DisplacementHeightMap.GetPixelBilinear(v.x,v.z);
        	displ2 = m_2ndDisplacementHeightMap.GetPixelBilinear(v.z,v.x);    
        				        		
        	vertices[i][1] = ((displ.a+displ2.a)*2.0F-2.0F) * heightDisplace;
        	normals[i][0]  = ((displ.r+displ2.r)*2.0F-2.0F) * m_NormalsDisplacement;
        	normals[i][2]  = ((displ.g+displ2.g)*2.0F-2.0F) * m_NormalsDisplacement;    	       	
        }
        
        // apply
        m.vertices = vertices;
        m.normals = normals;
            
        return true;
	}	
		
	private float DisplaceMeshAmountAt(Vector3 pos, Transform t) 
	{
		float returnVal = 0.0F;
		float heightDisplace = m_HeightDisplacement/t.localScale.y;
		float timeAdd = m_DisplacementTiling+Time.time*0.1F*m_SmallWavesSpeed;
		Vector3 v = pos;
        v.x = v.x*m_DisplacementTiling+timeAdd;
        v.z = v.z*m_DisplacementTiling+timeAdd;
        			
		Color displ = m_DisplacementHeightMap.GetPixelBilinear(v.x,v.z);
		Color displ2 = m_2ndDisplacementHeightMap.GetPixelBilinear(v.z,v.x);
        		
		returnVal = (displ.a+displ2.a*2.0F-1.0F) * heightDisplace;
		
		return returnVal;		
	}	
	
    // Ensure that the instance is destroyed when the game is stopped in the editor.
    void OnApplicationQuit() {
        s_Instance = null;
    }	
}
