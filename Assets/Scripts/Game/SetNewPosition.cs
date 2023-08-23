using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetNewPosition : MonoBehaviour
{
    public bool reposition;

    // Start is called before the first frame update
    void Start()
    {
        reposition = false;
    }

    void Update()
    {
        if (reposition)
        {
            Move();
        }
    }

    void Move()
    {
        Vector3 newp = new Vector3(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y, transform.position.z);
        transform.position = newp;
    }
}
