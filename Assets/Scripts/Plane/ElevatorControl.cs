using UnityEngine;
using Plane = MFlight.Demo.Plane;

public class ElevatorControl : MonoBehaviour
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
        transform.localRotation = Quaternion.Euler( angleRange * plane.Pitch,0, 0);
    }
}
