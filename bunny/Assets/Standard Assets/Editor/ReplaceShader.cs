using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class ReplaceShader : EditorWindow {
	
	public string folderToReplace = "";
	public Shader shaderToReplace;
	public Shader newShader;
	static Material material;
	static Vector2 scrollPosition;

	
	[MenuItem ("Window/Replace Shaders")]
	static void Init () {
		// Get existing open window or if none, make a new one:
        //ReplaceShader window = (ReplaceShader)
            EditorWindow.GetWindow (typeof (ReplaceShader));
	}
	
	/*
	[MenuItem ("Assets/Replace this folder shaders", false, 1011)] static void OpenWindow () {
	    GetWindow<ReplaceShader>(true, "Game Objects using " + material.name, true);
	}
	*/
	
	void OnGUI () {
		GUILayout.Label ("Replace shader from materials", EditorStyles.boldLabel);
		folderToReplace = EditorGUILayout.TextField ("Folder", folderToReplace);
		shaderToReplace = (Shader)EditorGUILayout.ObjectField("Replace this shader",shaderToReplace,typeof(Shader));
		newShader = (Shader)EditorGUILayout.ObjectField("With this shader",newShader,typeof(Shader));
		
		if(GUI.Button(new Rect(Screen.width-120.0f, Screen.height-60.0f, 100.0f, 20.0f), "Replace")) {
			
			string tmpPath = "Assets/" + folderToReplace;
			string searchPattern = "*.mat";
			
			int replacedCount = 0;
			
			DirectoryInfo di = new DirectoryInfo(tmpPath);
			FileInfo[] fi = di.GetFiles(searchPattern, SearchOption.TopDirectoryOnly);
			
			foreach(FileInfo fiTmp in fi) {
				Material mat = (Material)AssetDatabase.LoadAssetAtPath(tmpPath + "/" + fiTmp.Name, typeof(Material));
				if(mat && mat.shader == shaderToReplace) {
					mat.shader = newShader;
					replacedCount++;
				}
			}
			if(fi.Length <= 0) {
				EditorUtility.DisplayDialog("Info", "No assets found in [" + tmpPath + "]", "OK");
			} else {
				EditorUtility.DisplayDialog("Info", "Replaced " + replacedCount + " shaders", "OK");
			}
		}
	}
}
