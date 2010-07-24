using UnityEngine;
using System.Collections;

public class Generate2DReflection : MonoBehaviour
{
	public int textureSize = 256;
	public LayerMask mask = 1 << 0;
	private Camera cam;
	public RenderTexture rtex;
	private Material[] reflectingMaterials = null;

	private bool destroyTexture = false;

	void Start()
	{
		Component[] comps = transform.GetComponentsInChildren(typeof(Renderer));
		int reflectMats = 0;
		foreach(Renderer r in comps)
			if(r.material.HasProperty("_2DReflection"))
				reflectMats++;
		reflectingMaterials = new Material[reflectMats];

		int matsIndex = 0;
		foreach(Renderer r in comps)
			if(r.material.HasProperty("_2DReflection"))
			{
				reflectingMaterials[matsIndex] = r.material;
				matsIndex++;
			}
		if(rtex)
			foreach(Material m in reflectingMaterials)
				m.SetTexture("_2DReflection", rtex);
		
		if(QualitySettings.currentLevel > QualityLevel.Beautiful)
			UpdateReflection();
	}

	void LateUpdate()
	{
		UpdateReflection();
	}
	
	void UpdateReflection()
	{
		if(!rtex)
		{
			rtex = new RenderTexture(textureSize, textureSize, 16);
			rtex.isPowerOfTwo = true;
			foreach(Material m in reflectingMaterials)
				m.SetTexture("_2DReflection", rtex);
			destroyTexture = true;
		}

		if(!cam)
		{
			GameObject go = new GameObject("CubemapCamera", typeof(Camera));
			go.transform.position = transform.position + Vector3.up * 0.5f;
			go.transform.LookAt(Camera.main.transform);
			cam = go.camera;
			cam.fieldOfView = 160;
			cam.nearClipPlane = 0.05f;
			cam.farClipPlane = 150f;
			cam.enabled = false;
			cam.cullingMask = mask;
			cam.targetTexture = rtex;
		}

		cam.transform.position = transform.position + Vector3.up * 0.5f;
		cam.transform.LookAt(Camera.main.transform);
		cam.Render();
		foreach(Material m in reflectingMaterials)
			m.SetMatrix("_2DReflectionMatrix", cam.transform.worldToLocalMatrix);
	}

	void OnDisable()
	{
		DestroyImmediate(cam);
		if(destroyTexture)
			DestroyImmediate(rtex);
	}
}
