
//import System.IO;


//class TerrainImporter extends AssetPostprocessor {
//    static function OnPostprocessAllAssets  (
//        importedAssets : String[],
//        deletedAssets : String[],
//        movedAssets : String[],
//        movedFromAssetPaths : String[]) 
//    {
//        for (var ass in importedAssets) {
//            if (ass.EndsWith(".txt")) {
//                var sr = new StreamReader(ass);
//                var s = sr.ReadLine();
//                if (s == "[Terrain Importer]") {
//                    var baseparam = new Hashtable();
//                    baseparam["terrainWidth"] = "1000";
//                    baseparam["terrainHeight"] = "200";
//                    baseparam["terrainLength"] = "1000";
//                    baseparam["terrainTileX"] = "0";
//                    baseparam["terrainTileY"] = "0";
//                    baseparam["equalizeLayers"] = 0;
//                    baseparam["heightFormat"] = "r16littleendian";
//                    var param = baseparam.Clone();
//                    var currentoutput = "";
//                    while (sr.Peek() >= 0) {
//                        s = sr.ReadLine();
//                        if (s.StartsWith("[") && s.EndsWith("]")) {
//                            if (currentoutput == "") {
//                                baseparam = param.Clone();
//                            } else {
//                                GenerateTerrain(currentoutput,param,Path.GetDirectoryName(ass));
//                            }
//                            currentoutput = s.Substring(1,s.Length-2);
//                            param = baseparam.Clone();
//                        } else if (s == "" || s.StartsWith("//") || s.StartsWith("#")) {
//                            // Ignore (comment)
//                        } else {
//                            var kv = s.Split("="[0]);
//                            param[kv[0]]=kv[1];
//                        }
//                    }
//                    if (currentoutput == "") {
//                        GenerateTerrain(Path.GetFileNameWithoutExtension(ass)+"-terrain",param,Path.GetDirectoryName(ass));
//                    } else {
//                        GenerateTerrain(currentoutput,param,Path.GetDirectoryName(ass));
//                    }
//                }
//                sr.Close();
//            }
//        }
//    }
//    
//    static function GenerateTerrain(output : String, param : Hashtable, path : String)
//    {
//        var terraindatapath = Path.Combine(path,output + ".asset");
//        Debug.Log("Generate Terrain: " + terraindatapath);

//        var terrainData : TerrainData = AssetDatabase.LoadAssetAtPath(terraindatapath,TerrainData);
//        if (!terrainData) {
//            terrainData = new TerrainData();
//            AssetDatabase.CreateAsset(terrainData, terraindatapath);
//        }
//        
//        var fi = new FileInfo(Path.Combine(path,param["heightFile"]));
//        
//        var hfSamples : int = fi.Length/2;
//        var hfWidth : int;
//        var hfHeight : int;
//        if (!int.TryParse(param["heightFileWidth"],hfWidth))
//            hfWidth = 0;
//        if (!int.TryParse(param["heightFileHeight"],hfHeight) || hfHeight <= 0) {
//            if (hfWidth > 0)
//                hfHeight = hfSamples/hfWidth;
//            else
//                hfHeight = hfWidth = Mathf.CeilToInt(Mathf.Sqrt(hfSamples));
//        } else {
//            if (hfWidth <= 0)
//                hfWidth = hfSamples/hfHeight;
//        }
//        var size : int;
//        if (!int.TryParse(param["terrainTileSize"],size))
//            size = hfWidth;
//        var tOffX : int;
//        if (!int.TryParse(param["terrainTileX"],tOffX))
//            tOffX = 0;
//        var tOffY : int;
//        if (!int.TryParse(param["terrainTileY"],tOffY))
//            tOffY = 0;
//            
//        if (tOffX < 0 || tOffY < 0 || (size-1)*tOffX > hfWidth || (size-1)*tOffY > hfHeight) {
//            Debug.LogError("terrainTile ("+tOffX+","+tOffY+") of size "+size+"x"+size+" "
//                    +"is outside heightFile size "+hfWidth+"x"+hfHeight);
//            return; // We don't want to Seek/Read outside file bounds.
//        }
//                    
//        // Stitching reuses right/bottom edges.
//        tOffX = (size-1)*tOffX;
//        tOffY = (size-1)*tOffY;
//        
//        var bpp = 2; // only word formats are currently supported
//        
//        var x;
//        var y;
//        
//        var fs = fi.OpenRead();
//        var b = new byte[size*size*bpp];
//        fs.Seek((tOffX+tOffY*hfWidth)*bpp, SeekOrigin.Current);
//        if (size == hfWidth) {
//            fs.Read(b,0,size*size*bpp);
//        } else {
//            for (y=0; y<size; ++y) {
//                fs.Read(b,y*size*bpp,size*bpp);
//                if (y+1<size)
//                    fs.Seek((hfWidth-size)*bpp, SeekOrigin.Current);
//            }
//        }
//        fs.Close();

//        var h =  new float[size,size];
//        var i=0;
//        
//        if (param["heightFormat"] == "r16bigendian") {
//            for (x=0; x<size; ++x) {
//                for (y=0; y<size; ++y) {
//                    h[size-1-x,y] = (b[i++]*256.0+b[i++])/65535.0;
//                }
//            }
//        } else { // r16littleendian
//            for (x=0; x<size; ++x) {
//                for (y=0; y<size; ++y) {
//                    h[size-1-x,y] = (b[i++]+b[i++]*256.0)/65535.0;
//                }
//            }
//        }

//        terrainData.heightmapResolution = size-1;
//        
//        if (param["layer0File"] || param["layer1File"]) {
//            var nlayers = 2;
//            while (param["layer"+nlayers+"File"]) nlayers++;

//            var alphas = new float[1,1,1];
//            var asize = 0;
//            var amWidth = 0;
//            for (var lay=0; lay<nlayers; ++lay) {
//                if (param["layer"+lay+"File"]) {
//                    fi = new FileInfo(Path.Combine(path,param["layer"+lay+"File"]));
//                    if (asize==0) {
//                        var amSamples = fi.Length;
//                        asize = size * amSamples / hfSamples;
//                        amWidth = hfWidth * amSamples / hfSamples;
//                        terrainData.alphamapResolution = asize;
//                        alphas = new float[asize,asize,nlayers];
//                    }


//                    fs = fi.OpenRead();
//                    b = new byte[asize*asize];
//                    fs.Seek(tOffX+tOffY*amWidth, SeekOrigin.Current);
//                    if (asize == amWidth) {
//                        fs.Read(b,0,asize*asize);
//                    } else {
//                        for (y=0; y<asize; ++y) {
//                            fs.Read(b,y*asize,asize);
//                            if (y+1<asize)
//                                fs.Seek(amWidth-asize, SeekOrigin.Current);
//                        }
//                    }

//                    fs.Close();
//                    i=0;
//                    for (x=0; x<asize; ++x) {
//                        for (y=0; y<asize; ++y) {
//                            alphas[asize-1-x,y,lay] = b[i++]/256.0;
//                        }
//                    }
//                }
//            }
//            if (param["equalizeLayers"]) {
//                if (!param["layer0File"]) {
//                    // create layer0 by remainder
//                    for (x=0; x<asize; ++x) {
//                        for (y=0; y<asize; ++y) {
//                            var rem=1.0;
//                            for (lay=1; lay<nlayers; ++lay)
//                                rem -= alphas[asize-1-x,y,lay];
//                            if (rem > 0.0)
//                                alphas[asize-1-x,y,0] = rem;
//                        }
//                    }
//                }
//                // Equalize by rescaling
//                for (x=0; x<asize; ++x) {
//                    for (y=0; y<asize; ++y) {
//                        var tot=0.0;
//                        for (lay=0; lay<nlayers; ++lay)
//                            tot += alphas[asize-1-x,y,lay];
//                        if (tot > 0.0) {
//                            for (lay=0; lay<nlayers; ++lay)
//                                alphas[asize-1-x,y,lay] = alphas[asize-1-x,y,lay]/tot;
//                        }
//                    }
//                }
//            }
//            var oldsp = terrainData.splatPrototypes;
//            var sp = new SplatPrototype[nlayers];
//            for (lay=0; lay<nlayers; ++lay) {
//                var splat : String = "layer"+lay;
//                sp[lay] = new SplatPrototype();
//                
//                var ts : Vector2;
//                if (!float.TryParse(param[splat+"Width"],ts.x)) {
//                    if (lay >= oldsp.Length) ts.x = 15;
//                    else ts.x = oldsp[lay].tileSize.x;
//                }
//                if (!float.TryParse(param[splat+"Height"],ts.y)) {
//                    if (lay >= oldsp.Length) ts.y = 15;
//                    else ts.y = oldsp[lay].tileSize.y;
//                }
//                
//                var to : Vector2;
//                if (!float.TryParse(param[splat+"OffsetX"],to.x)) {
//                    if (lay >= oldsp.Length) to.x = 0;
//                    else to.x = oldsp[lay].tileOffset.x;
//                }
//                if (!float.TryParse(param[splat+"OffsetY"],to.y)) {
//                    if (lay >= oldsp.Length) to.y = 0;
//                    else to.y = oldsp[lay].tileOffset.y;
//                }
//                var tex : Texture2D;
//                var texfile = param[splat+"Texture"];
//                if (texfile) {
//                    if (texfile.StartsWith("/") || texfile.StartsWith("\\")) {
//                        tex = AssetDatabase.LoadMainAssetAtPath("Assets"+param[splat+"Texture"]);
//                    } else {
//                        tex = AssetDatabase.LoadMainAssetAtPath(Path.Combine(path,param[splat+"Texture"]));
//                    }
//                } else if (lay < oldsp.Length) {
//                    tex = oldsp[lay].texture;
//                }
//                if (!tex) {
//                    tex = new Texture2D(1,1);
//                    var g = (lay-1)/nlayers;
//                    tex.SetPixel(0,0,
//                        lay==0 ? Color.red :
//                        lay==1 ? Color.green :
//                        lay==2 ? Color.blue : Color(g,g,g));
//                    tex.Apply();
//                }
//                sp[lay].texture = tex;
//                sp[lay].tileSize = ts;
//                sp[lay].tileOffset = to;
//            }
//            terrainData.splatPrototypes = sp;
//            terrainData.SetAlphamaps(0,0,alphas);
//        }

//        var sz : Vector3;
//        sz.x = float.Parse(param["terrainWidth"]);
//        sz.y = float.Parse(param["terrainHeight"]);
//        sz.z = float.Parse(param["terrainLength"]);
//        
//        terrainData.size = sz;
//        terrainData.SetHeights(0,0,h);
//        terrainData.RefreshPrototypes();
//    }
//}