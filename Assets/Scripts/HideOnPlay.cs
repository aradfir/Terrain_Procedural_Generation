﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideOnPlay : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        gameObject.active = false;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
