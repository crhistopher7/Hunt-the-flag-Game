using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// CBDP Action class used in Plan
/// </summary>
public class Action
{
    public string action;
    public string agent;
    public string objetive;
    public DeceptiveLevel actionDefinition;
    public PathType pathType;
    public string distance_direction;
    public string distance_directionDeceptive;
    public int time;

    public override string ToString()
    {
        return action + "," + agent + "," + objetive + "," + actionDefinition + "," +
            pathType + "," + distance_direction + "," + distance_directionDeceptive + "," + time;
    }
}