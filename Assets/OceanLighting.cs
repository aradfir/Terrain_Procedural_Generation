using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class OceanLighting : MonoBehaviour
{
    Light lightSource;
    public Material oceanShaderMat;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (lightSource == null)
            lightSource = GetComponent<Light>();
        
        oceanShaderMat.SetVector("dirToSun", -lightSource.transform.forward); 
    }
}
