using System;
using System.Collections.Generic;
using UnityEngine;
using GeometryUtility;

[Serializable]
public class Map
{
    public float minlat;
    public float minlon;
    public float maxlat;
    public float maxlon;
    public Dictionary<int, Node> nodes = new Dictionary<int, Node>();
    public List<Way> ways = new List<Way>();
}
[Serializable]
public class Node
{
    public int id;
    public float lat;
    public float lon;
    public float x;
    public float y; 
    public Vector3 v { get { return new Vector3(0, y, x); } }
    public List<Tag> tags = new List<Tag>();
}
public class TagColor
{
    public string v;
    public string k;
    public Color color;
}
[Serializable]
public class Way
{
    public List<Node> nodes = new List<Node>();
    public List<CPoint2D> points = new List<CPoint2D>();
    public List<Tag> tags = new List<Tag>();
    //public List<CPoint2D[]> polygons = new List<CPoint2D[]>();
}
[Serializable]
public class Tag
{
    public string k;
    public string v;
}