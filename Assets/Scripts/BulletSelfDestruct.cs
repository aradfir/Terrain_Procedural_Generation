using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletSelfDestruct : MonoBehaviour
{
    public float selfDestructTimer;
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.transform.name);
        destroy();
    }
     void destroy()
    {
        Object.Destroy(this.gameObject);
    }
    // Start is called before the first frame update
    void Start()
    {
        Invoke("destroy", selfDestructTimer);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
