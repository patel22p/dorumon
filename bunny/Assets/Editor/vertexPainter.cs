//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
//Vertex Painter by Andrew Grant = reissgrant on Unity forums
//This is released free to use or modify.
//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class vertexPainter : EditorWindow {
	
	static bool toggleShader = false;	
	static bool paintVertexColors = false;
	static bool applyRandom = false;
	static bool applySolidColor = false;
	static bool randomizeColors = false;
	static bool materialChanged = false;
	static bool blendColors = false;
	
	static bool randomRed = true; 
	static bool randomBlue = true;
	static bool randomGreen = true;
	static bool randomAlpha = false;
	
	static float absoluteRed = 0f;
	static float absoluteGreen = 0f;
	static float absoluteBlue = 0f;
	static float absoluteAlpha = 1f;
	 
	static float radius = 1f;
	static float strength = .1f;
	static Color vertexColor = new Color(0f,0f,0f,1f);
	static Material[] startMaterials;
	static GameObject selectedObject;
	
	static Shader vertexShader;
	static Material vertexMaterial;

	[MenuItem ("Vertex Painter/Vertex Painter v.01")]
	
	static void Init () {
		
 		vertexPainter myWindow = (vertexPainter)EditorWindow.GetWindow (typeof (vertexPainter)); 
		myWindow.autoRepaintOnSceneChange  = true;
		
	}

	void OnGUI () {
		
		EditorGUIUtility.LookLikeControls(140f,90f);
		
		GUILayout.BeginVertical ("box");
		EditorGUILayout.BeginHorizontal ();
		EditorGUILayout.PrefixLabel("Paint Vertex Colors");
		paintVertexColors = EditorGUILayout.Toggle (paintVertexColors);
		EditorGUILayout.EndHorizontal ();
		GUILayout.EndVertical();
		
		if (paintVertexColors){
			
			EditorGUIUtility.LookLikeControls(250f,90f);
			GUILayout.BeginVertical ("box");
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.PrefixLabel("Press CTRL / CMD to Paint");
			EditorGUILayout.EndHorizontal ();
			GUILayout.EndVertical();
			
			
			EditorGUIUtility.LookLikeControls(140f,90f);
			GUILayout.BeginVertical ("box");
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.PrefixLabel("See Vertex Colors");
			toggleShader = EditorGUILayout.Toggle (toggleShader);
			EditorGUILayout.EndHorizontal ();
			GUILayout.EndVertical();
			
			GUILayout.BeginVertical ("box");

			GUILayout.Label("Paint Color");
			vertexColor = EditorGUILayout.ColorField(vertexColor);
			
			EditorGUIUtility.LookLikeControls(190f,50f);
			
			GUILayout.Label("Paint Radius");
			radius = EditorGUILayout.Slider (radius, .1f, 5f);

			GUILayout.Label("Paint Strength");
			strength = EditorGUILayout.Slider (strength, .01f, 1f);
						
			GUILayout.EndVertical();   
			
		EditorGUIUtility.LookLikeControls(140f,90f);
				
			GUILayout.BeginVertical ("box");
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.PrefixLabel("Randomize Colors");
			randomizeColors = EditorGUILayout.Toggle (randomizeColors);
			EditorGUILayout.EndHorizontal ();
			GUILayout.EndVertical();
				
			if(randomizeColors){
				
				
				GUILayout.BeginVertical ("box");
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.PrefixLabel("Blend Random Colors?");
				blendColors = EditorGUILayout.Toggle (blendColors);
				EditorGUILayout.EndHorizontal ();
				GUILayout.EndVertical();
				
				EditorGUIUtility.LookLikeControls(90f,30f);
				GUI.backgroundColor = Color.red;
				EditorGUILayout.BeginHorizontal ("box");	
				EditorGUILayout.PrefixLabel("Red");
				GUI.backgroundColor = Color.white;
				randomRed = EditorGUILayout.Toggle (randomRed);
				if(!randomRed){
					absoluteRed = EditorGUILayout.Slider (absoluteRed, 0f, 1f);
				}
				EditorGUILayout.EndHorizontal ();
				
				GUI.backgroundColor = Color.green;
				EditorGUILayout.BeginHorizontal ("box");	
				EditorGUILayout.PrefixLabel("Green");
				GUI.backgroundColor = Color.white;
				randomGreen = EditorGUILayout.Toggle (randomGreen);
				if(!randomGreen){
					absoluteGreen = EditorGUILayout.Slider (absoluteGreen, 0f, 1f);
				}
				EditorGUILayout.EndHorizontal ();
				
				GUI.backgroundColor = Color.blue;
				EditorGUILayout.BeginHorizontal ("box");	
				EditorGUILayout.PrefixLabel("Blue");
				GUI.backgroundColor = Color.white;
				randomBlue = EditorGUILayout.Toggle (randomBlue);
				if(!randomBlue){
					absoluteBlue = EditorGUILayout.Slider (absoluteBlue, 0f, 1f);
				}
				EditorGUILayout.EndHorizontal ();
				
				GUI.backgroundColor = Color.grey;
				EditorGUILayout.BeginHorizontal ("box");	
				EditorGUILayout.PrefixLabel("Alpha");
				GUI.backgroundColor = Color.white;
				randomAlpha = EditorGUILayout.Toggle (randomAlpha);
				if(!randomAlpha){
					absoluteAlpha = EditorGUILayout.Slider (absoluteAlpha, 0f, 1f);
				}
				EditorGUILayout.EndHorizontal ();
				
				GUI.backgroundColor = Color.white;
			
				if(GUILayout.Button ("Click to Apply Colors to All Vertices!", EditorStyles.miniButton)){
					
					applyRandom  = true;
					
				}	 
			}
		}
	}

	
	[DrawGizmo (GizmoType.SelectedOrChild | GizmoType.NotSelected)]
	
	static void RenderVisibleVertices (Transform obj, GizmoType gizmoType){
		
		if(Selection.activeGameObject && paintVertexColors){
			
			selectedObject = Selection.activeGameObject;

			if(paintVertexColors && selectedObject.GetComponent("MeshFilter")!=null){		
				
				if(startMaterials==null){
					
					startMaterials = selectedObject.renderer.sharedMaterials;
					
				}
				
				MeshFilter selectedMeshFilter = (MeshFilter)selectedObject.GetComponent("MeshFilter");
				
				if (selectedMeshFilter){
					
					Matrix4x4 selectedObjectMatrix = Matrix4x4.TRS(
					selectedObject.transform.position,
					selectedObject.transform.rotation,
					selectedObject.transform.lossyScale);
					
					Mesh selectedMesh = selectedMeshFilter.sharedMesh;
					Vector3[] vertices = selectedMesh.vertices;
					Color[] colors = new Color[vertices.Length];
					
					if(!selectedMeshFilter.GetComponent("MeshCollider")){
						
						MeshCollider newCollider =  (MeshCollider)selectedObject.AddComponent("MeshCollider");
						newCollider.sharedMesh = selectedMesh;
						
					} 
					
 					if(selectedMesh.colors.Length < 2){
											
						selectedMesh.colors = colors;
						
					}
					
					Event e = Event.current;
					RaycastHit hit;
					Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);				
					
					if (Physics.Raycast (ray, out hit)){
						
						if (selectedMeshFilter){ 
						
							Vector3 relativePoint = hit.point;
							Handles.color = vertexColor;
							Handles.DrawWireDisc(relativePoint,hit.normal, radius);
							
							if (e.control | e.command) {	
								
								colors = selectedMesh.colors;
										
								float sqrRadius = radius * radius;
								
								for (int i = 0	; i < vertices.Length	;	i++){
									
									vertices[i] = selectedObjectMatrix.MultiplyPoint(vertices[i]);
									float sqrMagnitude = (vertices[i] - relativePoint).sqrMagnitude;
									
									if (sqrMagnitude < sqrRadius){
										
										float distance = Mathf.Sqrt(sqrMagnitude);
										
										float falloff = Mathf.Clamp01 (Mathf.Pow (360.0f, -Mathf.Pow (distance / radius, 2.5f) - 0.1f));
										
										float colorAdd = falloff * strength;

										colors[i] = Color.Lerp(colors[i],vertexColor,colorAdd);

										selectedMesh.colors = colors;
									} 
								} 
							}
						} 
					}
					
						if(selectedMesh && (applyRandom | applySolidColor)){
							
							vertices =  selectedMesh.vertices;
							
							for (int i = 0	; i < vertices.Length	;	i++){
								
								if(applyRandom){ 
								
									float R,G,B,A;
									R = 0f;
									G = 0f;
									B = 0f;
									A = 0f;
									
									if(blendColors){
										
										if(randomRed){		R = UnityEngine.Random.Range(0f,1f); } else { R = absoluteRed;}
										if(randomGreen){	G = UnityEngine.Random.Range(0f,1f); } else { G = absoluteGreen;}
										if(randomBlue){		B = UnityEngine.Random.Range(0f,1f); } else { B = absoluteBlue;}
										if(randomAlpha){		A = UnityEngine.Random.Range(0f,1f); } else { A = absoluteAlpha;}
										
									} else {
										
										int numRandoms = 0;
										int index = 0;
										
										if(randomRed){		numRandoms+=1; } else { R = absoluteRed; 	}
										if(randomGreen){	numRandoms+=1; } else { G = absoluteGreen; }
										if(randomBlue){		numRandoms+=1; } else { B = absoluteBlue; 	}
										if(randomAlpha){		numRandoms+=1; } else { A = absoluteAlpha;	 }

							
										int randomColor = Mathf.Abs(UnityEngine.Random.Range(0,numRandoms));
										
										int[] randomArray = new int[] {0,0,0,0};
										
										randomArray[randomColor] = 1;
										
										if(randomRed){		R = randomArray[index]; index+=1;}
										if(randomGreen){	G = randomArray[index];index+=1;}
										if(randomBlue){		B = randomArray[index];index+=1;}
										if(randomAlpha){		A = randomArray[index];index+=1;}
										
									}
									
									colors[i] = new Color(R,G,B,A);
						
								}
						
								if(applySolidColor){
						
									colors[i] = vertexColor;
						
									} 
								}
					
							if(applyRandom | applySolidColor){
					
								selectedMesh.colors = colors;
					
								applyRandom = false;
					
								applySolidColor =false;
					
								} 
							}	
							
							
						if(vertexShader == null){
							
							vertexShader = Shader.Find ("Custom/VertexColorsOnly");
			
							vertexMaterial = new Material(vertexShader);
							
						}
						
						if(toggleShader && Selection.activeGameObject){
							
							if(selectedObject.renderer.sharedMaterial != vertexMaterial){
								
								selectedObject.renderer.sharedMaterial = vertexMaterial;
								
								materialChanged = true;
							
							}
						
						}else if(Selection.activeGameObject && selectedObject.renderer.sharedMaterial.shader == vertexShader){
							
							selectedObject.renderer.sharedMaterials = startMaterials;
					
						}
						
				} // if (selectedMeshFilter)
			}// if(paintVertexColors)
		} else if(selectedObject != null){
			
			if(materialChanged){
				
				selectedObject.renderer.sharedMaterials = startMaterials;
				
			}
			
			materialChanged = false;
			startMaterials = null;
			selectedObject = null;
		}
			
	HandleUtility.Repaint(); 
		
	} // render gizmo	 
	
	void OnDestroy(){
		
					
			if(materialChanged){
				
				selectedObject.renderer.sharedMaterials = startMaterials;
				
			}
		
		toggleShader = false;	
		paintVertexColors = false;
		applyRandom = false;
		applySolidColor = false;
		randomizeColors = false;
		materialChanged = false;
		startMaterials = null;
	}
	
} // class definition

