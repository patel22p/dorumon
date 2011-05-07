using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

public class Tool : bs
{
    public int toolid;
}

public enum Brushes { Draw, Line, Trail, Spawn }

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
}
[Serializable]
public class DB
{
    public Vector3 startpos;
    public static XmlSerializer xml = new XmlSerializer(typeof(DB), new[] { typeof(Tooldb) });
    public List<Tooldb> tools = new List<Tooldb>();
}