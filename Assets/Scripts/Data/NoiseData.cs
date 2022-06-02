using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu()]
public class NoiseData : UpdatableData
{
    [Range(0f, 1f)]
    public float persistance;
    public float lacunarity;
    public float noiseNormalizationEstimationMult;
    public int seed;
    public float noiseScale;
    public int octaves;
    public Vector2 offset;
#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        if (noiseScale < 0)
            noiseScale = 0;
        if (octaves < 0)
            octaves = 0;
        if (lacunarity < 1)
            lacunarity = 1;
        
    }
#endif
}
