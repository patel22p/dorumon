using UnityEngine;
using System.Collections;

public class Bs : Base {

    public static Vector3 ZeroY(Vector3 v)
    {
        v.y = 0;
        return v;
    }
    public static Vector3 ZeroYNorm(Vector3 v)
    {
        v.y = 0;
        return v.normalized;
    }
}
