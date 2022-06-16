using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum Distance
{
    VC = 1, C = 2, A = 3, F = 4, VF = 5
}

public enum Direction
{
    F = 10, RF = 20, LF = 30, R = 40, L = 50, B = 60, RB = 70, LB = 80
}

public enum Strategy
{
    DEFENSIVE, OFENSIVE
}

public enum DeceptiveLevel
{
    HIGHLY_DECEPTIVE, PARTIALLY_DECEPTIVE, LITTLE_DECEPTIVE, NOT_DECEPTIVE
}

public enum Sector
{
    DEFENSIVE, NEUTRAL, OFENSIVE
}

public enum PathType
{
    NORMAL, DECEPTIVE_1, DECEPTIVE_2, DECEPTIVE_3, DECEPTIVE_4
}

public static class CBDPUtils
{
    public static List<AgentController> OrderAgentList(List<AgentController> agents)
    {
        return agents.OrderBy(p => p.transform.position.x).ThenBy(p => p.transform.position.y).ToList();
    }

    public static Distance CalculeDistance(Vector3 a, Vector3 b)
    {
        //float distance = Vector3.Distance(a, b);
        float distance = GetDistanceByAStarPath(a, b);

        if (distance >= Constants.MAX_DISTANCE / 2)
            return Distance.VF;
        if (distance >= Constants.MAX_DISTANCE / 4)
            return Distance.F;
        if (distance >= Constants.MAX_DISTANCE / 8)
            return Distance.A;
        if (distance >= Constants.MAX_DISTANCE / 16)
            return Distance.C;

        return Distance.VC;
    }

    /// <summary>
    /// Função que calcula a distância de dois pontos a partir do path formado por um A*
    /// </summary>
    /// <param name="a">Posição de um objeto</param>
    /// <param name="b">Posição de um outro objeto</param>
    /// <returns>Retorna a soma da distância de cada ponto do path. Retorna o MaxValue caso não encontre um path</returns>
    public static float GetDistanceByAStarPath(Vector3 a, Vector3 b)
    {
        AStar AStar = GameObject.Find(Constants.PATHFINDER).GetComponent<AStar>();
        LogicMap pointA = AStar.GetTileByPosition(new Vector3Int((int) a.x, (int) a.y, 0) / Constants.MAP_OFFSET);
        LogicMap pointB = AStar.GetTileByPosition(new Vector3Int((int) b.x, (int) b.y, 0) / Constants.MAP_OFFSET);

        AStar.Search(pointA, pointB);
        List<LogicMap>  path = AStar.BuildPath(pointB);

        if (path.Count == 1)
            return float.MaxValue;

        float distance = 0;

        for (int i = 0; i < path.Count - 1; i++)
            distance += Vector3.Distance(path[i].ClickPosition, path[i + 1].ClickPosition);

        return distance;
    }

    public static Direction CalculeDirection(float x, float y)
    {
        if (x > -0.5 && x < 0.5)
        {
            if (y == 1)
            {
                return Direction.F;
            }
            else
            {
                return Direction.B;
            }
        }

        if (y > -0.5 && y < 0.5)
        {
            if (x == 1)
            {
                return Direction.R;
            }
            else
            {
                return Direction.L;
            }
        }

        if (x >= 0.5)
        {
            if (y >= 0.5)
            {
                return Direction.RF;
            }
            else
            {
                return Direction.RB;
            }
        }

        else
        {
            if (y >= 0.5)
            {
                return Direction.LF;
            }
            else
            {
                return Direction.LB;
            }
        }
    }
}
