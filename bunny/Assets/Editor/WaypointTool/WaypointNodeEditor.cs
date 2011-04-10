using UnityEditor;
using UnityEngine;
using System.Collections;

public enum NodeEditorState { NONE = 0, ADD = 1, DELETENODE = 2, LINK = 3, UNLINK = 4 }

[CustomEditor(typeof(WaypointNodeParent))]
public class WaypointNodeEditor : Editor
{
    // Editor variables
    NodeEditorState editState = NodeEditorState.NONE;
    string[] editNames = { "[1] Stop", "[2] Add Node", "[3] Delete Node", "[4] Link", "[5] Unlink" };
    WaypointNodeParent dummy;

    // Reference
    WaypointNodeParent currentWaypoints;

    // Linking variables
    WaypointNode nFrom;
    WaypointNode nTo;

    Vector3 from = Vector3.zero;
    Vector3 to = Vector3.zero;

    [MenuItem("Bunny Tools/New Waypoint")]
    static void Init()
    {
        GameObject newDummy = new GameObject();
        newDummy.name = "NewWaypointParent";
        newDummy.AddComponent<WaypointNodeParent>();
        Selection.activeGameObject = newDummy;
    }

    // When the node editor is selected, should be used for initialization.
    void OnEnable()
    {
        currentWaypoints = (WaypointNodeParent)target;
    }

    // When the waypoint list holder is deselected, destruct shit should be set here. If needed.
    void OnDisable()
    {
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.BeginHorizontal();
        editState = (NodeEditorState)GUILayout.SelectionGrid((int)editState,editNames, 1);
        EditorGUILayout.EndHorizontal();
        base.OnInspectorGUI();
    }

    void OnSceneGUI()
    {
        int ControlID = GUIUtility.GetControlID(FocusType.Passive);
        Hotkeys();

        switch (editState)
        {
            case NodeEditorState.NONE:
                break;
            case NodeEditorState.ADD:
                AddNodes();
                break;
            case NodeEditorState.DELETENODE:
                DeleteNode();
                break;
            case NodeEditorState.LINK:
                LinkNodes();
                break;
            case NodeEditorState.UNLINK:
                UnlinkNodes();
                break;
        }
        HandleUtility.AddDefaultControl(ControlID);
    }

    // Add node to the point in world.
    void AddNodes()
    {
        Event current = Event.current;

        if (!current.alt)
        {
            if (current.type == EventType.mouseDown && current.button == 0)
            {
                Ray mRay = HandleUtility.GUIPointToWorldRay(current.mousePosition);
                RaycastHit rHit;
                if (Physics.Raycast(mRay, out rHit, Mathf.Infinity))
                {
                    WaypointNode newNode = new WaypointNode();
                    newNode.Position = rHit.point;
                    currentWaypoints.WaypointList.Add(newNode);
                    Event.current.Use();
                }
            }
        }
        
    }

    // Delete the closest node to proximity of 1.0f on click
    void DeleteNode()
    {
        Event current = Event.current;
        WaypointNode dNode = null;

        if (!current.alt)
        {
            Ray mRay = HandleUtility.GUIPointToWorldRay(current.mousePosition);
            RaycastHit rHit;

            if (Physics.Raycast(mRay, out rHit, Mathf.Infinity))
            {
                dNode = currentWaypoints.GetClosestNode(rHit.point, 1.0f);
            }

            if (dNode != null)
                Handles.DrawWireDisc(dNode.Position, Vector3.up, 0.5f);
            

            if (current.type == EventType.mouseDown&& dNode!=null)
            {
                foreach (WaypointLink p in dNode.NeighborNodes)
                {
                    WaypointNode node = (WaypointNode)currentWaypoints.GetClosestNode(p.from);
                    node.RemoveLink(p);
                    node = (WaypointNode)currentWaypoints.GetClosestNode(p.to);
                    node.RemoveLink(p);
                }
                currentWaypoints.WaypointList.Remove(dNode);
                dNode = null;
            }
        }
    }

    // Link waypoint nodes
    void LinkNodes()
    {
        Event current = Event.current;
        WaypointNode cNode = null;

        if (!current.alt)
        {
            Ray mRay = HandleUtility.GUIPointToWorldRay(current.mousePosition);
            RaycastHit rHit;
            if (Physics.Raycast(mRay, out rHit, Mathf.Infinity))
            {
                cNode = currentWaypoints.GetClosestNode(rHit.point, 1.0f);
            }

            // Clear if right mouse is pressed.
            if (current.type == EventType.mouseDown && current.button == 1)
            {
                from = Vector3.zero;
                to = Vector3.zero;
            }

            if (cNode != null)
            {
                Handles.DrawWireDisc(cNode.Position, Vector3.up, 0.5f);
                if (current.type == EventType.mouseDown && current.button == 0 && from == Vector3.zero) // Mouse1
                {
                    from = cNode.Position;
                }
                else if (current.type == EventType.mouseDown && current.button == 0 && to == Vector3.zero) // Mouse1 and we have a from
                {
                    if (cNode.Position != from)
                    {
                        to = cNode.Position;
                        WaypointLink newLink = new WaypointLink(from, to);
                        WaypointNode tempN = currentWaypoints.GetClosestNode(from);
                        if (tempN != null)
                        {
                            if(tempN.AddLink(newLink))
                            {
                                tempN = currentWaypoints.GetClosestNode(to);
                                tempN.AddLink(newLink);
                                cNode = null;
                                from = to;
                                to = Vector3.zero;
                            }
                        }
                        else
                        {
                            cNode = null;
                            from = Vector3.zero;
                            to = Vector3.zero;
                            Debug.LogError("Could not find node from list.");
                        }
                    }
                }
            }

            if (from != Vector3.zero)
            {
                Handles.DrawLine(from, rHit.point);
            }
        }
    }

    void UnlinkNodes()
    {
        Event current = Event.current;
        WaypointNode cNode = null;
        // Handles.DrawRectangle(ControlID, 

        if (!current.alt)
        {
            Ray mRay = HandleUtility.GUIPointToWorldRay(current.mousePosition);
            RaycastHit rHit;
            if (Physics.Raycast(mRay, out rHit, Mathf.Infinity))
            {
                cNode = currentWaypoints.GetClosestNode(rHit.point, 1.0f);
            }

            // Clear if right mouse is pressed.
            if (current.type == EventType.mouseDown && current.button == 1)
            {
                from = Vector3.zero;
                to = Vector3.zero;
            }

            if (cNode != null)
            {
                Handles.DrawWireDisc(cNode.Position, Vector3.up, 0.5f);
                if (current.type == EventType.mouseDown && current.button == 0 && from == Vector3.zero) // Mouse1
                {
                    from = cNode.Position;
                }
                else if (current.type == EventType.mouseDown && current.button == 0 && to == Vector3.zero) // Mouse1 and we have a from
                {
                    if (cNode.Position != from)
                    {
                        to = cNode.Position;
                        WaypointLink newLink = new WaypointLink(from, to);
                        WaypointNode tempN = currentWaypoints.GetClosestNode(from);
                        if (tempN != null)
                        {
                            if (tempN.Unlink(newLink))
                            {
                                tempN = currentWaypoints.GetClosestNode(to);
                                tempN.Unlink(newLink);
                                cNode = null;
                                from = to;
                                to = Vector3.zero;
                            }
                        }
                        else
                        {
                            cNode = null;
                            from = Vector3.zero;
                            to = Vector3.zero;
                            Debug.LogError("Could not find node from list.");
                        }
                    }
                }
            }

            if (from != Vector3.zero)
            {
                Handles.DrawLine(from, rHit.point);
            }
        }

    }

    void Hotkeys()
    {
        Event current = Event.current;
        if (!current.alt)
        {
            if (current.isKey && current.keyCode == KeyCode.Alpha1)
            {
                editState = NodeEditorState.NONE;
                Repaint();
            }
            if (current.isKey && current.keyCode == KeyCode.Alpha2)
            {
                editState = NodeEditorState.ADD;
                Repaint();
            }
            if (current.isKey && current.keyCode == KeyCode.Alpha3)
            {
                editState = NodeEditorState.DELETENODE;
                Repaint();
            }
            if (current.isKey && current.keyCode == KeyCode.Alpha4)
            {
                editState = NodeEditorState.LINK;
                Repaint();
            }
            if (current.isKey && current.keyCode == KeyCode.Alpha5)
            {
                editState = NodeEditorState.UNLINK;
                Repaint();
            }
        }
    }
}