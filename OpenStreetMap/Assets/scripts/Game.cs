using UnityEngine;
using System.Collections;
using System.IO;
using System.Linq;
using System.Xml;
using PolygonCuttingEar;
using GeometryUtility;
using System.Collections.Generic;
public class Game : bs {
    public TagColor[] tags = new TagColor[]{
        new TagColor{ color = Color.green , k = "natural"},
        new TagColor{ color = Color.blue , k = "natural", v="water" },
        new TagColor{ color = Color.green , k = "natural", v="wood" },
        new TagColor{ color = Color.green , k = "wood", v="mixed" },
        
    };

    public Map map = new Map();
	IEnumerator Start () {        
        return ParseMap();
	}

    public Material mat;
    void OnDrawGizmos()
    {
        Gizmos.DrawLine(Vector3.zero, new Vector3(0, 0, 1));
        Gizmos.DrawLine(new Vector3(0, 0, 1), new Vector3(0, 1, 1));
        Gizmos.DrawLine(new Vector3(0, 1, 1), new Vector3(0, 1, 0));
        Gizmos.DrawLine(new Vector3(0, 1, 0), Vector3.zero);
        foreach (var v in map.ways)
        { 
            Node old=null;
            foreach (var n in v.nodes)
            {
                if (old != null)
                    Gizmos.DrawLine(old.v, n.v);
                old = n;
            }
        }
    }

    XmlElement prs;
    IEnumerator ParseMap()
    {
        Debug.Log("Load Map");
        LoadXml();
        LoadNodes();
        LoadWays();        
        //LoadMeshes();
        yield return null;
        
    }

    private void LoadWays()
    {
        foreach (XmlNode xmlnode in prs.SelectNodes("way"))
        {
            Way way = new Way();
            List<CPoint2D> points = new List<CPoint2D>();
            foreach (XmlNode nd in xmlnode.SelectNodes("nd"))
            {
                var id = int.Parse(nd.Attributes["ref"].Value);
                var node = map.nodes[id];
                way.nodes.Add(node);
                points.Add(new CPoint2D(node.x, node.y));
            }
            way.points = points;
            var cpolyShape = new CPolygonShape(way.points.ToArray());
            cpolyShape.CutEar();

            foreach (XmlNode tg in xmlnode.SelectNodes("tag"))
                way.tags.Add(new Tag { k = tg.Attributes["k"].Value, v = tg.Attributes["v"].Value });


            map.ways.Add(way);            
        }
    }

    private IEnumerator LoadMeshes()
    {
        Debug.Log("LoadMeshes");
        foreach (var way in map.ways)
        {
            Debug.Log("LoadWay");
            var cpolyShape = new CPolygonShape(way.points.ToArray());
            cpolyShape.CutEar();
            var count = cpolyShape.NumberOfPolygons * 3;
            Vector3[] vertices = new Vector3[count];
            int[] triangles = new int[count];
            int ji = 0;
            for (int i = 0; i < cpolyShape.NumberOfPolygons; i++)
            {
                var p = cpolyShape.Polygons(i);
                for (int j = 0; j < p.Length; j++)
                {
                    triangles[ji] = ji; ;
                    vertices[ji] = new Vector3(0, (float)p[j].Y, (float)p[j].X);
                    ji++;
                }
            }

            var g = new GameObject();
            var f = g.AddComponent<MeshFilter>();
            

            var info = g.AddComponent<Info>();
            info.tags = way.tags;

            var mesh = f.mesh = new Mesh();

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            var r = g.AddComponent<MeshRenderer>();
            r.material = mat;

            g.AddComponent<BoxCollider>();
            yield return null;
        }
        yield return null;
    }

    private void LoadNodes()
    { 
        foreach (XmlNode xmlnode in prs.SelectNodes("node"))
        {
            Node n = new Node();
            n.id = int.Parse(xmlnode.Attributes["id"].Value);
            n.lat = float.Parse(xmlnode.Attributes["lat"].Value);
            n.lon = float.Parse(xmlnode.Attributes["lon"].Value);
            n.x = (n.lon - map.minlon) / (map.maxlon - map.minlon);
            n.y = (n.lat - map.minlat) / (map.maxlat - map.minlat);

            foreach (XmlNode tg in xmlnode.SelectNodes("tag"))
                n.tags.Add(new Tag { k = tg.Attributes["k"].Value, v = tg.Attributes["v"].Value });                
            map.nodes.Add(n.id, n);
        }
    }

    void LoadXml()
    {
        var xml = new XmlDocument();

        xml.LoadXml(File.ReadAllText("Assets/map.xml"));
        prs = xml.DocumentElement;

        var b = prs.SelectSingleNode("bounds");
        map.minlat = float.Parse(b.Attributes["minlat"].Value);
        map.minlon = float.Parse(b.Attributes["minlon"].Value);

        map.maxlat = float.Parse(b.Attributes["maxlat"].Value);
        map.maxlon = float.Parse(b.Attributes["maxlon"].Value);
    }
	void Update () {
	
	}
}
