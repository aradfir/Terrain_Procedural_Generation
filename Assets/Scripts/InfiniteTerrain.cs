using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class InfiniteTerrain : MonoBehaviour
{
    public LODInfo[] LODInfos;
    
    [SerializeField]
    public static float maxViewDistance ;
    public Transform viewerTransform;
    public static Vector2 viewerPosition;
    public static Vector2 viewerPositionOld;
    Dictionary<Vector2, TerrainChunk> createdChunksDictionary = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();
    int chunkSize;
    int chunksVisibleInView;
    public Material material1;
    static MapGenerator mapGenerator;
    const float viewerMoveThrForChunkUpdate = 25f;
    const float sqrViewMoveThrForChunkUpdate = viewerMoveThrForChunkUpdate * viewerMoveThrForChunkUpdate;
    // Start is called before the first frame update
    void Start()
    {
        //grid count
        if (mapGenerator == null)
            mapGenerator = (MapGenerator)FindObjectOfType(typeof(MapGenerator));
        chunkSize = MapGenerator.chunkSize - 1;
        maxViewDistance = LODInfos[LODInfos.Length - 1].visibleDistThr;
        viewerPosition = new Vector2(viewerTransform.position.x, viewerTransform.position.z)/mapGenerator.terrainData.uniformScale;
        chunksVisibleInView = Mathf.RoundToInt(maxViewDistance / chunkSize);
        UpdateVisibleChunks();
        
    }
    // Update is called once per frame
    void Update()
    {
        viewerPosition = new Vector2(viewerTransform.position.x, viewerTransform.position.z)/ mapGenerator.terrainData.uniformScale;
        if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrViewMoveThrForChunkUpdate)
        {
            viewerPositionOld = viewerPosition;
            UpdateVisibleChunks();
        }
    }
    void UpdateVisibleChunks()
    {
        foreach (var chunk in terrainChunksVisibleLastUpdate)
        {
            chunk.SetVisible(false);
        }
        terrainChunksVisibleLastUpdate.Clear();


        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);
        for (int yOffset = -chunksVisibleInView; yOffset <= chunksVisibleInView; yOffset++)
        {
            for (int xOffset = -chunksVisibleInView; xOffset <= chunksVisibleInView; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                if (createdChunksDictionary.ContainsKey(viewedChunkCoord))
                {
                    createdChunksDictionary[viewedChunkCoord].UpdateTerrainChunk();
                    if (createdChunksDictionary[viewedChunkCoord].isVisible())
                    {
                        terrainChunksVisibleLastUpdate.Add(createdChunksDictionary[viewedChunkCoord]);
                    }
                }
                else
                {
                    if (material1 == null)
                        Debug.Log("WTF?");
                    createdChunksDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize,LODInfos, transform, material1,UpdateVisibleChunks));

                }
            }
        }
    }

    public class TerrainChunk
    {
        GameObject meshObject;
        Vector2 position;
        Bounds chunkBound;

        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        MeshCollider meshCollider;

        MapData data;
        bool mapDataReceived;
        bool meshColliderSet;
        LODInfo[] LODs;
        LODMesh[] lodmeshes;
        LODMesh colliderLODMesh;
        int activeLODLevel;
        Action updateCallBack;


        public TerrainChunk(Vector2 coord, int size,LODInfo[] infos, Transform parent, Material material,Action updateCallBack)
        {
            meshColliderSet = false;
            
            this.updateCallBack = updateCallBack;
            activeLODLevel = -1;
            LODs = infos;
            position = coord * size;
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);
            meshObject = new GameObject("Terrain Chunk");
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshCollider = meshObject.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = null;
            meshRenderer.material = material;
            meshObject.transform.position = positionV3* mapGenerator.terrainData.uniformScale;
            meshObject.transform.parent = parent;
            meshObject.transform.localScale = Vector3.one * mapGenerator.terrainData.uniformScale;
            chunkBound = new Bounds(positionV3, Vector3.one * size);
            lodmeshes = new LODMesh[infos.Length];
            for (int i = 0; i < lodmeshes.Length; i++)
            {
                lodmeshes[i] = new LODMesh(infos[i].lod, updateCallBack);
                if (infos[i].useForCollider)
                    colliderLODMesh = lodmeshes[i];
            }
            RequestMapData();
            UpdateTerrainChunk();
            
        }
        public void UpdateTerrainChunk()
        {
            if (!mapDataReceived)
                return;
            Vector3 positionV3 = new Vector3(viewerPosition.x, 0, viewerPosition.y);
            float viewerDistFromChunkSqr = Mathf.Sqrt(chunkBound.SqrDistance(positionV3));
            bool visible = viewerDistFromChunkSqr < maxViewDistance;
            if(visible)
            {
                int requiredLOD = 0;
                for(int i=0;i<LODs.Length-1;i++)
                {
                    if (LODs[i].visibleDistThr >= viewerDistFromChunkSqr)
                        break;
                    else
                        requiredLOD = i + 1;
                }
                if (activeLODLevel != requiredLOD)
                {
                    LODMesh mesh = lodmeshes[requiredLOD];
                    if (mesh.hasMesh)
                    {
                        activeLODLevel = requiredLOD;
                        meshFilter.mesh = mesh.mesh;
                        meshCollider.sharedMesh = mesh.mesh;
                    }
                    else if (!mesh.hasRequestedMesh)
                        mesh.RequestMesh(data);
                }
                if(LODs[requiredLOD].hasCollision)
                {
                    if (colliderLODMesh.hasMesh)
                    {
                        meshCollider.sharedMesh = colliderLODMesh.mesh;
                        meshColliderSet = true;
                    }
                    else if (!colliderLODMesh.hasRequestedMesh)
                        colliderLODMesh.RequestMesh(data);
                }
            }
            SetVisible(visible);
        }
        public void RequestMapData()
        {
            mapGenerator.RequestMapData(position,OnMapDataFetch);
        }
        public void OnMapDataFetch(MapData data)
        {
            this.data = data;
            mapDataReceived = true;
            updateCallBack();
        }
        public void SetVisible(bool visible)
        {
            meshObject.SetActive(visible);
        }
        public bool isVisible()
        {
            return meshObject.activeSelf;
        }
    }
    public class LODMesh
    {
        public int lod;
        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh = false;
        Action updateCallBack;
        public LODMesh(int lod, Action updateCallBack)
        {
            this.updateCallBack = updateCallBack;
            this.lod = lod;
            hasMesh = false;
            hasRequestedMesh = false;
        }
        public void onMeshDataReceieved(MeshData data)
        {   
            mesh = data.CreateMesh();
            hasMesh = true;
            updateCallBack();
        }
        public void RequestMesh(MapData data)
        {
            hasRequestedMesh = true;
            mapGenerator.RequestMeshData(data,lod, onMeshDataReceieved);
        }
    }
    [System.Serializable]
    public struct LODInfo
    {
        public int lod;
        public float visibleDistThr;
        public bool useForCollider;
        public bool hasCollision;
    }
}

