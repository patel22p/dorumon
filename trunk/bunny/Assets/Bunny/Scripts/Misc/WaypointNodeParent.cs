using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class WaypointNodeParent : MonoBehaviour
{
    // Debugging
    public bool ShowNodes = true;
    public bool ShowLinks = true;
    public Color GizmoColor = Color.red;
    public Color LinkColor = Color.red;

    public List<WaypointNode> WaypointList = new List<WaypointNode>();

    // Return a cloned list of the waypoitns for pathfinder aka monster. Which will be destroyed after usage
    public ListedNode[] GetWaypoints { get { return (ListedNode[])myWaypoints.Clone(); } }
    private ListedNode[] myWaypoints;

    public bool UNIQUE = false;

    void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            if (WaypointList.Count > 0 && ShowNodes)
            {
                foreach (WaypointNode n in WaypointList)
                {
                    Gizmos.color = GizmoColor;
                    Gizmos.DrawCube(n.Position, Vector3.one * 0.5f);
                    if (ShowLinks && n.NeighborNodes.GetLength(0) > 0)
                    {
                        foreach (WaypointLink l in n.NeighborNodes)
                        {
                            if (l.from == n.Position)
                            {
                                Gizmos.color = LinkColor;
                                Gizmos.DrawLine(l.from, l.to);
                            }
                        }
                    }
                }
            }
        }
        else
        {
            if (myWaypoints.Length > 0 && ShowNodes)
            {
                foreach (ListedNode ln in myWaypoints)
                {
                    Gizmos.color = GizmoColor;
                    Gizmos.DrawCube(ln.Position, Vector3.one * 0.5f);

                    foreach (ListedNode lnn in ln.NeighborNodes)
                    {
                        Gizmos.color = LinkColor;
                        Gizmos.DrawLine(ln.Position, lnn.Position);
                    }
                }
            }
        }
    }

    // Used in editor, obsolete i guess.
    public WaypointNode GetClosestNode(Vector3 pos)
    {
        /// <summary>
        /// ASDASDADASD
        /// </summary>
        WaypointNode closest=null;
        float bestDist=10000.0f;
        float dist;
        foreach (WaypointNode n in WaypointList)
        {
            dist = Vector3.Distance(pos, n.Position);
            if (dist < bestDist)
            {
                bestDist = dist;
                closest = n;
            }
        }
        return closest;
    }

    // Used in editor
    public WaypointNode GetClosestNode(Vector3 pos, float snapDistance)
    {
        WaypointNode closest = null;
        float bestDist = 10000.0f;
        float dist;
        foreach (WaypointNode n in WaypointList)
        {
            dist = Vector3.Distance(pos, n.Position);
            if (dist < bestDist && dist < snapDistance)
            {
                bestDist = dist;
                closest = n;
            }
        }
        return closest;
    }

    // Get listednode with position value
    private ListedNode getListedNode(Vector3 pos)
    {
        foreach (ListedNode ln in myWaypoints)
        {
            if (ln.Position == pos)
                return ln;
        }
        return null;
    }

    /// <summary>
    /// Return some random point position in the world nodes
    /// </summary>
    /// <returns></returns>
    public Vector3 GetRandomNode()
    {
        return myWaypoints[UnityEngine.Random.Range(0, myWaypoints.Length)].Position;
    }

    // We need to convert the grid we have, into a referenced grid to nodes without links.
    void Start()
    {
        int i = 0;
        // Set the nodes in a neat array
        myWaypoints = new ListedNode[WaypointList.Count];
        foreach (WaypointNode n in WaypointList)
        {
            myWaypoints[i] = (ListedNode)n;
            i++;
        }

        // Set the neighbors in the neat array nodes
        for (i = 0; i < WaypointList.Count; i++)
        {
            // ITerate through the neighbornodes
            for (int x = 0; x < WaypointList[i].NeighborNodes.Length; x++)
            {
                ListedNode listedNode = getListedNode(WaypointList[i].NeighborNodes[x].from);
                if (myWaypoints[i].Position != listedNode.Position)
                {
                    myWaypoints[i].NeighborNodes[x] = listedNode;
                    continue;
                }
                listedNode = getListedNode(WaypointList[i].NeighborNodes[x].to);
                if (myWaypoints[i].Position != listedNode.Position)
                {
                    myWaypoints[i].NeighborNodes[x] = listedNode;
                    continue;
                }
                Debug.LogWarning("ERROR, NO MIRRION DORRARS FOR U!! FUCK U!!");
            }
        }
    }
}

// Node to store data
[System.Serializable]
public class WaypointNode
{
    public Vector3 Position;
    public WaypointLink[] NeighborNodes = new WaypointLink[0];

    public bool AddLink(WaypointLink link)
    {
        bool contains = false;
        foreach (WaypointLink l in NeighborNodes)
        {
            if (l.Equals(link))
                contains = true;
        }
        if (!contains)
        {
            WaypointLink[] tempList = new WaypointLink[NeighborNodes.Length + 1];
            tempList[NeighborNodes.Length] = link;
            for (int i = 0; i < NeighborNodes.Length; i++)
            {
                tempList[i] = NeighborNodes[i];
            }
            NeighborNodes = tempList;
            return true;
        }
        else
            return false;
    }

    public void RemoveLink(WaypointLink link)
    {
        bool contains = false;
        int i = 0;
        foreach (WaypointLink l in NeighborNodes)
        {
            if (l.Equals(link))
                contains = true;
        }

        if (contains)
        {
            WaypointLink[] tempList = new WaypointLink[NeighborNodes.Length - 1];
            if (NeighborNodes.Length > 1)// The length is bigger than one, so we have to make a new list.
            {
                foreach (WaypointLink l in NeighborNodes)
                {
                    if (!l.Equals(link))
                    {
                        tempList[i] = l;
                        i++;
                    }
                }
                NeighborNodes = tempList;

            }
            else
            {
                NeighborNodes = new WaypointLink[0];
            }
        }
    }

    public bool Unlink(WaypointLink link)
    {
        int i = 0;
        bool contains = false;
        foreach (WaypointLink l in NeighborNodes)
        {
            if (l.Equals(link))
                contains = true;
        }
        if (contains)
        {
            WaypointLink[] tempList = new WaypointLink[NeighborNodes.Length - 1];
            if (NeighborNodes.Length > 1)// The length is bigger than one, so we have to make a new list.
            {
                foreach (WaypointLink l in NeighborNodes)
                {
                    if (!l.Equals(link))
                    {
                        tempList[i] = l;
                        i++;
                    }
                }
                NeighborNodes = tempList;

            }
            else
            {
                NeighborNodes = new WaypointLink[0];
            }
            return true;
        }
        else
            return false;
    }
}

[System.Serializable]
public class WaypointLink
{
    public Vector3 from;
    public Vector3 to;

    public WaypointLink(Vector3 From, Vector3 To)
    {
        this.from = From;
        this.to = To;
    }

    // Makes comparing easier.
    public WaypointLink Flipped
    {
        get
        {
            WaypointLink newLink = new WaypointLink(this.to, this.from);
            return newLink;
        }
    }

    public override bool Equals(object obj)
    {
        if (obj == null)
            return false;

        WaypointLink link = (WaypointLink)obj;
        if ((object)obj == null)
            return false;

        if (obj.GetType() != typeof(WaypointLink))
            return false;

        if ((from == link.from && to == link.to) || (from == (link.Flipped).from && to == (link.Flipped).to))
        {
            return true;
        }

        return base.Equals(obj);
    }
}

// Listed node is used only in the pathfinding.. converted from nodes to listendnodes.. listednodes hold key value for pathfinding algorithm F, G, H values and so on.
public class ListedNode
{
    public Vector3 Position;
    public ListedNode[] NeighborNodes;
    public ListedNode Parent;

    public float F;     // Combined score of H + G
    public float H;     // Distance from start to this node
    public float G;     // Distance from this node to end

    public void Scores(Vector3 start, Vector3 end)
    {
        H = Vector3.Distance(this.Position, start);
        G = Vector3.Distance(this.Position, end);
        F = H + G;
    }

    // Conversion from Node to ListedNode
    public static implicit operator ListedNode(WaypointNode n)
    {
        ListedNode ln = new ListedNode();
        ln.Position = n.Position;
        ln.NeighborNodes = new ListedNode[n.NeighborNodes.Length];
        return ln;
    }
}