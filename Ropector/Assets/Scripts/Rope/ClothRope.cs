using UnityEngine;
using System;
using System.Collections.Generic;

[System.Serializable]
public class TubeVertex
{
    public Vector3 point = Vector3.zero;
    public float radius = 1.0f;

    public TubeVertex(Vector3 pt, float r)
    {
        point = pt;
        radius = r;
    }
}

[RequireComponent(typeof(InteractiveCloth))]
[RequireComponent(typeof(ClothRenderer))]
[RequireComponent(typeof(Rigidbody))]
public class ClothRope : MonoBehaviour
{
    public GameObject ropeEnd = null;

    public int ropeSegments = 10;
    public int ropeRadialDetail = 8;
    public float ropeWidth = 0.25f;
    public bool ropeCanTearFromEnds = false;
    public bool RopeIsTorn { get { return iCloth.isTeared; } }
    Vector3 heading = Vector3.zero;
    Mesh ropeMesh;

    public void OnDrawGizmos()
    {
        if (!ropeEnd || Application.isPlaying) { return; }

        Gizmos.DrawLine(transform.position, ropeEnd.transform.position);

        DestroyImmediate(ropeMesh);
        tVerts.Clear();
        BuildMesh();
    }
    public void Start()
    {
        DestroyImmediate(ropeMesh);
        tVerts.Clear();
        BuildMesh();

        iCloth.AttachToCollider(gameObject.collider, ropeCanTearFromEnds, true);
        iCloth.AttachToCollider(ropeEnd.collider, ropeCanTearFromEnds, true);
    }

    public InteractiveCloth iCloth = null;
    List<TubeVertex> tVerts = new List<TubeVertex>();
    void BuildMesh()
    {
        if (!ropeEnd) { return; }

        heading = (ropeEnd.transform.position - transform.position) / ropeSegments;

        for(int i = 0; i <= ropeSegments; i++)
            tVerts.Add(new TubeVertex((heading * i), ropeWidth));


        ropeMesh = Tube.BuildRopeMesh(tVerts.ToArray(), ropeRadialDetail);
        ropeMesh.name = "RopeMesh";

        iCloth = gameObject.GetComponent<InteractiveCloth>();
        iCloth.mesh = ropeMesh;

        if (gameObject.collider == null)
            gameObject.AddComponent<BoxCollider>();

        if (ropeEnd.collider == null)
            ropeEnd.AddComponent<BoxCollider>();

        if (ropeEnd.rigidbody == null)
            ropeEnd.AddComponent<Rigidbody>();
    }
}


public class Tube
{
    private static Vector3[] crossPoints;
    private static int lastCrossSegments;
    private static Mesh mesh;
    private static bool colliderExists = false;

    public static Mesh BuildRopeMesh(TubeVertex[] vertices, int crossSegments)
    {
        mesh = new Mesh();

        if (crossSegments != lastCrossSegments)
        {
            crossPoints = new Vector3[crossSegments];
            float theta = 2.0f * Mathf.PI / crossSegments;
            for (int c = 0; c < crossSegments; c++)
            {
                crossPoints[c] = new Vector3(Mathf.Cos(theta * c), Mathf.Sin(theta * c), 0);
            }
            lastCrossSegments = crossSegments;
        }

        Vector3[] meshVertices = new Vector3[vertices.Length * crossSegments];
        Vector2[] uvs = new Vector2[vertices.Length * crossSegments];
        int[] tris = new int[vertices.Length * crossSegments * 6];
        int[] lastVertices = new int[crossSegments];
        int[] theseVertices = new int[crossSegments];
        Quaternion rotation = Quaternion.identity;

        for (int p = 0; p < vertices.Length; p++)
        {
            if (p < vertices.Length - 1)
                rotation = Quaternion.FromToRotation(Vector3.forward, vertices[p + 1].point - vertices[p].point);

            for (int c = 0; c < crossSegments; c++)
            {
                int vertexIndex = p * crossSegments + c;
                meshVertices[vertexIndex] = vertices[p].point + (rotation * crossPoints[c] * vertices[p].radius);
                uvs[vertexIndex] = new Vector2((0.0f + c) / crossSegments, (0.0f + p) / vertices.Length);

                lastVertices[c] = theseVertices[c];
                theseVertices[c] = p * crossSegments + c;
            }

            //make triangles
            if (p > 0)
            {
                for (int c = 0; c < crossSegments; c++)
                {
                    int start = (p * crossSegments + c) * 6;
                    tris[start] = lastVertices[c];
                    tris[start + 1] = lastVertices[(c + 1) % crossSegments];
                    tris[start + 2] = theseVertices[c];
                    tris[start + 3] = tris[start + 2];
                    tris[start + 4] = tris[start + 1];
                    tris[start + 5] = theseVertices[(c + 1) % crossSegments];
                }
            }
        }

        //Clear mesh for new build  (jf)   
        mesh.Clear();
        mesh.vertices = meshVertices;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        mesh.tangents = CalculateTangents(meshVertices);
        mesh.uv = uvs;

        return mesh;
    }

    static Vector4[] CalculateTangents(Vector3[] verts)
    {
       Vector4[] tangents = new Vector4[verts.Length];
   
       for(int i=0;i<tangents.Length;i++)
       {
          var vertex1 = i > 0 ? verts[i-1] : verts[i];
            var vertex2 = i < tangents.Length - 1 ? verts[i+1] : verts[i];
            var tan = (vertex1 - vertex2).normalized;
            tangents[i] = new Vector4( tan.x, tan.y, tan.z, 1.0f );
       }

       return tangents;   
    }
}
