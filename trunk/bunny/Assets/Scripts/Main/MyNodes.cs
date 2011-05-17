using System.Linq;
using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using GUI = UnityEngine.GUILayout;
public class MyNodes : bs{

    public bool ShowPaths = true;
    public GameObject NodePrefab;

    public override void OnEditorGui()
    {
        GUI.Label("Use N to Create ");
        
    }
    public override void Init()
    {
        foreach (Transform a in transform)
            a.gameObject.layer = LayerMask.NameToLayer("Node");
        base.Init();
    }
    Node lastNode;
    public override void OnSceneGUI()
    {        
        Event e = Event.current;
        if (e.type == EventType.keyDown && e.keyCode == KeyCode.N)
        {
            Undo.RegisterSceneUndo("rtools");
            Ray r = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            RaycastHit rhit;
            if (Physics.Raycast(r, out rhit, Mathf.Infinity))
            {
                Event.current.Use();

                var g = (GameObject)(GameObject)UnityEditor.EditorUtility.InstantiatePrefab(NodePrefab);
                    //Instantiate(NodePrefab, rhit.point, Quaternion.identity);
                g.transform.parent = transform;
                Node n = g.GetComponent<Node>();
                n.pos = rhit.point;

                if (lastNode != null)
                {
                    n.nodes.Add(lastNode);
                    lastNode.nodes.Add(n);
                }
                lastNode = n;
            }
        }

        base.OnSceneGUI();
    }
    void OnDrawGizmos()
    {
        //var ac = Selection.activeGameObject;
        //if (ac != null && ShowPaths && (ac == this.gameObject || ac.GetComponent<Node>() != null))
        if(ShowPaths)
        {
            foreach (Transform a in transform)
            {
                var n = a.GetComponent<Node>();
                foreach (var o in n.nodes)
                    if (o != null)
                        Gizmos.DrawLine(n.pos, o.pos);
            }
        }
        //Debug.Log("test");
    }
}
