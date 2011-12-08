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
    public bool EndNode;
    public float height;
    internal int walkCount;
    public List<Node> nodes = new List<Node>();
    public IEnumerable<Node> Nodes { get { return nodes.Where(a => a != null); } }
    public override void Awake()
    {
        if (!isEditor)
            Active(false);
    }

    public Vector3 GetPos(float offset)
    {
        return pos + rot * Vector3.left * offset;
    }
    
} 