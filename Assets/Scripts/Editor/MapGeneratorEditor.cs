using UnityEditor;
using UnityEditor.Experimental.TerrainAPI;
using UnityEngine;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    public override void OnInspectorGUI()
    {
        MapGenerator gen = (MapGenerator)target;
        if (DrawDefaultInspector())
            if (gen.autoUpdate)
                gen.DrawMapInEditor();
        if (GUILayout.Button("Generate"))
        {
            gen.DrawMapInEditor();
        }
    }
}
