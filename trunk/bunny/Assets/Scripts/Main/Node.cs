using System.Linq;
using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using System.Collections.Generic;
using GUI = UnityEngine.GUILayout;
public class Node : bs
{
    public List<Node> nodes = new List<Node>();
    public bool Jump;
    public bool Land;
    public float JumpPower = 7f;
    public void Start()
    {
        gameObject.active = false;
    }
    public override void OnEditorGui()
    {
        if (GUI.Button("Link Nodes"))
        {
            Undo.RegisterSceneUndo("rtools");
            Node[] nds = GetNodes();
            foreach (Node a in nds)
                foreach (var node in nds)
                    if (a != node && !a.nodes.Contains(node))
                        a.nodes.Add(node);            
        }
        if (GUI.Button("UnLink Nodes"))
        {
            Undo.RegisterSceneUndo("rtools");
            Node[] nds = GetNodes();
            foreach (Node a in nds)
                foreach (var node in nds)
                    a.nodes.Remove(node);
        }
    }

    private static Node[] GetNodes()
    {
        Node[] nds = Selection.gameObjects.Select(a => a.GetComponent<Node>()).Where(b => b != null).ToArray();
        return nds;
    }
}
