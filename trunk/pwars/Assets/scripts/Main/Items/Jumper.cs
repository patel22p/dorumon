using UnityEngine;
using System.Collections;
public class Jumper : MapItem
{
    public Vector3 Magnet;
    public Vector3 Release;
    public float distance;
    public Vector2 multiplier = new Vector2(1, 1);
}
