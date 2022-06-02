
using UnityEngine;

public class MachineGunFire : MonoBehaviour
{
    public float bulletVelocity;
    public float fireRate;
    GameObject parentPlane;
    Rigidbody parentRB;
    // Start is called before the first frame update
    void Start()
    {
        parentPlane = transform.parent.gameObject;
        parentRB = parentPlane.GetComponent<Rigidbody>();
    }
    public void Fire(GameObject bulletPrefab)
    {
        if (!enabled)
            return;

        GameObject bullet = GameObject.Instantiate<GameObject>(bulletPrefab, transform.position, parentPlane.transform.rotation * Quaternion.Euler(0, 0, 0));
        //bullet.GetComponent<Rigidbody>().AddForce((bulletVelocity + Vector3.Dot(parentRB.velocity, parentPlane.transform.forward)) * parentPlane.transform.forward, ForceMode.VelocityChange);
        bullet.GetComponent<Rigidbody>().AddForce(bulletVelocity * parentPlane.transform.forward + parentRB.velocity, ForceMode.VelocityChange);
        enabled = false;
        Invoke("enable", fireRate);
    }

    private void enable()
    {
        this.enabled = true;
    }
    // Update is called once per frame
    void Update()
    {

    }
}
