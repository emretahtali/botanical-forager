using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour
{
    private const float viewerMoveTresholdForChunkUpdate = 25f;
    private const float sqrViewerMoveTresholdForChunkUpdate = viewerMoveTresholdForChunkUpdate * viewerMoveTresholdForChunkUpdate;
    
    public LODInfo[] detailLevels;
    public static float maxViewDst;

    public Transform viewer;
    public Material mapMaterial;
    
    public static Vector2 viewerPosition;
    private Vector2 viewerPositionOld;
    
    private static MapGenerator mapGenerator;
    private int chunkSize;
    private int chunksVisibleInViewDst;
    
    private Dictionary<Vector2, TerrainChunk> chunkDictionary = new();
    private static List<TerrainChunk> chunksVisibleLastUpdate = new();

    void Start()
    {
        maxViewDst = detailLevels[^1].visibleDstTreshold;
        
        mapGenerator = FindObjectOfType<MapGenerator>();
        chunkSize = MapGenerator.mapChunkSize - 1;
        chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDst / chunkSize);
        
        UpdateVisibleChunks();
    }

    void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z) / mapGenerator.terrainData.uniformScale;
        
        if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrViewerMoveTresholdForChunkUpdate)
        {
            viewerPositionOld = viewerPosition;
            UpdateVisibleChunks();
        }
    }

    void UpdateVisibleChunks()
    {
        foreach (TerrainChunk chunk in chunksVisibleLastUpdate)
        {
            chunk.SetVisible(false);
        }
        chunksVisibleLastUpdate.Clear();
        
        int chunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int chunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        for (int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++)
        {
            for (int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(chunkCoordX + xOffset, chunkCoordY + yOffset);

                if (!chunkDictionary.ContainsKey(viewedChunkCoord))
                    chunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, detailLevels, transform, mapMaterial));
                
                chunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
            }
        }
    }

    public class TerrainChunk
    {
        private readonly GameObject meshObject;
        private readonly Vector2 position;
        private Bounds bounds;
        
        private MeshRenderer meshRenderer;
        private MeshFilter meshFilter;
        private MeshCollider meshCollider;
        
        private LODInfo[] detailLevels;
        private LODMesh[] lodMeshes;
        private LODMesh collisionLODMesh;
        private int previousLODIndex = -1;
        
        private MapData mapData;
        private bool mapDataReceived;

        public TerrainChunk(Vector2 coord, int size, LODInfo[] detailLevels, Transform parent, Material material)
        {
            this.detailLevels = detailLevels;
            
            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);
        
            meshObject = new GameObject("Terrain Chunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshCollider = meshObject.AddComponent<MeshCollider>();
            meshRenderer.material = material;
            
            meshObject.transform.position = positionV3 * mapGenerator.terrainData.uniformScale;
            meshObject.transform.parent = parent;
            meshObject.transform.localScale = Vector3.one * mapGenerator.terrainData.uniformScale;
            SetVisible(false);
            
            lodMeshes = new LODMesh[detailLevels.Length];
            for (int i = 0; i < lodMeshes.Length; i++)
            {
                lodMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateTerrainChunk);
                if (detailLevels[i].useForCollider)
                {
                    collisionLODMesh = lodMeshes[i];
                }
            }
            
            mapGenerator.RequestMapData(position, OnMapDataReceived);
        }

        private void OnMapDataReceived(MapData mapData)
        {
            this.mapData = mapData;
            mapDataReceived = true;
            
            UpdateTerrainChunk();
        }
        
        public void UpdateTerrainChunk()
        {
            if (!mapDataReceived) return;
            
            float sqrViewerDstFromNearestEdge = bounds.SqrDistance(viewerPosition);
            bool visible = sqrViewerDstFromNearestEdge <= math.pow(maxViewDst, 2);

            if (visible)
            {
                int lodIndex = 0;
                while (lodIndex < detailLevels.Length - 1
                       && sqrViewerDstFromNearestEdge > math.pow(detailLevels[lodIndex].visibleDstTreshold, 2))
                {
                    lodIndex++;
                }
                
                if (lodIndex != detailLevels.Length)
                {
                    LODMesh lodMesh = lodMeshes[lodIndex];
                    if (lodMesh.hasMesh)
                    {
                        previousLODIndex = lodIndex;
                        meshFilter.mesh = lodMesh.mesh;
                    }
                    else if (!lodMesh.hasRequestedMesh)
                    {
                        lodMesh.RequestMesh(mapData);
                    }
                }

                if (lodIndex == 0)
                {
                    if (collisionLODMesh.hasMesh)
                    {
                        meshCollider.sharedMesh = collisionLODMesh.mesh;
                    }
                    else if (!collisionLODMesh.hasRequestedMesh)
                    {
                        collisionLODMesh.RequestMesh(mapData);
                    }
                }
                
                chunksVisibleLastUpdate.Add(this);
            }
            
            SetVisible(visible);
        }

        public void SetVisible(bool visible)
        {
            meshObject.SetActive(visible);
        }

        public bool IsVisible()
        {
            return meshObject.activeSelf;
        }
    }

    private class LODMesh
    {
        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh;
        private int lod;
        private Action updateCallback;

        public LODMesh(int lod, Action updateCallback)
        {
            this.lod = lod;
            this.updateCallback = updateCallback;
        }

        private void OnMeshDataReceived(MeshData meshData)
        {
            mesh = meshData.CreateMesh();
            hasMesh = true;
            
            updateCallback?.Invoke();
        }

        public void RequestMesh(MapData mapData)
        {
            hasRequestedMesh = true;
            mapGenerator.RequestMeshData(mapData, lod, OnMeshDataReceived);
        }
    }

    [Serializable]
    public struct LODInfo
    {
        [Range(0, 6)] public int lod;
        public float visibleDstTreshold;
        public bool useForCollider;
    }
}
