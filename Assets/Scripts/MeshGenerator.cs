using Assets.Scripts;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Scripting;

public class MeshData
{
    public Vector3[] vertices;
    public Vector3[] borderVertices;
    public int[] triangles;
    public int[] borderTriangles;
    public Vector2[] uvs;
    public Vector3[] bakedNormals;
    int triangleIndex;
    int borderTriangleIndex;

    public MeshData(int verticesPerLine)
    {
        vertices = new Vector3[verticesPerLine * verticesPerLine];
        borderVertices = new Vector3[4 * verticesPerLine + 4];

        uvs = new Vector2[verticesPerLine * verticesPerLine];

        triangles = new int[6 * (verticesPerLine - 1) * (verticesPerLine - 1)];
        triangleIndex = 0;
        borderTriangles = new int[24 * verticesPerLine];
        borderTriangleIndex = 0;
    }
    public void AddVertex(int index, Vector3 position, Vector2 uv)
    {
        if (index >= 0)
        {

            vertices[index] = position;
            uvs[index] = uv;
        }
        else
            try
            {
                borderVertices[-1 - index] = position;
            }
            catch(System.Exception ex)
            {
                Debug.Log(index+" "+ borderVertices.Length);
            }
    }
    public void addTriangle(int a, int b, int c)
    {
        if (a < 0 || b < 0 || c < 0)
        {
            //its a border triangle
            borderTriangles[borderTriangleIndex++] = a;
            borderTriangles[borderTriangleIndex++] = b;
            borderTriangles[borderTriangleIndex++] = c;
            return;
        }
        triangles[triangleIndex++] = a;
        triangles[triangleIndex++] = b;
        triangles[triangleIndex++] = c;
    }
    Vector3[] CalcuateNormals()
    {
        Vector3[] normals = new Vector3[vertices.Length];
        for (int i = 0; i < triangles.Length; i += 3)
        {
            int indexA = triangles[i];
            int indexB = triangles[i + 1];
            int indexC = triangles[i + 2];
            Vector3 triangleNormal = calculatePolygonNormal(indexA, indexB, indexC);
            normals[indexA] += triangleNormal;
            normals[indexB] += triangleNormal;
            normals[indexC] += triangleNormal;
        }
        for (int i = 0; i < borderTriangles.Length; i += 3)
        {
            int indexA = borderTriangles[i];
            int indexB = borderTriangles[i + 1];
            int indexC = borderTriangles[i + 2];
            Vector3 triangleNormal = calculatePolygonNormal(indexA, indexB, indexC);
            if (indexA >= 0)
                normals[indexA] += triangleNormal;
            if (indexB >= 0)
                normals[indexB] += triangleNormal;
            if (indexC >= 0)
                normals[indexC] += triangleNormal;
        }
        for (int i = 0; i < normals.Length; i++)
            normals[i].Normalize();
        return normals;
    }
    Vector3 calculatePolygonNormal(int indexA, int indexB, int indexC)
    {
        Vector3 positionA = (indexA < 0) ? borderVertices[-1 - indexA] : vertices[indexA];
        Vector3 positionB = (indexB < 0) ? borderVertices[-1 - indexB] : vertices[indexB];
        Vector3 positionC = (indexC < 0) ? borderVertices[-1 - indexC] : vertices[indexC];
        Vector3 sideAB = positionB - positionA;
        Vector3 sideAC = positionC - positionA;
        return Vector3.Cross(sideAB, sideAC).normalized;
    }
    public Mesh CreateMesh() //ONLY METHOD TO RUN ON MAIN THREAD!
    {
        Mesh mesh = new Mesh();
        mesh.indexFormat = IndexFormat.UInt32;
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.normals = bakedNormals;
        return mesh;
    }
    public void bakeNormals()
    {
        bakedNormals = CalcuateNormals();
    }
}
public static class MeshGenerator
{
    public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, AnimationCurve heightCurve, int levelOfDetail)
    {
        int borderedSize = heightMap.GetLength(0);
        int simplificationLevel = (levelOfDetail == 0) ? 1 : levelOfDetail * 2;
        int meshSize = borderedSize - 2*simplificationLevel;
        int meshSizeUnsimplified = borderedSize - 2;
        
        int[,] vertexIndexMap = new int[borderedSize, borderedSize];
        int borderedIndex = -1;
        int innerIndex = 0;
        for (int x = 0; x < borderedSize; x+=simplificationLevel)
            for (int y = 0; y < borderedSize; y+= simplificationLevel)
            {
                bool isBorderedVertex = y == 0 || y == borderedSize - 1 || x == 0 || x == borderedSize - 1;
                if (isBorderedVertex)
                    vertexIndexMap[x, y] = borderedIndex--;
                else
                    vertexIndexMap[x, y] = innerIndex++;
            }

        AnimationCurve curve = new AnimationCurve(heightCurve.keys);
        float topLeftX = (meshSizeUnsimplified - 1) / -2f;
        float topLeftZ = (meshSizeUnsimplified - 1) / 2f;
        
        int verticesPerRow = (borderedSize - 1) / simplificationLevel + 1;
        MeshData data = new MeshData(verticesPerRow);

        for (int z = 0; z < borderedSize; z += simplificationLevel)
        {
            for (int x = 0; x < borderedSize; x += simplificationLevel)
            {
                int index = vertexIndexMap[x, z];
                Vector2 percent = new Vector2((x - simplificationLevel) / (float)meshSize, (z - simplificationLevel) / (float)meshSize); //-simplificationLevel, so the actual inner mesh (not the border) starts at 0%
                float height = heightMultiplier * curve.Evaluate(heightMap[x, z]);
                Vector3 vertexPosition = new Vector3(topLeftX + percent.x * meshSizeUnsimplified, height, topLeftZ - percent.y * meshSizeUnsimplified); //
                data.AddVertex(index, vertexPosition, percent);
                if (x < borderedSize - 1 && z < borderedSize - 1)
                {
                    int a = vertexIndexMap[x, z];
                    int b = vertexIndexMap[x + simplificationLevel, z];
                    int c = vertexIndexMap[x, z + simplificationLevel];
                    int d = vertexIndexMap[x + simplificationLevel, z + simplificationLevel];
                    data.addTriangle(a, d, c);
                    data.addTriangle(d, a, b);
                }

            }
        }
        data.bakeNormals();
        return data;
    }
}