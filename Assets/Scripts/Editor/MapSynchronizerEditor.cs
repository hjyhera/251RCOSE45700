using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapSynchronizer))]
public class MapSynchronizerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        MapSynchronizer synchronizer = (MapSynchronizer)target;
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Map Synchronization Tools", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Synchronize Map Data"))
        {
            synchronizer.SynchronizeMapData();
        }
        
        if (GUILayout.Button("Generate Destructible Tiles"))
        {
            synchronizer.GenerateDestructibleTiles();
        }
        
        if (GUILayout.Button("Clear Destructible Tiles"))
        {
            synchronizer.ClearDestructibleTiles();
        }
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Full Synchronization"))
        {
            synchronizer.EditorSynchronize();
        }
        
        GUILayout.Space(5);
        
        EditorGUILayout.BeginHorizontal();
        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("Reset & Regenerate All"))
        {
            synchronizer.ResetAndRegenerateAll();
        }
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        if (synchronizer.mapData != null)
        {
            EditorGUILayout.LabelField($"Map Size: {synchronizer.mapData.width} x {synchronizer.mapData.height}");
            EditorGUILayout.LabelField($"Origin: {synchronizer.mapData.origin}");
            if (synchronizer.mapData.mapMatrix != null)
            {
                int destructibleCount = 0;
                int blockedCount = 0;
                foreach (int cell in synchronizer.mapData.mapMatrix)
                {
                    if (cell == 1) destructibleCount++;
                    else blockedCount++;
                }
                EditorGUILayout.LabelField($"Total Cells: {synchronizer.mapData.mapMatrix.Length}");
                EditorGUILayout.LabelField($"Empty Cells: {destructibleCount}");
                EditorGUILayout.LabelField($"Blocked Cells: {blockedCount}");
                
                float spawnRate = synchronizer.destructibleSpawnChance;
                int expectedTiles = Mathf.RoundToInt(destructibleCount * spawnRate);
                EditorGUILayout.LabelField($"Expected Tiles: ~{expectedTiles} ({spawnRate:P0} of {destructibleCount})");
            }
        }
        
        EditorGUILayout.Space();
        
        // Validation warnings
        if (synchronizer.indestructibleTilemap == null)
            EditorGUILayout.HelpBox("Indestructible Tilemap is not assigned!", MessageType.Error);
        if (synchronizer.destructibleTilemap == null)
            EditorGUILayout.HelpBox("Destructible Tilemap is not assigned!", MessageType.Error);
        if (synchronizer.destructibleTile == null)
            EditorGUILayout.HelpBox("Destructible Tile is not assigned!", MessageType.Error);
        if (synchronizer.mapData == null)
            EditorGUILayout.HelpBox("Map Data ScriptableObject is not assigned!", MessageType.Error);
    }
}
