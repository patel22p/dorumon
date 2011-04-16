using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class WaypointNodeParent : MonoBehaviour
{
    public bool ShowNodes = true;
    public bool ShowLinks = true;
    public Color GizmoColor = Color.red;
    public Color LinkColor = Color.red;
    public List<WaypointNode> WaypointList = new List<WaypointNode>();
    public ListedNode[] GetWaypoints { get { return (ListedNode[])myWaypoints.Clone(); } }
    private ListedNode[] myWaypoints;

    void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            if (WaypointList.Count > 0 && ShowNodes)
                foreach (WaypointNode n in WaypointList)
                {
                    Gizmos.color = GizmoColor;
                    Gizmos.DrawCube(n.Position, Vector3.one * 0.5f);
                    if (ShowLinks && n.NeighborNodes.GetLength(0) > 0)
                        foreach (WaypointLink l in n.NeighborNodes)
                            if (l.from == n.Position)
                            {
                                Gizmos.color = LinkColor;
                                Gizmos.DrawLine(l.from, l.to);
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

    public WaypointNode GetClosestNode(Vector3 pos)
    {
        WaypointNode closest = null;
        float bestDist = 10000.0f;
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

    private ListedNode getListedNode(Vector3 pos)
    {
        foreach (ListedNode ln in myWaypoints)
            if (ln.Position == pos)
                return ln;
        return null;
    }

    public Vector3 GetRandomNode()
    {
        return myWaypoints[UnityEngine.Random.Range(0, myWaypoints.Length)].Position;
    }

    void Start()
    {
        int i = 0;
        myWaypoints = new ListedNode[WaypointList.Count];
        foreach (WaypointNode n in WaypointList)
        {
            myWaypoints[i] = (ListedNode)n;
            i++;
        }

        for (i = 0; i < WaypointList.Count; i++)
        {
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

[System.Serializable]
public class WaypointNode
{
    public Vector3 Position;
    public WaypointLink[] NeighborNodes = new WaypointLink[0];

    public bool AddLink(WaypointLink link)
    {
        bool contains = false;
        foreach (WaypointLink l in NeighborNodes)
            if (l.Equals(link))
                contains = true;
        if (!contains)
        {
            WaypointLink[] tempList = new WaypointLink[NeighborNodes.Length + 1];
            tempList[NeighborNodes.Length] = link;
            for (int i = 0; i < NeighborNodes.Length; i++)
                tempList[i] = NeighborNodes[i];
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
            if (NeighborNodes.Length > 1)
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
            if (NeighborNodes.Length > 1)
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
    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}

public class ListedNode
{
    public Vector3 Position;
    public ListedNode[] NeighborNodes;
    public ListedNode Parent;

    public float F; public float H; public float G;
    public void Scores(Vector3 start, Vector3 end)
    {
        H = Vector3.Distance(this.Position, start);
        G = Vector3.Distance(this.Position, end);
        F = H + G;
    }

    public static implicit operator ListedNode(WaypointNode n)
    {
        ListedNode ln = new ListedNode();
        ln.Position = n.Position;
        ln.NeighborNodes = new ListedNode[n.NeighborNodes.Length];
        return ln;
    }
}