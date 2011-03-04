using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;

public class TerrainImporter : AssetPostprocessor
{
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (var ass in importedAssets)
        {
            if (ass.EndsWith(".txt"))
            {
                StreamReader sr = new StreamReader(ass);
                var s = sr.ReadLine();
                if (s == "[Terrain Importer]")
                {
                    var baseparam = new Hashtable();
                    baseparam["terrainWidth"] = "1000";
                    baseparam["terrainHeight"] = "200";
                    baseparam["terrainLength"] = "1000";
                    baseparam["terrainTileX"] = "0";
                    baseparam["terrainTileY"] = "0";
                    baseparam["equalizeLayers"] = 0;
                    baseparam["heightFormat"] = "r16littleendian";
                    var param = (Hashtable)baseparam.Clone();
                    var currentoutput = "";
                    while (sr.Peek() >= 0)
                    {
                        s = sr.ReadLine();
                        if (s.StartsWith("[") && s.EndsWith("]"))
                        {
                            if (currentoutput == "")
                            {
                                baseparam = (Hashtable)param.Clone();
                            }
                            else
                            {
                                GenerateTerrain(currentoutput, param, Path.GetDirectoryName(ass));
                            }
                            currentoutput = s.Substring(1, s.Length - 2);
                            param = (Hashtable)baseparam.Clone();
                        }
                        else if (s == "" || s.StartsWith("//") || s.StartsWith("#"))
                        {
                            
                        }
                        else
                        {
                            var kv = s.Split("="[0]);
                            param[kv[0]] = kv[1];
                        }
                    }
                    if (currentoutput == "")
                    {
                        GenerateTerrain(Path.GetFileNameWithoutExtension(ass) + "-terrain", param, Path.GetDirectoryName(ass));
                    }
                    else
                    {
                        GenerateTerrain(currentoutput, param, Path.GetDirectoryName(ass));
                    }
                }
                sr.Close();
            }
        }
    }

    static void GenerateTerrain(string output, Hashtable param, string path)
    {
        string terraindatapath = Path.Combine(path, output + ".asset");
        Debug.Log("Generate Terrain: " + terraindatapath);

        TerrainData terrainData = (TerrainData)AssetDatabase.LoadAssetAtPath(terraindatapath, typeof(TerrainData));
        if (!terrainData)
        {
            terrainData = new TerrainData();
            AssetDatabase.CreateAsset(terrainData, terraindatapath);
        }

        FileInfo fi = new FileInfo(Path.Combine(path, (string)param["heightFile"]));

        int hfSamples = (int)fi.Length / 2;
        int hfWidth;
        int hfHeight;
        if (!int.TryParse((string)param["heightFileWidth"], out hfWidth))
            hfWidth = 0;
        if (!int.TryParse((string)param["heightFileHeight"], out hfHeight) || hfHeight <= 0)
        {
            if (hfWidth > 0)
                hfHeight = hfSamples / hfWidth;
            else
                hfHeight = hfWidth = Mathf.CeilToInt(Mathf.Sqrt(hfSamples));
        }
        else
        {
            if (hfWidth <= 0)
                hfWidth = hfSamples / hfHeight;
        }
        int size;
        if (!int.TryParse((string)param["terrainTileSize"], out size))
            size = hfWidth;
        int tOffX;
        if (!int.TryParse((string)param["terrainTileX"], out tOffX))
            tOffX = 0;
        int tOffY;
        if (!int.TryParse((string)param["terrainTileY"], out tOffY))
            tOffY = 0;

        if (tOffX < 0 || tOffY < 0 || (size - 1) * tOffX > hfWidth || (size - 1) * tOffY > hfHeight)
        {
            Debug.LogError("terrainTile (" + tOffX + "," + tOffY + ") of size " + size + "x" + size + " "
                    + "is outside heightFile size " + hfWidth + "x" + hfHeight);
            return; 
        }

        
        tOffX = (size - 1) * tOffX;
        tOffY = (size - 1) * tOffY;

        int bpp = 2; 

        int x;
        int y;

        FileStream fs = fi.OpenRead();
        var b = new byte[size * size * bpp];
        fs.Seek((tOffX + tOffY * hfWidth) * bpp, SeekOrigin.Current);
        if (size == hfWidth)
            fs.Read(b, 0, size * size * bpp);
        else
        {
            for (y = 0; y < size; ++y)
            {
                fs.Read(b, y * size * bpp, size * bpp);
                if (y + 1 < size)
                    fs.Seek((hfWidth - size) * bpp, SeekOrigin.Current);
            }
        }
        fs.Close();

        var h = new float[size, size];
        int i = 0;

        if ((string)param["heightFormat"] == "r16bigendian")
        {
            for (x = 0; x < size; ++x)
            {
                for (y = 0; y < size; ++y)
                {
                    h[size - 1 - x, y] = (b[i++] * 256.0f + b[i++]) / 65535.0f;
                }
            }
        }
        else
        { 
            for (x = 0; x < size; ++x)
            {
                for (y = 0; y < size; ++y)
                {
                    h[size - 1 - x, y] = (b[i++] + b[i++] * 256.0f) / 65535.0f;
                }
            }
        }

        terrainData.heightmapResolution = size - 1;

        if (param.ContainsKey("layer0File") || param.ContainsKey("layer1File"))
        {
            var nlayers = 2;
            while (param["layer" + nlayers + "File"] != null) nlayers++;

            var alphas = new float[1, 1, 1];
            int asize = 0;
            int amWidth = 0;
            for (var lay = 0; lay < nlayers; ++lay)
            {
                if (param.ContainsKey("layer" + lay + "File"))
                {
                    fi = new FileInfo(Path.Combine(path, (string)param["layer" + lay + "File"]));
                    if (asize == 0)
                    {
                        var amSamples = fi.Length;
                        asize = (int)(size * amSamples / hfSamples);
                        amWidth = (int)(hfWidth * amSamples / hfSamples);
                        terrainData.alphamapResolution = (int)asize;
                        alphas = new float[asize, asize, nlayers];
                    }


                    fs = fi.OpenRead();
                    b = new byte[asize * asize];
                    fs.Seek(tOffX + tOffY * amWidth, SeekOrigin.Current);
                    if (asize == amWidth)
                    {
                        fs.Read(b, 0, asize * asize);
                    }
                    else
                    {
                        for (y = 0; y < asize; ++y)
                        {
                            fs.Read(b, y * asize, asize);
                            if (y + 1 < asize)
                                fs.Seek(amWidth - asize, SeekOrigin.Current);
                        }
                    }

                    fs.Close();
                    i = 0;
                    for (x = 0; x < asize; ++x)
                    {
                        for (y = 0; y < asize; ++y)
                        {
                            alphas[asize - 1 - x, y, lay] = b[i++] / 256.0f;
                        }
                    }
                }
            }
            if (param.ContainsKey("equalizeLayers"))
            {
                if (!param.ContainsKey("layer0File"))
                {
                    
                    for (x = 0; x < asize; ++x)
                    {
                        for (y = 0; y < asize; ++y)
                        {
                            float rem = 1.0f;
                            for (var lay = 1; lay < nlayers; ++lay)
                                rem -= alphas[asize - 1 - x, y, lay];
                            if (rem > 0.0f)
                                alphas[asize - 1 - x, y, 0] = rem;
                        }
                    }
                }
                
                for (x = 0; x < asize; ++x)
                {
                    for (y = 0; y < asize; ++y)
                    {
                        float tot = 0.0f;
                        for (var lay = 0; lay < nlayers; ++lay)
                            tot += alphas[asize - 1 - x, y, lay];
                        if (tot > 0.0f)
                        {
                            for (var lay = 0; lay < nlayers; ++lay)
                                alphas[asize - 1 - x, y, lay] = alphas[asize - 1 - x, y, lay] / tot;
                        }
                    }
                }
            }
            SplatPrototype[] oldsp = terrainData.splatPrototypes;
            SplatPrototype[] sp = new SplatPrototype[nlayers];
            for (var lay = 0; lay < nlayers; ++lay)
            {
                string splat = "layer" + lay;
                sp[lay] = new SplatPrototype();

                Vector2 ts;
                if (!float.TryParse((string)param[splat + "Width"], out ts.x))
                {
                    if (lay >= oldsp.Length) ts.x = 15;
                    else ts.x = oldsp[lay].tileSize.x;
                }
                if (!float.TryParse((string)param[splat + "Height"], out ts.y))
                {
                    if (lay >= oldsp.Length) ts.y = 15;
                    else ts.y = oldsp[lay].tileSize.y;
                }

                Vector2 to;
                if (!float.TryParse((string)param[splat + "OffsetX"], out to.x))
                {
                    if (lay >= oldsp.Length) to.x = 0;
                    else to.x = oldsp[lay].tileOffset.x;
                }
                if (!float.TryParse((string)param[splat + "OffsetY"], out to.y))
                {
                    if (lay >= oldsp.Length) to.y = 0;
                    else to.y = oldsp[lay].tileOffset.y;
                }
                Texture2D tex = null;
                var texfile = (string)param[splat + "Texture"];
                if (texfile != null)
                {
                    if (texfile.StartsWith("/") || texfile.StartsWith("\\"))
                    {
                        tex = (Texture2D)AssetDatabase.LoadMainAssetAtPath("Assets" + (string)param[splat + "Texture"]);
                    }
                    else
                    {
                        tex = (Texture2D)AssetDatabase.LoadMainAssetAtPath(Path.Combine(path, (string)param[splat + "Texture"]));
                    }
                }
                else if (lay < oldsp.Length)
                {
                    tex = oldsp[lay].texture;
                }
                if (tex == null)
                {
                    tex = new Texture2D(1, 1);
                    int g = (lay - 1) / nlayers;
                    tex.SetPixel(0, 0,
                        lay == 0 ? Color.red :
                        lay == 1 ? Color.green :
                        lay == 2 ? Color.blue : new Color(g, g, g));
                    tex.Apply();
                }
                sp[lay].texture = tex;
                sp[lay].tileSize = ts;
                sp[lay].tileOffset = to;
            }
            terrainData.splatPrototypes = sp;
            terrainData.SetAlphamaps(0, 0, alphas);
        }

        Vector3 sz;
        sz.x = float.Parse((string)param["terrainWidth"]);
        sz.y = float.Parse((string)param["terrainHeight"]);
        sz.z = float.Parse((string)param["terrainLength"]);

        terrainData.size = sz;
        terrainData.SetHeights(0, 0, h);
        terrainData.RefreshPrototypes();
    }
}
 
