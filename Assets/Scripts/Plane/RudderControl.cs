using MFlight.Demo;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Plane = MFlight.Demo.Plane;

public class RudderControl : MonoBehaviour
{
    Plane plane;
    public float angleRange;
    // Start is called before the first frame update
    void Start()
    {
        plane = FindObjectOfType<Plane>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.localRotation = Quaternion.Euler(0, angleRange*plane.Yaw, 0);
    }
}
