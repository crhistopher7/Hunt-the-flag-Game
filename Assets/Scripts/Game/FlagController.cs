using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlagController : MonoBehaviour
{
    public string team;
    public int value;
    public Vector3 Agentposition;
    public Vector3 StartFlagposition;
    public float agentSpeed;
    public bool beingCarried;

    // Start is called before the first frame update
    void Start()
    {
        transform.position = StartFlagposition;
        beingCarried = false;
    }

    void Update()
    {
        if (beingCarried)
        {
            Move();
        }
    }

    void Move()
    {
        Vector3 targetDirection = Agentposition - transform.position;
        transform.position = Vector3.MoveTowards(transform.position, targetDirection, agentSpeed * Time.fixedDeltaTime);
    }

    internal void RestartPosition()
    {
        transform.position = StartFlagposition;
        beingCarried = false;
    }
}
