// C# example:
using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(PaintDummy))]
public class PaintableObjectEditor : Editor {

    private bool painting = false;
    private int brushStep = 0;

    void Awake()
    {
    }

    public override void OnInspectorGUI()
    {
        GUILayout.Label("Brush radius: " + PaintableWindow.PaintVariables.brushRadius);
    }

    void OnSceneGUI()
    {
        if (PaintableWindow.isPaintOn)
        {
            // Editor events
            Event current = Event.current;

            // Current id
            int ControlID = GUIUtility.GetControlID(FocusType.Passive);

            // Do wicked stuff.
            Ray mRay = HandleUtility.GUIPointToWorldRay(current.mousePosition);
            RaycastHit rHit;

            if (!current.alt)
            {
                switch (current.type)
                {
                    case EventType.mouseDown:
                        if (current.button == 0)
                        {
                            painting = true;
                        }
                        else if (current.button == 1)
                        {
                            Debug.Log("Delete not implemented yet. :-) M2");
                        }
                        Event.current.Use();
                        break;
                    case EventType.mouseUp:
                        painting = false;
                        Event.current.Use();
                        break;
                    case EventType.layout:
                        HandleUtility.AddDefaultControl(ControlID);
                        break;
                }
            }

            if (painting)
            {
                // Painting raycast here.
                if (PaintableWindow.PaintVariables.ignorePaintLayer)
                {
                    if (Physics.Raycast(mRay, out rHit, Mathf.Infinity, PaintableWindow.PaintVariables.myLayer))
                    {
                        // Draw the handle, thinking about changing it to a gizmo.
                        // Handles.DrawSolidDisc(rHit.point, rHit.normal, PaintableWindow.PaintVariables.brushRadius);

                        // Intensity stepping, as the update goes only when events is happening in the scene window
                        // it will work in our favor.
                        brushStep++;
                        if (brushStep > (int)PaintableWindow.PaintVariables.brushIntensity)
                        {
                            // Reset brushstep
                            brushStep = 0;
                            // Create object in position in world and give it also the normal, which to reflect and check position
                            CreateObject(rHit.point, rHit.normal);
                        }
                        Event.current.Use();
                    }
                }
                else
                {
                    if (Physics.Raycast(mRay, out rHit))
                    {
                        // Draw the handle, thinking about changing it to a gizmo.
                        Handles.DrawSolidDisc(rHit.point, rHit.normal, PaintableWindow.PaintVariables.brushRadius);

                        // Intensity stepping, as the update goes only when events is happening in the scene window
                        // it will work in our favor.
                        brushStep++;
                        if (brushStep > (int)PaintableWindow.PaintVariables.brushIntensity)
                        {
                            // Reset brushstep
                            brushStep = 0;
                            // Create object in position in world and give it also the normal, which to reflect and check position
                            CreateObject(rHit.point, rHit.normal);
                        }
                        Event.current.Use();
                    }
                }
            }
            else if(!current.alt)
            {
                // Draw circle eventhough not painting and not rotationg around with camera, or moving viewport.
                // And ofcourse painting enabled.
                if (PaintableWindow.PaintVariables.ignorePaintLayer)
                {
                    if (Physics.Raycast(mRay, out rHit, Mathf.Infinity, PaintableWindow.PaintVariables.myLayer))
                    {
                        Handles.DrawWireDisc(rHit.point, rHit.normal, PaintableWindow.PaintVariables.brushRadius);
                        // Handles.Disc(Quaternion.LookRotation(rHit.normal), rHit.point, Vector3.up, PaintableWindow.PaintVariables.brushRadius, true, 1.0f);
                        Event.current.Use();
                    }
                }
                else
                {
                    if (Physics.Raycast(mRay, out rHit))
                    {
                        Handles.DrawWireDisc(rHit.point, rHit.normal, PaintableWindow.PaintVariables.brushRadius);
                        // Handles.Disc(Quaternion.LookRotation(rHit.normal), rHit.point, Vector3.up, PaintableWindow.PaintVariables.brushRadius, true, 1.0f);
                        Event.current.Use();
                    }
                }
            }
        }
    }

    void CreateObject(Vector3 p, Vector3 n)
    {
        RaycastHit mR; // The raycasthit
        bool objCreated = false;
        GameObject newObj = EditorUtility.InstantiatePrefab(EditorUtility.GetPrefabParent(PaintableWindow.PaintVariables.myObj)) as GameObject;

        // Random seed raycast position from radius
        Vector3 seedRadius = Random.insideUnitCircle * PaintableWindow.PaintVariables.brushRadius;
        seedRadius = new Vector3(seedRadius.x, 0.0f, seedRadius.y);

        if (!newObj)
        {
            Debug.LogWarning("This object is not connected to a prefab");
            objCreated = false;
        }
        else if (PaintableWindow.PaintVariables.ignorePaintLayer)
        {
            if (Physics.Raycast((p + seedRadius) + n, -n, out mR, 6.0f, PaintableWindow.PaintVariables.myLayer))
            {
                p = mR.point;
                n = mR.normal;

                Undo.RegisterCreatedObjectUndo(newObj, "objPainting");
                objCreated = true;
            }
        }
        else
        {
            if (Physics.Raycast((p + seedRadius) + n, -n, out mR, 6.0f))
            {
                p = mR.point;
                n = mR.normal;

                Undo.RegisterCreatedObjectUndo(newObj, "objPainting");
                objCreated = true;
            }
        }

        if (objCreated)
        {
            newObj.transform.rotation = Quaternion.identity;
            newObj.transform.position = p;
            newObj.transform.localScale *= PaintableWindow.PaintVariables.objectScale;

            // Calculate the orientation based on the ground normals
            if (PaintableWindow.PaintVariables.useNormals)
            {
                newObj.transform.rotation = Quaternion.FromToRotation(newObj.transform.up, n) * newObj.transform.rotation;
            }

            // Randomize 360, up.
            newObj.transform.rotation *= Quaternion.AngleAxis(Random.Range(0.0f, 360.0f), Vector3.up);

            // Some weird yittering
            if (PaintableWindow.PaintVariables.scaleYitter)
            {
                newObj.transform.localScale += new Vector3(Random.Range(PaintableWindow.PaintVariables.minYitter.x, PaintableWindow.PaintVariables.maxYitter.x), Random.Range(PaintableWindow.PaintVariables.minYitter.y, PaintableWindow.PaintVariables.maxYitter.y), Random.Range(PaintableWindow.PaintVariables.minYitter.z, PaintableWindow.PaintVariables.maxYitter.z));
            }

            // Better scale yittering
            if (PaintableWindow.PaintVariables.uniformscaleYitter)
            {
                newObj.transform.localScale *= (1 + Random.Range(PaintableWindow.PaintVariables.uniYitter.x, PaintableWindow.PaintVariables.uniYitter.y));
            }

            // Some randomness in the orientation
            if (PaintableWindow.PaintVariables.axisYitter)
            {
                Vector3 insideUnitSphere = Random.insideUnitCircle;
                insideUnitSphere = new Vector3(insideUnitSphere.x, 0.0f, insideUnitSphere.y);
                if (PaintableWindow.PaintVariables.useNormals)
                {
                    // Calculate the base if with normals.
                    Vector3 randNorm = n + insideUnitSphere;
                    n.Normalize();

                    newObj.transform.rotation *= Quaternion.AngleAxis(Random.Range(PaintableWindow.PaintVariables.minAxisYitterAngle, PaintableWindow.PaintVariables.maxAxisYitterAngle), randNorm);

                }
                else
                {
                    // If we dont use normals, we yitter by the inherited axis align
                    Vector3 randNorm = newObj.transform.up + insideUnitSphere;
                    n.Normalize();

                    newObj.transform.rotation *= Quaternion.AngleAxis(Random.Range(PaintableWindow.PaintVariables.minAxisYitterAngle, PaintableWindow.PaintVariables.maxAxisYitterAngle), randNorm);
                }
            }

            // Iterate through child colliders and assign correct layer.
            Collider[] children = newObj.GetComponentsInChildren<Collider>();
            foreach (Collider col in children)
            {
                col.gameObject.layer = LayerMask.NameToLayer("ObjPaint");
            }
            newObj.layer = LayerMask.NameToLayer("ObjPaint"); // Shift the layermask to a integer
        }
        else
            DestroyImmediate(newObj);
    }
}