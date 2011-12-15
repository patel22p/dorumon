using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;

public class NPath : Bs {
    public IEnumerable<Node> nodes { get { return transform.Cast<Transform>().Select(a => a.GetComponent<Node>()); } }
    public Node StartNode;
    public Team team;
    public PlType[] plTypes = new [] { PlType.Bot, PlType.Fatty, PlType.Monster };    
}
