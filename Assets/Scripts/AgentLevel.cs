using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentLevel
{
    public int level;
    public int life;
    public float speed;
    public int sensorMultiplier;


    public AgentLevel(int requestLevel)
    {
        switch (requestLevel)
        {
            case 1:
                this.level = 1;
                this.life = 100;
                this.speed = 40.0f;
                this.sensorMultiplier = 20;
                break;

            case 2:
                this.level = 2;
                this.life = 120;
                this.speed = 45.0f;
                this.sensorMultiplier = 25;
                break;

            case 3:
                this.level = 3;
                this.life = 140;
                this.speed = 50.0f;
                this.sensorMultiplier = 30;
                break;

            default:
                break;
        }
    }
}
