using System;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Random = UnityEngine.Random;
public class MyTerrain:MonoBehaviour
{
    //public List<Texture2D> list = new List<Texture2D>();
    public Texture2D[] grass;
    public Texture2D[] trees;
    public Texture2D[] alfas;
    public void Init()
    {
        Debug.Log("Genrate Terrain");
        var t = Terrain.activeTerrain;
        TerrainData td = t.terrainData;

        for (int i = 0; i < alfas.Length; i++)
        {
            var tx = alfas[i];
            float[, ,] hds = new float[tx.width, tx.height,2];
            var count = 0;
            for (int y = 0; y < tx.height; y++)
                for (int x = 0; x < tx.width; x++)
                {
                    //hds[x, y] = 1;
                    //var a = tx.GetPixel(y, tx.width - x).grayscale;
                    var a = tx.GetPixel(x, y).grayscale;
                    if (a > 0)
                    {
                        hds[x, y, 0] = a;
                        count++;                        
                    }
                }
            Debug.Log(count);
            td.RefreshPrototypes();
            td.SetAlphamaps(0, 0, hds);
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
        
    }
    void InitTerrain()
    {
        

        //var tx = (Texture2D)GameObject.FindObjectsOfTypeIncludingAssets(typeof(Texture2D)).Where(a => a.name == "terrain").FirstOrDefault();
        //float[,] hds = new float[tx.width, tx.height];
        //for (int y = 0; y < tx.height; y++)
        //    for (int x = 0; x < tx.width; x++)
        //    {
        //        hds[x, y] = tx.GetPixel(y, tx.width - x).grayscale;
        //    }        
        //td.SetHeights(0, 0, hds);

        //t.Flush();
        //var g = Terrain.CreateTerrainGameObject(td);
        //Instantiate(g);
        ////for (int y = 0; y < tx.height; y++)
        ////    for (int x = 0; x < tx.width; x++)
        ////    {
        ////        hds[x, y] = td.GetInterpolatedHeight(x, y);

        ////    }
        ////td.SetHeights(0, 0, hds);
    }
}