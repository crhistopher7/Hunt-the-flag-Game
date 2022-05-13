using System.Collections;
using System.Collections.Generic;
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
    public static Distance CalculeDistance(Vector3 a, Vector3 b)
    {
        float distance = Vector3.Distance(a, b);

        if (distance >= Config.MAX_DISTANCE / 2)
        {
            return Distance.VF;
        }

        if (distance >= Config.MAX_DISTANCE / 4)
        {
            return Distance.F;
        }

        if (distance >= Config.MAX_DISTANCE / 8)
        {
            return Distance.A;
        }

        if (distance >= Config.MAX_DISTANCE / 16)
        {
            return Distance.C;
        }

        return Distance.VC;
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
