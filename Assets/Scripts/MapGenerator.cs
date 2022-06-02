using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode { NoiseMap, Mesh };
    public DrawMode drawMode;

    public Noise.NormalizeMode normalizeMode;
    //vertices
    public const int chunkSize = 239;
    [Range(0f, 6f)]
    public int editorLOD;

    public TerrainData terrainData;
    public NoiseData noiseData;
    public TextureData textureData;

    public Material terrainMat;
 

    public bool autoUpdate;
    // Start is called before the first frame update
  
    void onTextureValuesUpdated()
    {
        textureData.ApplyToMaterial(terrainMat);
    }
    private void OnValidate()
    {
        if (terrainData != null)
        {
            terrainData.OnValuesUpdated -= DrawMapInEditor;
            terrainData.OnValuesUpdated += DrawMapInEditor;
        }
        if(noiseData!=null)
        {
            noiseData.OnValuesUpdated -= DrawMapInEditor;
            noiseData.OnValuesUpdated += DrawMapInEditor;
        }
        if(textureData!=null)
        {
            textureData.OnValuesUpdated -= onTextureValuesUpdated;
            textureData.OnValuesUpdated += onTextureValuesUpdated;
        }
    }
    public void DrawMapInEditor()
    {
        Texture2D outputTexture = null;
        MapData data = GenerateMapData(Vector2.zero);
        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (drawMode == DrawMode.NoiseMap)
        {
            outputTexture = TextureGenerator.GenerateTextureFromHeightMap(data.heightMap);
            display.DrawTexture(outputTexture);

        }
        else if (drawMode == DrawMode.Mesh)
        {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(data.heightMap,terrainData. meshHeightMultiplier, terrainData.meshHeightCurve, editorLOD));
        }
    }
    public Queue<DataCallBack<MapData>> queuedMapData = new Queue<DataCallBack<MapData>>();
    public Queue<DataCallBack<MeshData>> queuedMeshData = new Queue<DataCallBack<MeshData>>();
    public void RequestMapData(Vector2 chunkCenter, Action<MapData> callBack)
    {
        ThreadStart ts = delegate
        {
            MapDataThread(chunkCenter,callBack);
        };
        new Thread(ts).Start();
    }
    public void MapDataThread(Vector2 chunkCenter,Action<MapData> callBack)
    {
        MapData data = GenerateMapData(chunkCenter);
        lock (queuedMapData)
        {
            queuedMapData.Enqueue(new DataCallBack<MapData>(data, callBack));
        }
    }
    public void RequestMeshData(MapData mapData,int lod, Action<MeshData> callBack)
    {
        ThreadStart ts = delegate
        {
            MeshDataThread(mapData, lod,callBack);
        };
        new Thread(ts).Start();
    }
    public void MeshDataThread(MapData mapData,int lod, Action<MeshData> callBack)
    {
        MeshData data = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, terrainData.meshHeightMultiplier, terrainData.meshHeightCurve, lod);
        lock (queuedMeshData)
        {
            queuedMeshData.Enqueue(new DataCallBack<MeshData>(data, callBack));
        }
    }
    private void Update()
    {
        if (queuedMapData.Count > 0)
            for (int i = 0; i < queuedMapData.Count; i++)
            {
                DataCallBack<MapData> mapData = queuedMapData.Dequeue();
                mapData.callback(mapData.data);

            }
        if (queuedMeshData.Count > 0)
            for (int i = 0; i < queuedMeshData.Count; i++)
            {
                DataCallBack<MeshData> meshData = queuedMeshData.Dequeue();
                meshData.callback(meshData.data);

            }
        
    }
    void Awake()
    {
        textureData.ApplyToMaterial(terrainMat);
        textureData.UpdateMeshHeights(terrainMat, terrainData.minHeight, terrainData.maxHeight);
    }
    void Start()
    {
        textureData.UpdateMeshHeights(terrainMat, terrainData.minHeight, terrainData.maxHeight);
    }
    public MapData GenerateMapData(Vector2 chunkCenter)
    {
        //chunkSize +2 to compensate for borders
        float[,] noiseMap = Noise.GenerateNoiseMap(chunkSize+2, chunkSize+2, noiseData.seed, noiseData.noiseScale, noiseData.octaves, noiseData.persistance, noiseData.lacunarity, noiseData.offset +chunkCenter,normalizeMode, noiseData.noiseNormalizationEstimationMult);
        
        
        return new MapData(noiseMap);
    }

    // Update is called once per frame
 
}

public struct MapData
{
    public readonly float[,] heightMap;
    

    public MapData(float[,] heightMap)
    {
        this.heightMap = heightMap;
    }
}
public struct DataCallBack<T>
{
    public T data;
    public Action<T> callback;

    public DataCallBack(T data, Action<T> callback)
    {
        this.data = data;
        this.callback = callback;
    }
}