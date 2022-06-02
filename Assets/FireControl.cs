using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireControl : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject bulletPrefab;
    MachineGunFire[] machineGuns;

    void Start()
    {
        machineGuns = transform.GetComponentsInChildren<MachineGunFire>();

    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (Input.GetKey(KeyCode.Space))
            shoot();
    }
    public void shoot()
    {
        foreach (MachineGunFire gun in machineGuns)
        {
            gun.Fire(bulletPrefab);
        }
    }
}
