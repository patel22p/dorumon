using System.Linq;
using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using gui = UnityEngine.GUILayout;
using System.Collections.Generic;

public class LevelEditor : Bs
{
    public GameObject NodePrefab;
    public GameObject PathPrefab;
    Transform path;
    public IEnumerable<Path> paths { get { return transform.Cast<Transform>().Select(a => a.GetComponent<Path>()); } }

    public override void OnSceneGUI(SceneView scene, ref bool repaint)
    {
        Event e = Event.current;
        if (e.type == EventType.keyDown && e.keyCode == KeyCode.N)
        {
            //Undo.RegisterSceneUndo("rtools");
            Ray r = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            RaycastHit rhit;
            if (Physics.Raycast(r, out rhit, Mathf.Infinity))
            {
                Event.current.Use();

                var n = ((GameObject)UnityEditor.EditorUtility.InstantiatePrefab(NodePrefab)).GetComponent<Node>();
                n.transform.parent = transform;
                n.pos = rhit.point;
                Node lastNode;
                if (Selection.activeGameObject != null && (lastNode = Selection.activeGameObject.GetComponent<Node>()) != null)
                {
                    n.nodes.Add(lastNode);
                    lastNode.nodes.Add(n);
                }
                else
                {
                    path = ((GameObject)EditorUtility.InstantiatePrefab(PathPrefab)).transform;
                    path.position = n.pos;
                    path.parent = this.transform;
                }
                n.parent = path;
                Selection.activeGameObject = n.gameObject;
            }
        }
    }
   
    public void OnDrawGizmos()
    {
        if (transform.root.name == "LevelEditor")
        {
            foreach (Transform a in transform.GetComponentsInChildren<Transform>())
            {
                var n = a.GetComponent<Node>();
                if (n != null)
                    foreach (var o in n.nodes)
                        if (o != null)
                            Gizmos.DrawLine(n.pos, o.pos);
            }
        }
    }

}
