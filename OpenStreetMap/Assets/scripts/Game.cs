using UnityEngine;
using System.Collections;
using System.IO;
using System.Linq;
using System.Xml;
using PolygonCuttingEar;
using GeometryUtility;
using System.Collections.Generic;
public class Game : bs {
    public Map map = new Map();
	void Start () {
        ParseMap();
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
    static Material lineMaterial;
    static void CreateLineMaterial()
    {
        if (!lineMaterial)
        {
            lineMaterial = new Material("Shader \"Lines/Colored Blended\" {" +
                "SubShader { Pass { " +
                "    Blend SrcAlpha OneMinusSrcAlpha " +
                "    ZWrite Off Cull Off Fog { Mode Off } " +
                "    BindChannels {" +
                "      Bind \"vertex\", vertex Bind \"color\", color }" +
                "} } }");
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            lineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
        }
    }

    void OnPostRender1()
    {
        CreateLineMaterial();
        lineMaterial.SetPass(0);
        
        GL.Begin(GL.TRIANGLES);
        GL.Color(Color.yellow);
        foreach (var w in map.ways)
        {
            foreach (var pl in w.polygons)
            {                
                for (int i = 0; i < pl.Length; i++)
                {
                    var p = pl[i];
                    GL.Vertex3(0, (float)p.Y, (float)p.X);
                }
            }
        }

        GL.End();
        //GL.Vertex3(0, 0, 0);
        //GL.Vertex3(1, 0, 0);
        //GL.Vertex3(0, 1, 0);
        //GL.Vertex3(1, 1, 0);

        //GL.Vertex3(0, 0, 0);
        //GL.Vertex3(0, 1, 0);
        //GL.Vertex3(1, 0, 0);
        //GL.Vertex3(1, 1, 0);
        //GL.End();
    } 

    void ParseMap()
    {
        
        var xml = new XmlDocument();
        
        xml.LoadXml(File.ReadAllText("Assets/map.xml"));
        var prs = xml.DocumentElement;

        var b = prs.SelectSingleNode("bounds");
        map.minlat = float.Parse(b.Attributes["minlat"].Value);
        map.minlon = float.Parse(b.Attributes["minlon"].Value);

        map.maxlat = float.Parse(b.Attributes["maxlat"].Value);
        map.maxlon = float.Parse(b.Attributes["maxlon"].Value);

        foreach (XmlNode xmlnode in prs.SelectNodes("node"))
        {
            Node n = new Node();
            n.id = int.Parse(xmlnode.Attributes["id"].Value);
            n.lat = float.Parse(xmlnode.Attributes["lat"].Value);
            n.lon = float.Parse(xmlnode.Attributes["lon"].Value);
            n.x = (n.lon - map.minlon) / (map.maxlon - map.minlon);
            n.y = (n.lat - map.minlat) / (map.maxlat - map.minlat);

            foreach (XmlNode tg in xmlnode.SelectNodes("tag"))
            {
                n.tags.Add(new Tag { k = tg.Attributes["k"].Value });
                n.tags.Add(new Tag { v = tg.Attributes["v"].Value });
            }
            map.nodes.Add(n);
        }
        foreach (XmlNode xmlnode in prs.SelectNodes("way"))
        {
            Way way = new Way();
            List<CPoint2D> points = new List<CPoint2D>();
            foreach (XmlNode nd in xmlnode.SelectNodes("nd"))
            {
                var node = map.nodes.First(a => a.id == int.Parse(nd.Attributes["ref"].Value));
                way.nodes.Add(node);
                points.Add(new CPoint2D(node.x,node.y));
            }
            var cpolyShape = new CPolygonShape(points.ToArray());
            cpolyShape.CutEar();
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            int ji = 0;
            for (int i = 0; i < cpolyShape.NumberOfPolygons; i++)
            {
                var p = cpolyShape.Polygons(i);
                way.polygons.Add(p);
                for (int j = 0; j < p.Length; j++)
                {
                    triangles.Add(ji);
                    vertices.Add(new Vector3(0, (float)p[j].Y, (float)p[j].X));
                    ji++;
                }
            }
            
            var g =  new GameObject();
            var f = g.AddComponent<MeshFilter>();
            var r = g.AddComponent<MeshRenderer>();
            r.material = mat;
            var m = f.mesh = new Mesh();
            
            m.vertices = vertices.ToArray();
            m.triangles = triangles.ToArray();
            //m.normals = normals.ToArray();
            m.RecalculateNormals();

            foreach (XmlNode tg in xmlnode.SelectNodes("tag"))
            {
                way.tags.Add(new Tag { k = tg.Attributes["k"].Value });
                way.tags.Add(new Tag { v = tg.Attributes["v"].Value });
            } 
            map.ways.Add(way);
        }
    }
	void Update () {
	
	}
}
