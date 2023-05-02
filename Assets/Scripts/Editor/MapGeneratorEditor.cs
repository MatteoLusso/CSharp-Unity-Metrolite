using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor( typeof( MapGenerator ) )]
public class MapGeneratorEditor : Editor 
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MapGenerator mapGen = ( MapGenerator )target;
        if( GUILayout.Button( "Genera Mappa" ) )
        {
            mapGen.StartGeneration();
        }
    }
}
