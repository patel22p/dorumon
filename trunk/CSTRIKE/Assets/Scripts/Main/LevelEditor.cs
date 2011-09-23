using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections;

using System;
using gui = UnityEngine.GUILayout;
using System.Collections.Generic;


public class LevelEditor : Bs
{
    public string Prefix;
    public GameObject NodePrefab;
    public GameObject PathPrefab;
    Transform path;
    public IEnumerable<Path> paths { get { return transform.Cast<Transform>().Select(a => a.GetComponent<Path>()); } }
#if UNITY_EDITOR
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

                Debug.Log("Hit");
                var n = ((GameObject)Instantiate(NodePrefab)).GetComponent<Node>();
                n.name = Prefix + "Node";
                n.transform.parent = transform;
                n.pos = rhit.point;
                Node lastNode;
                if (Selection.activeGameObject != null && (lastNode = Selection.activeGameObject.GetComponent<Node>()) != null)
                {                    
                    lastNode.nodes.Add(n);
                    lastNode.transform.LookAt(n.transform);
                    n.rot = lastNode.rot;
                    path = lastNode.parent;
                }
                else
                {
                    path = ((GameObject)Instantiate(PathPrefab)).transform;
                    path.name = Prefix + "Path";
                    path.GetComponent<Path>().StartNode = n;
                    path.position = n.pos;
                    n.StartNode = true;
                    path.parent = this.transform;
                }
                n.parent = path;
                Selection.activeGameObject = n.gameObject;
            }
        }
    }
    public override void OnEditorGui()
    {
        if (gui.Button("Link"))
        {
            Undo.RegisterSceneUndo("rtools");
            var a = Selection.activeGameObject.GetComponent<Node>();
            var b = Selection.gameObjects.Select(c => c.GetComponent<Node>()).FirstOrDefault(c => c != a);
            a.nodes.Add(b);
            a.transform.LookAt(b.transform);
        }
        if (gui.Button("Unlink"))
        {
            Undo.RegisterSceneUndo("rtools");
            var a = Selection.gameObjects[0].GetComponent<Node>();
            var b = Selection.gameObjects[1].GetComponent<Node>();
            a.nodes.Remove(b);
            b.nodes.Remove(a);
        }
        base.OnEditorGui();
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
                        {
                            //Gizmos.color = Color.white;
                            Gizmos.DrawLine(n.pos, o.pos);
                            //Gizmos.color = Color.red;
                            //Gizmos.DrawRay(o.pos, Quaternion.Euler(0, 35, 0) * (o.pos - n.pos).normalized * .3f);
                            //Gizmos.DrawRay(o.pos, Quaternion.Euler(0, -35, 0) * (o.pos - n.pos).normalized * .3f);
                        }

            }
        }
    }
#endif
}
