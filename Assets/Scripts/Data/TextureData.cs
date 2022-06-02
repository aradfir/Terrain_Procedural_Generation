using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
[CreateAssetMenu()]
public class TextureData : UpdatableData
{
    public Layer[] layers;
    float savedMinHeight;
    float savedMaxHeight;
    
    public void ApplyToMaterial(Material mat)
    {
        mat.SetColorArray("baseColors", layers.Select(x=>x.tint).ToArray());
        mat.SetFloatArray("baseColorsStrength", layers.Select(x => x.tintStrength).ToArray());
        mat.SetFloatArray("baseTextureScale", layers.Select(x => x.textureScale).ToArray());
        mat.SetFloatArray("baseStartHeights", layers.Select(x => x.startHeight).ToArray());
        mat.SetFloatArray("baseBlendHeights", layers.Select(x => x.blendStrength).ToArray());
        mat.SetInt("layerCount", layers.Length);
        Texture2DArray texture2DArray = GenerateTextureArray(layers.Select(x => x.texture).ToArray());
        mat.SetTexture("baseTextures", texture2DArray);
        UpdateMeshHeights(mat, savedMinHeight, savedMaxHeight);
    }
    public Texture2DArray GenerateTextureArray(Texture2D[] textures)
    {
        Texture2DArray texture2DArray = new Texture2DArray(textures[0].width, textures[0].height, textures.Length, textures[0].format, true);
        for (int i = 0; i < textures.Length; i++)
            texture2DArray.SetPixels(textures[i].GetPixels(), i);
        texture2DArray.Apply();
        return texture2DArray;
    }
    public void UpdateMeshHeights(Material material, float minHeight,float maxHeight)
    {
        savedMaxHeight = maxHeight;
        savedMinHeight = minHeight;
        
        material.SetFloat("minHeight", minHeight);
        material.SetFloat("maxHeight", maxHeight);
        
    }
    [System.Serializable]
    public class Layer
    {
        public Texture2D texture;
        public Color tint;
        [Range(0,1)]
        public float tintStrength;
        [Range(0, 1)]
        public float startHeight;
        [Range(0, 1)]
        public float blendStrength;
        public float textureScale;
    }
}

