using UnityEditor;
using UnityEngine;

internal class AdjustHeights : ScriptableWizard
{
    private static TerrainData _terrainData;
    public float HeightAdjustment = 0.1f;

    [MenuItem("Terrain/Adjust Heights")]
    public static void CreateWizard()
    {
        string buttonText = "Cancel";
        _terrainData = null;

        Terrain terrainObject = Selection.activeObject as Terrain ?? Terrain.activeTerrain;

        if (terrainObject)
        {
            _terrainData = terrainObject.terrainData;
            buttonText = "Adjust Heights";
            
        }

        DisplayWizard<AdjustHeights>("Adjust Heights", buttonText);
    }

    private void OnWizardUpdate()
    {
        if (!_terrainData)
        {
            helpString = "No terrain found";
            return;
        }

        HeightAdjustment = Mathf.Clamp(HeightAdjustment, -1.0f, 1.0f);
        helpString = (_terrainData.size.y * HeightAdjustment) + " meters (" + (HeightAdjustment * 100.0) + "%)";
    }

    private void OnWizardCreate()
    {
        if (!_terrainData) return;

        Undo.RegisterUndo(_terrainData, "Adjust Heights");

        float[,] heights = _terrainData.GetHeights(0, 0, _terrainData.heightmapWidth, _terrainData.heightmapHeight);

        for (int y = 0; y < _terrainData.heightmapHeight; y++)
        {
            for (int x = 0; x < _terrainData.heightmapWidth; x++)
            {
                heights[y, x] = heights[y, x] + HeightAdjustment;
            }
        }

        _terrainData.SetHeights(0, 0, heights);
        _terrainData = null;
    }
}