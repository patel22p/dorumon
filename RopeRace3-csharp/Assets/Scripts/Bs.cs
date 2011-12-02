using UnityEngine;
using System.Collections;

public class Bs : Base {
    public static Game _Game;
    public static Camera _Camera;
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
