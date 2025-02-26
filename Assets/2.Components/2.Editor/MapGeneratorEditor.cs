using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MapGenerator mapGenerator = (MapGenerator)target;

        if (DrawDefaultInspector() && mapGenerator.autoUpdate)
        {
            mapGenerator.DrawMapInEditor();
        }

        if (GUILayout.Button("Generate Map"))
        {
            mapGenerator.DrawMapInEditor();
        }
    }
}

#endif