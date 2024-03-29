using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogicMap : ICloneable
{
    public Vector3Int Position;
    public Vector3Int ClickPosition;
    public bool Walkable;
    public bool isDeceptive;
    public int ColorMapIndex;
    public float CostFromOrigin;
    public float CostToObjective;
    public float Score;
    public int MoveCost;
    public float heigth;
    public LogicMap Previous;

    public bool InArea(List<Vector2> area)
    {
        float x = this.Position.x;
        float y = this.Position.y;

        return x >= area[0].x && x <= area[1].x && y >= area[0].y && y <= area[1].y;
    }

    public object Clone()
    {
        LogicMap logicMap = new LogicMap
        {
            Position = this.Position,
            ClickPosition = this.ClickPosition,
            Walkable = this.Walkable,
            MoveCost = this.MoveCost,
            heigth = this.heigth,
            ColorMapIndex = this.ColorMapIndex,
            isDeceptive = false,
            CostFromOrigin = int.MaxValue,
            CostToObjective = int.MaxValue,
            Score = int.MaxValue,
            Previous = null
        };
        return logicMap;
    }
}
