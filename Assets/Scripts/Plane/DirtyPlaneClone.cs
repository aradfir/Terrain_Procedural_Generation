using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirtyPlaneClone : MonoBehaviour
{
    public Transform truePlane;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        this.transform.position = truePlane.position;
        this.transform.rotation = truePlane.rotation * Quaternion.Euler(0, -90, 0);
    }
}
