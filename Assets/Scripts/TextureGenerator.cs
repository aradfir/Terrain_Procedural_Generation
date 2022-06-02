using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureGenerator
{
    // Start is called before the first frame update
    public static Texture2D GenerateTextureFromHeightMap(float[,] heightMap)
    {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        Color[] colorMap = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                colorMap[y * width + x] = Color.Lerp(Color.black, Color.white, heightMap[x, y]);
            }
        }
        return GenerateTextureFromColorMap(colorMap, width, height);
    }
    public static Texture2D GenerateTextureFromColorMap(Color[] colorMap,int width,int height)
    {
        Texture2D texture = new Texture2D(width, height);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Point;
        texture.SetPixels(colorMap);
        texture.Apply();
        return texture;
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
