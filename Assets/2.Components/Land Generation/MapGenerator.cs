using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using System.Threading;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode {NoiseMap, ColorMap, DrawMesh}
    public DrawMode drawMode;
    
    public Noise.NormalizeMode normalizeMode;
    
    [Space]
    public int seed;
    
    public const int mapChunkSize = 241;
    [Range(0, 6)] public int lodForPreview;
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
    
    private readonly Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new();
    private readonly Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new();
    
    public void DrawMapInEditor()
    {
        MapData mapData = GenerateMapData(Vector2.zero);
        MapDisplay mapDisplay = FindObjectOfType<MapDisplay>(); // TODO: later change to serialized field
        
        switch (drawMode)
        {
            case DrawMode.NoiseMap:
                mapDisplay.meshRenderer.gameObject.SetActive(false);
                mapDisplay.textureRenderer.gameObject.SetActive(true);
                mapDisplay.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
                break;
            case DrawMode.ColorMap:
                mapDisplay.meshRenderer.gameObject.SetActive(false);
                mapDisplay.textureRenderer.gameObject.SetActive(true);
                mapDisplay.DrawTexture(TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
                break;
            case DrawMode.DrawMesh:
                mapDisplay.meshRenderer.gameObject.SetActive(true);
                mapDisplay.textureRenderer.gameObject.SetActive(false);
                mapDisplay.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, lodForPreview),
                    TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
                break;
        }
    }

    public void RequestMapData(Vector2 center, Action<MapData> callback)
    {
        ThreadStart threadStart = delegate { MapDataThread(center, callback); };
        new Thread(threadStart).Start();
    }

    private void MapDataThread(Vector2 center, Action<MapData> callback)
    {
        MapData mapData = GenerateMapData(center);
        lock (mapDataThreadInfoQueue)
        {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
        }
    }

    public void RequestMeshData(MapData mapData, int lod, Action<MeshData> callback)
    {
        ThreadStart threadStart = delegate { MeshDataThread(mapData, lod, callback); };
        new Thread(threadStart).Start();
    }

    private void MeshDataThread(MapData mapData, int lod, Action<MeshData> callback)
    {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, lod);
        lock (meshDataThreadInfoQueue)
        {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
    }

    private void Update()
    {
        if (mapDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < mapDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
                threadInfo.callback (threadInfo.parameter);
            }
        }
        
        if (meshDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < meshDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                threadInfo.callback (threadInfo.parameter);
            }
        }
    }

    public MapData GenerateMapData(Vector2 center)
    {
        var noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, noiseScale, seed, octaves, persistence, lacunarity, center + offset, normalizeMode);

        Color[] colorMap = new Color[mapChunkSize * mapChunkSize];
        for (int y = 0; y < mapChunkSize; y++)
        {
            for (int x = 0; x < mapChunkSize; x++)
            {
                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentHeight < regions[i].height) break;
                    
                    colorMap[y * mapChunkSize + x] = regions[i].color;
                }
            }
        }

        return new MapData(noiseMap, colorMap);

    }

    void OnValidate()
    {
        if (lacunarity < 1) lacunarity = 1;
        if (octaves < 0) octaves = 0;
    }

    struct MapThreadInfo<T>
    {
        public readonly Action<T> callback;
        public T parameter;

        public MapThreadInfo(Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }
    }
}

[Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color color;
}

public struct MapData
{
    public readonly float[,] heightMap;
    public readonly Color[] colorMap;

    public MapData(float[,] heightMap, Color[] colorMap)
    {
        this.heightMap = heightMap;
        this.colorMap = colorMap;
    }
}