using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    public  AgentController a;
    public AgentController b;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Sensor.Check();
        Debug.Log(Sensor.CheckDirection(a.transform, b.transform)[0]);
    }
}
