using System;
using UnityEngine;
public class bs : Base
{
    public Vector2 pos2 { get { return new Vector2(transform.position.x, transform.position.y); } set { transform.position = new Vector3(value.x, value.y, transform.position.z); } }
    [FindTransform(scene=true)]
    public Game Game;
    [FindTransform(scene=true)]
    public Cam Cam;
    [FindTransform(scene = true)]
    public Menu GameGui;
    public void SetActive(bool value)
    {
        foreach (var t in this.transform.GetTransforms())
            t.gameObject.active = value;            
    }
}