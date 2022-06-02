using System;
using UnityEngine;

public class AIPlane : MonoBehaviour
{
    public float damping = 6.0f;
    // Start is called before the first frame update\
    public Transform currentTarget;

    //TODO
    public float velocity;
    public float fireThr;

    public Transform getTarget()
    {
        //TODO
        return currentTarget;
    }
   
    public GameObject bulletPrefab;
    MachineGunFire[] machineGuns;
    Rigidbody rb;
    void Start()
    {
        machineGuns = transform.GetComponentsInChildren<MachineGunFire>();
        rb = GetComponent<Rigidbody>();
        
    }

    // Update is called once per frame
    private Quaternion Damp(Quaternion a, Quaternion b, float lambda, float dt)
    {
        return Quaternion.Slerp(a, b, 1 - Mathf.Exp(-lambda * dt));
    }
    void Update()
    {
        Vector3 tanget = Vector3.Cross(currentTarget.position - transform.position, transform.forward);
        Vector3 upDir = Vector3.Cross(tanget, transform.forward);
        Quaternion rotation = Quaternion.LookRotation(currentTarget.position - transform.position,upDir);
        transform.rotation = Damp(transform.rotation,rotation,damping,Time.deltaTime);
        rb.velocity = transform.forward * velocity;

        float forwardDistToTar = Vector3.Dot((currentTarget.position - transform.position).normalized, transform.forward);
        
        if (forwardDistToTar > fireThr)
        {
            shoot();
        }
    }
    public void shoot()
    {
        foreach (MachineGunFire gun in machineGuns)
        {
            gun.Fire(bulletPrefab);
        }
    }
}
