using System.Linq;
#if UNITY_EDITOR
using UnityEditor;

#endif
using System.Collections;
using UnityEngine;
using System;
using System.Collections.Generic;
using gui = UnityEngine.GUILayout;

public class Node : Bs
{
    public bool StartNode;
    internal int walkCount;
    public List<Node> nodes = new List<Node>();
    public IEnumerable<Node> Nodes { get { return nodes.Where(a => a != null); } }
    public override void Awake()
    {
        Active(false);
    }
#if UNITY_EDITOR

    public override void OnEditorGui()
    {
        if (gui.Button("Link Nodes"))
        {
            Undo.RegisterSceneUndo("rtools");
            Node[] nds = GetNodes();
            foreach (Node a in nds)
                foreach (var node in nds)
                    if (a != node && !a.nodes.Contains(node))
                        a.nodes.Add(node);
        }
        if (gui.Button("UnLink Nodes"))
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
#endif
}