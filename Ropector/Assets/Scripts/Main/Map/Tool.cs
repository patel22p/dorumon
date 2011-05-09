using UnityEngine;
using System.Collections;

using System.IO;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

public class Tool : bs
{
    public int toolid;
    public GUIContent discription;
    public ToolType toolType;
    public new Collider collider
    {
        get
        {
            return this.GetComponentInChildren<Collider>();
        }
    }
}
public enum ToolType { Grid, Trail }

[Serializable]
public class Tooldb
{
    public int toolid;
    //public Brushes tool;
    public Vector3 Pos;
    public Vector3 scale;
    public Quaternion rot;
    public string text;
    public bool spawn;
    public float speedTrackVell;
    public float animSpeedFactor;
}
[Serializable]
public class DB
{
    public Vector3 startpos;
    public static XmlSerializer xml = new XmlSerializer(typeof(DB), new[] { typeof(Tooldb) });
    public List<Tooldb> tools = new List<Tooldb>();
}