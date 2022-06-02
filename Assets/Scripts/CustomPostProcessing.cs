using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode, ImageEffectAllowedInSceneView]
public class CustomPostProcessing : MonoBehaviour
{
    public Material mat;
    Camera cam;
    void Awake()
    {
        cam = GetComponent<Camera>();
        cam.depthTextureMode = DepthTextureMode.Depth;
        
    }
    [ImageEffectOpaque]
    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        mat.SetMatrix("_ViewProjectInverse", (cam.projectionMatrix * cam.worldToCameraMatrix).inverse);
        Graphics.Blit(src, dest, mat);
    }
}
