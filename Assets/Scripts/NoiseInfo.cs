

using System;
using UnityEngine;
using UnityEngine.Assertions.Must;

namespace Assets.Scripts
{
    [Serializable]
    public static class Noise
    {
        public enum NormalizeMode {Local,Global };

        public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset,NormalizeMode normalizeMode,float globalEstimateDiv)
        {
            System.Random prng = new System.Random(seed);
            Vector2[] octaveOffsets = new Vector2[octaves];
            float frequency = 1;
            float amplitude = 1;
            float maxHeightValEstimate = 0;
            for (int i = 0; i < octaves; i++)
            {
                octaveOffsets[i] = new Vector2(prng.Next(-1000, 1000) + offset.x, prng.Next(-1000, 1000) + offset.y);
                maxHeightValEstimate += amplitude;
                amplitude *= persistance;
            }
            float maxNoiseHeight = float.MinValue;
            float minnoiseHeight = float.MaxValue;
            float halfWidth = mapWidth / 2f;
            float halfHeight = mapHeight / 2f;
            float[,] noiseMap = new float[mapWidth, mapHeight];
            if (scale <= 0)
                scale = 0.00001f;
            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    frequency = 1;
                    amplitude = 1;
                    float height = 0;
                    for (int i = 0; i < octaves; i++)
                    {
                        float sampleX = (float)(x-halfWidth + octaveOffsets[i].x) / scale * frequency ;
                        float sampleY = (float)(y-halfHeight- octaveOffsets[i].y) / scale * frequency ;
                        float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                        height += perlinValue * amplitude;
                        amplitude *= persistance;
                        frequency *= lacunarity;
                    }
                    if (height < minnoiseHeight)
                    {
                        minnoiseHeight = height;
                    }
                    if (height > maxNoiseHeight)
                        maxNoiseHeight = height;
                    noiseMap[x, y] = height;
                }
            }
            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    if(normalizeMode == NormalizeMode.Local)
                        noiseMap[x, y] = Mathf.InverseLerp(minnoiseHeight, maxNoiseHeight, noiseMap[x, y]);
                    else if(normalizeMode==NormalizeMode.Global)
                    {
                        noiseMap[x,y]=((noiseMap[x, y] + 1) / (2 * maxHeightValEstimate / globalEstimateDiv));
                        noiseMap[x, y] = Mathf.Clamp(noiseMap[x, y], 0, float.MaxValue);
                    }
                }
            }
            return noiseMap;
        }
    }
}
