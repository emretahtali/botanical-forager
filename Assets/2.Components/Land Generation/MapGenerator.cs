using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using System.Threading;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode {NoiseMap, DrawMesh, FalloffMap}
    public DrawMode drawMode;
    
    public TerrainData terrainData;
    public NoiseData noiseData;
    public TextureData textureData;
    
    public Material terrainMaterial;

    public const int mapChunkSize = 239;
    
    [Range(0, 6)] public int lodForPreview;
    
    [Space]
    public bool autoUpdate;
    
    private readonly Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new();
    private readonly Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new();

    private void OnValuesUpdated()
    {
        if (!Application.isPlaying) DrawMapInEditor();
        textureData.ApplyToMaterial(terrainMaterial);
    }
    
    public void DrawMapInEditor()
    {
        MapData mapData = GenerateMapData(Vector2.zero, terrainData.useFalloff);
        MapDisplay mapDisplay = FindObjectOfType<MapDisplay>(); // TODO: later change to serialized field
        
        switch (drawMode)
        {
            case DrawMode.NoiseMap:
                mapDisplay.meshRenderer.gameObject.SetActive(false);
                mapDisplay.textureRenderer.gameObject.SetActive(true);
                mapDisplay.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
                break;
            case DrawMode.DrawMesh:
                mapDisplay.meshRenderer.gameObject.SetActive(true);
                mapDisplay.textureRenderer.gameObject.SetActive(false);
                mapDisplay.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, terrainData.meshHeightMultiplier, terrainData.meshHeightCurve, lodForPreview));
                break;
            case DrawMode.FalloffMap:
                mapDisplay.meshRenderer.gameObject.SetActive(false);
                mapDisplay.textureRenderer.gameObject.SetActive(true);
                mapDisplay.DrawTexture(TextureGenerator.TextureFromHeightMap(FalloffGenerator.GenerateFalloffMap(mapChunkSize)));
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
        MapData mapData = GenerateMapData(center, false);
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
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, terrainData.meshHeightMultiplier, terrainData.meshHeightCurve, lod);
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

    public MapData GenerateMapData(Vector2 center, bool useFalloff)
    {
        var noiseMap = Noise.GenerateNoiseMap(mapChunkSize + 2, mapChunkSize + 2, noiseData.noiseScale, noiseData.seed, noiseData.octaves, noiseData.persistence, noiseData.lacunarity, center + noiseData.offset, noiseData.normalizeMode);
        float[,] falloffMap = (useFalloff) ? FalloffGenerator.GenerateFalloffMap(mapChunkSize + 2) : null;
        
        for (int y = 0; y < mapChunkSize + 2; y++)
        {
            for (int x = 0; x < mapChunkSize + 2; x++)
            {
                if (useFalloff) noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - falloffMap[x, y]);
            }
        }

        // textureData.updateMeshHeights(terrainMaterial, terrainData.minHeight, terrainData.maxHeight);
        textureData.updateMeshHeights(terrainMaterial, terrainData.minHeight, terrainData.maxHeight);
        
        return new MapData(noiseMap);
    }

    private void OnValidate()
    {
        if (terrainData != null)
        {
            terrainData.OnValuesUpdated -= OnValuesUpdated;
            terrainData.OnValuesUpdated += OnValuesUpdated;
        }
        if (noiseData != null)
        {
            noiseData.OnValuesUpdated += OnValuesUpdated;
            noiseData.OnValuesUpdated += OnValuesUpdated;
        }
        if (textureData != null)
        {
            textureData.OnValuesUpdated += OnValuesUpdated;
            textureData.OnValuesUpdated += OnValuesUpdated;
        }
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

public struct MapData
{
    public readonly float[,] heightMap;

    public MapData(float[,] heightMap)
    {
        this.heightMap = heightMap;
    }
}