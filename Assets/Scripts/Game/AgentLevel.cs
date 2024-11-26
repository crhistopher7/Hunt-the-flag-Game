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
                this.speed = 60.0f;
                this.sensorMultiplier = 10;
                break;

            case 2:
                this.level = 2;
                this.life = 120;
                this.speed = 70.0f;
                this.sensorMultiplier = 10;
                break;

            case 3:
                this.level = 3;
                this.life = 140;
                this.speed = 80.0f;
                this.sensorMultiplier = 10;
                break;

            default:
                break;
        }
    }
}
