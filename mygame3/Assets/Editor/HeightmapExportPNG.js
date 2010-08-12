import System.IO; 
  
class HeightmapExportPNG extends EditorWindow 
{ 
    static var terraindata : TerrainData; 
    
    @MenuItem ("Terrain/Export Height Map as PNG") 
    static function Init () { 
        terraindata = null; 
        var terrain : Terrain = null; 
        
        if ( Selection.activeGameObject ) 
           terrain = Selection.activeGameObject.GetComponent( Terrain ); 

        if (!terrain) { 
            terrain = Terrain.activeTerrain; 
        } 
        if (terrain) { 
            terraindata = terrain.terrainData; 
          } 
      if (terraindata == null) { 
         EditorUtility.DisplayDialog("No terrain selected", "Please select a terrain.", "Cancel"); 
         return; 
       } 
        
        //// get the terrain heights into an array and apply them to a texture2D 
      var myBytes : byte[]; 
      var myIndex : int = 0; 
      var rawHeights = new Array(0.0,0.0); 
      var duplicateHeightMap = new Texture2D(terraindata.heightmapWidth, terraindata.heightmapHeight, TextureFormat.ARGB32, false); 
      rawHeights = terraindata.GetHeights(0, 0, terraindata.heightmapWidth, terraindata.heightmapHeight); 

      /// run through the array row by row 
       for (y=0; y < duplicateHeightMap.height; ++y) 
       { 
           for (x=0; x < duplicateHeightMap.width; ++x) 
           { 
              /// for wach pixel set RGB to the same so it's gray 
            var color = Vector4(rawHeights[myIndex], rawHeights[myIndex], rawHeights[myIndex], 1.0); 
            duplicateHeightMap.SetPixel (x, y, color); 
            myIndex++; 
           } 
       } 
          // Apply all SetPixel calls 
       duplicateHeightMap.Apply(); 

      /// make it a PNG and save it to the Assets folder 
      myBytes = duplicateHeightMap.EncodeToPNG(); 
      var filename : String = "DupeHeightMap.png"; 
      File.WriteAllBytes(Application.dataPath + "/" + filename, myBytes); 
      EditorUtility.DisplayDialog("Heightmap Duplicated", "Saved as PNG in Assets/ as: " + filename, ""); 
   } 
}