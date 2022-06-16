using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogicMap
{
    public Vector3Int Position;
    public Vector3Int ClickPosition;
    public bool Walkable;
    public int ColorMapIndex;
    public float CostFromOrigin;
    public float CostToObjective;
    public float Score;
    public int MoveCost;
    public float heigth;
    public LogicMap Previous;
}
