using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class TextureData : UpdatableData
{
    // public Color[] baseColors;
    // [Range(0, 1)] public float[] baseStartHeights;
    
    private float savedMinHeight;
    private float savedMaxHeight;
    
    public void ApplyToMaterial(Material material)
    {
        // material.SetInt("baseColorCount", baseColors.Length);
        // material.SetColorArray("baseColors", baseColors);
        // material.SetFloatArray("baseStartHeights", baseStartHeights);

        updateMeshHeights(material, savedMinHeight, savedMaxHeight);
    }

    public void updateMeshHeights(Material material, float minHeight, float maxHeight)
    {
        savedMinHeight = minHeight;
        savedMaxHeight = maxHeight;
        
        material.SetFloat("_minHeight", minHeight);
        material.SetFloat("_maxHeight", maxHeight);
    }
}
