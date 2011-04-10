using UnityEngine;
using System.Collections;

// General pathfinding class
[System.Serializable]
public class PathFinder
{
    ListedNode[] nodeList;
    ArrayList OpenList = new ArrayList();
    ArrayList ClosedList = new ArrayList();
    ArrayList Path = new ArrayList();

    public ArrayList GetPath() { return this.Path; }

    private ListedNode startNode;
    private ListedNode endNode;
    private ListedNode currentNode;

    private bool started = false;

    // Constructor for pathfinder.
    public PathFinder(ListedNode[] nodeList)
    {
        this.nodeList = nodeList;
    }

    public void Start(Vector3 curPos, Vector3 endPos)
    {
        OpenList.Clear();
        ClosedList.Clear();
        Path.Clear();
        started = true;

        float startDist = 100000.0f;
        float endDist = 100000.0f;

        foreach (ListedNode n in this.nodeList)
        {
            float dist = Vector3.Distance(curPos, n.Position);
            if (dist < startDist)
            {
                startDist = dist;
                startNode = (ListedNode)n;
            }
            dist = Vector3.Distance(endPos, n.Position);
            if (dist < endDist)
            {
                endDist = dist;
                endNode = (ListedNode)n;
            }
        }

        startNode.Scores(startNode.Position, endNode.Position);
        endNode.Scores(startNode.Position, endNode.Position);

        currentNode = startNode;
        OpenList.Add(startNode);
    }

    public bool Update()
    {
        if (started)
        {
            ListedNode bestNode = null;
            float BestF = 10000.0f;

            if (currentNode == endNode)
            {
                started = false;

                while (currentNode != startNode)
                {
                    Path.Add(currentNode);
                    currentNode = currentNode.Parent;
                    ClosedList.Remove(currentNode);
                }
            }

            foreach (ListedNode n in OpenList)
            {
                if (n.F < BestF)
                {
                    BestF = n.F;
                    bestNode = n;
                }
            }

            currentNode = bestNode;

            if (bestNode != null)
            {
                OpenList.Remove(currentNode);
                ClosedList.Add(currentNode);
                AddNeighboursToOpenList(currentNode);
            }
            else Debug.LogError("bestNode should not be null!!!!");


            return true;
        }
        else
        {
            return false;
        }
    }

    public void DebugMe()
    {
        if (started)
        {
            foreach (ListedNode n in ClosedList)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawCube(n.Position, Vector3.one);
            }
            foreach (ListedNode n in OpenList)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawCube(n.Position, Vector3.one);
            }
        }
        else
        {
            foreach (ListedNode n in Path)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawCube(n.Position, Vector3.one);
            }
        }
    }

    public void ShowNodes()
    {
        foreach (ListedNode n in nodeList)
        {
            Gizmos.DrawCube(n.Position, Vector3.one);
        }
    }

    void AddNeighboursToOpenList(ListedNode n)
    {
        foreach (ListedNode ln in n.NeighborNodes)
        {
            if (!OpenList.Contains(ln) && !ClosedList.Contains(ln))
            {
                ln.Parent = currentNode;
                ln.Scores(startNode.Position, endNode.Position);
                OpenList.Add(ln);
            }
        }
    }
}