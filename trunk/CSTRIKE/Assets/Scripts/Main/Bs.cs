using UnityEngine;
using System.Collections;

public class Bs : Base {
    internal Transform tr { get { return transform; } }
    public Vector3 pos { get { return tr.position; } set { tr.position = value; } }
    public Vector3 position { get { return tr.position; } set { tr.position = value; } }
    public Transform parent { get { return tr.parent; } set { tr.parent = value; } }
    public Vector3 scale { get { return tr.localScale; } set { tr.localScale = value; } }
    public float posx { get { return pos.x; } set { var v = pos; v.x = value; pos = v; } }
    public float posy { get { return pos.y; } set { var v = pos; v.y = value; pos = v; } }
    public float posz { get { return pos.z; } set { var v = pos; v.z = value; pos = v; } }
    public float rotx { get { return rot.eulerAngles.x; } set { var e = rot.eulerAngles; e.x = value; rot = Quaternion.Euler(e); } }
    public float roty { get { return rot.eulerAngles.y; } set { var e = rot.eulerAngles; e.y = value; rot = Quaternion.Euler(e); } }
    public float rotz { get { return rot.eulerAngles.z; } set { var e = rot.eulerAngles; e.z = value; rot = Quaternion.Euler(e); } }
    public Quaternion rot { get { return tr.rotation; } set { tr.rotation = value; } }

    public float lrotx { get { return lrot.eulerAngles.x; } set { var e = lrot.eulerAngles; e.x = value; lrot = Quaternion.Euler(e); } }
    public float lroty { get { return lrot.eulerAngles.y; } set { var e = lrot.eulerAngles; e.y = value; lrot = Quaternion.Euler(e); } }
    public float lrotz { get { return lrot.eulerAngles.z; } set { var e = lrot.eulerAngles; e.z = value; lrot = Quaternion.Euler(e); } }
    public Quaternion lrot { get { return tr.localRotation; } set { tr.localRotation = value; } }

    static Cam m_Cam;
    public static Cam _Cam { get { if (m_Cam == null) m_Cam = (Cam)MonoBehaviour.FindObjectOfType(typeof(Cam)); return m_Cam; } }

    public virtual void Awake()
    {
        //tr = transform;
    }
}
