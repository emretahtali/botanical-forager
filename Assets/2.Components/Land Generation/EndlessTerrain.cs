using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour
{
    public const float maxViewDst = 450;
    public Transform viewer;
    
    public static Vector2 viewerPosition;
    int chunkSize;
    int chunksVisibleInViewDst;
    
    Dictionary<Vector2, TerrainChunk> chunkDictionary = new();
    List<TerrainChunk> chunksVisibleLastUpdate = new();

    void Start()
    {
        chunkSize = MapGenerator.mapChunkSize - 1;
        chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDst / chunkSize);
    }

    void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        UpdateVisibleChunks();
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
                    chunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, transform));
                
                chunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                if (chunkDictionary[viewedChunkCoord].IsVisible())
                    chunksVisibleLastUpdate.Add(chunkDictionary[viewedChunkCoord]);
            }
        }
    }
    
    public class TerrainChunk
    {
        GameObject meshObject;
        Vector2 position;
        Bounds bounds;

        public TerrainChunk(Vector2 coord, int size, Transform parent)
        {
            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);
        
            meshObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
            meshObject.transform.position = positionV3;
            meshObject.transform.localScale = Vector3.one * size / 10f;
            meshObject.transform.parent = parent;
            SetVisible(false);
        }

        public void UpdateTerrainChunk()
        {
            float sqrViewerDistanceFromNearestEdge = bounds.SqrDistance(viewerPosition);
            bool visible = sqrViewerDistanceFromNearestEdge <= math.pow(maxViewDst, 2);
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
}
