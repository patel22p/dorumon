using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;

public class Path : Bs {
    public IEnumerable<Node> nodes { get { return transform.Cast<Transform>().Select(a => a.GetComponent<Node>()); } }
    public int walkCount;
}
