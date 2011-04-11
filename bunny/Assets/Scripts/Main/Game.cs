using System.Linq;
using UnityEngine;
using System.Collections;
using gui = UnityEditor.EditorGUILayout;
using GUI = UnityEngine.GUILayout;
using UnityEditor;
public class Game : bs {

	void Start () {
	
	}
	
	void Update () {
        if (Input.GetKeyDown(KeyCode.Tab))
            Screen.lockCursor = !Screen.lockCursor;
	}
    public override void OnEditorGui()
    {
        Undo.RegisterSceneUndo("rtools");
        if (GUI.Button("Desttroy Mono"))
        {
            var ar = GameObject.FindObjectsOfType(typeof(GameObject)).ToArray();
            Debug.Log(ar.Count());
            int c=0;
            
            foreach (GameObject a in ar)
            {
                //Debug.Log(a.name);
                if (a.GetComponent<Base>() == null)
                {

                    foreach (var b in a.GetComponents<Behaviour>())
                    {
                        c++;
                        DestroyImmediate(b);
                    }
                }
            }
            Debug.Log(c );
        }
        base.OnEditorGui();
    }
}
