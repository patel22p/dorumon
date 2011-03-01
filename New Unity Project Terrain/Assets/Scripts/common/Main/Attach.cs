using UnityEditor;
using UnityEngine;
using gui = UnityEditor.EditorGUILayout;
[ExecuteInEditMode]

public class Attach : MonoBehaviour
{
    public GameObject attachto;
    private Vector3 relPos;
    private Quaternion relRot;
    Quaternion rot;
    internal bool attach;
    public void OnInspectorGUI()
    {
        var o = attach;
        attach = gui.Toggle("attach", attach);
        if (attach != o)
            Undo.RegisterSceneUndo("rtools");
    }
    void Update()
    {
        if (attach)
        {
            transform.position = attachto.transform.TransformPoint(relPos);
            transform.rotation = attachto.transform.rotation * Quaternion.Inverse(relRot) * rot;
        }
        else
        {
            relRot = attachto.transform.rotation;
            relPos = attachto.transform.InverseTransformPoint(transform.position);
            rot = transform.rotation;
        }
    }

}
