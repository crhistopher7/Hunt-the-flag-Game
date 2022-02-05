using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlagController : MatchBehaviour
{
    public string team;
    public int value;
    public Vector3 Agentposition;
    public Vector3 StartFlagposition;
    public float agentSpeed;
    Rigidbody rb;
    public bool beingCarried;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.MovePosition(StartFlagposition);
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
        Vector3 targetDirection = Agentposition - rb.position;
        rb.MovePosition(rb.position + (targetDirection * Time.fixedDeltaTime * agentSpeed));
    }

    internal void RestartPosition()
    {
        rb.MovePosition(StartFlagposition);
        rb.angularVelocity.Set(0, 0, 0);
        beingCarried = false;
    }
}
