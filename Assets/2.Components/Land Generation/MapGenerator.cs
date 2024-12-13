using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode {NoiseMap, ColorMap, DrawMesh}
    public DrawMode drawMode;
    
    [Space]
    public int seed;
    
    public const int mapChunkSize = 241;
    [Range(0, 6)] public int levelOfDetail;
    public float noiseScale;

    public int octaves;
    public float lacunarity;
    [Range(0, 1)] public float persistence;
    
    public Vector2 offset;
    
    [Space]
    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;
    
    [Space]
    public TerrainType[] regions;
    
    [Space]
    public bool autoUpdate;
    
    public void GenerateMap()
    {
        var noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, noiseScale, seed, octaves, persistence, lacunarity, offset);

        Color[] colorMap = new Color[mapChunkSize * mapChunkSize];
        for (int y = 0; y < mapChunkSize; y++)
        {
            for (int x = 0; x < mapChunkSize; x++)
            {
                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentHeight > regions[i].height) continue;
                    
                    colorMap[y * mapChunkSize + x] = regions[i].color;
                    break;
                }
            }
        }
        
        MapDisplay mapDisplay = FindObjectOfType<MapDisplay>(); // TODO: later change to serialized field
        
        switch (drawMode)
        {
            case DrawMode.NoiseMap:
                mapDisplay.meshRenderer.gameObject.SetActive(false);
                mapDisplay.textureRenderer.gameObject.SetActive(true);
                mapDisplay.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
                break;
            case DrawMode.ColorMap:
                mapDisplay.meshRenderer.gameObject.SetActive(false);
                mapDisplay.textureRenderer.gameObject.SetActive(true);
                mapDisplay.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap, mapChunkSize, mapChunkSize));
                break;
            case DrawMode.DrawMesh:
                mapDisplay.meshRenderer.gameObject.SetActive(true);
                mapDisplay.textureRenderer.gameObject.SetActive(false);
                mapDisplay.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail),
                    TextureGenerator.TextureFromColorMap(colorMap, mapChunkSize, mapChunkSize));
                break;
        }
            
    }

    void OnValidate()
    {
        if (lacunarity < 1) lacunarity = 1;
        if (octaves < 0) octaves = 0;
    }

    [Serializable]
    public struct TerrainType
    {
        public string name;
        public float height;
        public Color color;
    }
}
