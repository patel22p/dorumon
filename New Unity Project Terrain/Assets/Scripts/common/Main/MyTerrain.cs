using System;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;
using GUI = UnityEngine.GUILayout;
using gui = UnityEditor.EditorGUILayout;


public class MyTerrain : Base
{
    //public List<Texture2D> list = new List<Texture2D>();
    
    
    public Texture2D[] alphas;
    public Texture2D HeightMap;
    public Texture2D grass;
    public int grassType;
    public float alphaFactor = 1;
    public float grassFactor = 1;
    public float grassPow = 1;
    public float grassRandom = 0;
    public Texture2D trees;
    public int treeType;
    TerrainData td { get { return Terrain.activeTerrain.terrainData; } }

    void Start()
    {
    }
    void InitTerrain()
    {
        Start();
        var tx = HeightMap;
        var sz = td.heightmapResolution = tx.height;        
        float[,] hds = new float[sz, sz];
        for (int y = 0; y < sz; y++)
            for (int x = 0; x < sz; x++)
                hds[x, y] = tx.GetPixel(x,y).a;
        
        td.SetHeights(0, 0, hds);
    }
    void InitGrass()
    {
        Start();
        
        var sz = grass.width;
        Texture2D white = WhiteTexture(sz);
        //td.SetDetailResolution(sz, 8);
        int[,] hds = new int[sz, sz];
        grassRandom = Math.Max(grassRandom, 0);
        
        var tx = grass;
        //for (int lr = 0; lr < td.detailPrototypes.Length; lr++)

        for (int y = 0; y < sz; y++)
            for (int x = 0; x < sz; x++)
            {
                var a = Mathf.Pow(tx.GetPixel(x, y).a, grassPow) * grassFactor * Random.Range(1 - grassRandom, 1 + grassRandom);
                hds[x, y] = Mathf.RoundToInt(a);
            }
        td.SetDetailLayer(0, 0, grassType, hds);
    }
    void InitAlfaMaps()
    {
        Start();
        var sz = alphas[0].width;
        Texture2D white = WhiteTexture(sz);
        var alphs = new Texture2D[] { white }.Concat(alphas).ToArray();

        td.alphamapResolution = sz;
        float[, ,] hds = new float[sz, sz, alphs.Length];
        float[,] total = new float[sz, sz];
        for (int lr = alphs.Length - 1; lr >= 0; lr--)
        {
            var tx = alphs[lr];
            for (int y = 0; y < sz; y++)
                for (int x = 0; x < sz; x++)
                {
                    var a = Math.Min(1, tx.GetPixel(x, y).a * alphaFactor);
                    hds[x, y, lr] = Math.Max(0, a - total[x, y]);
                    total[x, y] += a;
                }
            td.SetAlphamaps(0, 0, hds);
        }
    }

    private static Texture2D WhiteTexture(int sz)
    {
        var clr = new Color[sz * sz];
        for (int i = 0; i < clr.Length; i++)
            clr[i] = new Color(1, 1, 1, 1);
        Texture2D white = new Texture2D(sz, sz);
        white.SetPixels(clr);
        return white;
    }
    

    public override void OnInspectorGUI()
    {
        
        if (GUI.Button("Init Terrain"))
            InitTerrain();
        if (GUI.Button("Init Alfas"))
            InitAlfaMaps();
        if (GUI.Button("Init Grass"))
            InitGrass();
        //for (int i = 0; i < td.treePrototypes.Length; i++)
        //{

        //}
        base.OnInspectorGUI();
    }
    

}















































//for (int i = 0; i < grass.Length; i++)
//{
//    var tx = grass[i];
//    int[,] hds = new int[tx.width, tx.height];
//    var count = 0;
//    for (int y = 0; y < tx.height; y++)
//        for (int x = 0; x < tx.width; x++)
//        {
//            //hds[x, y] = 1;
//            //var a = tx.GetPixel(y, tx.width - x).grayscale;
//            var a = tx.GetPixel(x, y).grayscale;
//            if (a > 0)
//            {
//                var r = Random.Range(0, (int)(a * 50));
//                if (r > 0)
//                {
//                    hds[x, y] = 1;
//                    count++;
//                }
//            }
//        }
//    Debug.Log(count);
//    td.SetDetailLayer(0, 0, i, hds);
//}