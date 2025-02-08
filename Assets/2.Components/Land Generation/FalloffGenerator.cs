using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FalloffGenerator
{
    public static float[,] GenerateFalloffMap(int size)
    {
        float[,] map = new float[size, size];
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                float x = (i / (float)size) * 2f - 1;
                float y = (j / (float)size) * 2f - 1;
                // float value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
                float value = (Mathf.Pow(x, 2) + Mathf.Pow(y, 2)) / 2f;
                map[i, j] = Mathf.Clamp01(value);
            }
        }
        return map;
    }
}
