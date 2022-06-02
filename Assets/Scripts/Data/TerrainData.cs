using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu()]
public class TerrainData : UpdatableData
{
    public float uniformScale;
    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;
    public float minHeight
    {
        get
        {
            return uniformScale * meshHeightMultiplier * meshHeightCurve.Evaluate(0);
        }
    }
    public float maxHeight
    {
        get
        {
            return uniformScale * meshHeightMultiplier * meshHeightCurve.Evaluate(1);
        }
    }
}
