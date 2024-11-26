using Assets.Scripts.CBDP;
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
    F = 1, RF = 2, LF = 3, R = 4, L = 5, B = 6, RB = 7, LB = 8
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

    public static IEnumerable<T> Flatten<T>(this T[,] matrix)
    {
        foreach (var item in matrix) yield return item;
    }

    public static List<Qualitative> ToQualitative(this IEnumerable<string> vector)
    {
        List<Qualitative> list = new List<Qualitative>();
        foreach (var item in vector)
        {
            if(item != "")
                list.Add(new Qualitative(item));
                
        }

        return list;
    }

    public static List<Qualitative> Intersection(List<Qualitative> list1, List<Qualitative> list2)
    {
        IEnumerable<string> strs = list1.Select(i => i.ToString()).Intersect(list2.Select(i => i.ToString()));
        return list1.Where(x => strs.Contains(x.ToString())).ToList();
    }

    public static List<GameObject> Filter(GameObject[] array, string criteria)
    {
        List<GameObject> list = new List<GameObject>();
        foreach (var item in array)
        {
            if (item.name.Contains(criteria))
                list.Add(item);
        }
        return list;
    }

    /// <summary>
    /// Função que reescreve uma matriz[,] do tipo string em forma de string
    /// </summary>
    /// <param name="matrix">A matriz[,] do tipo string</param>
    /// <param name="delimiter">Delimitador</param>
    /// <returns>Matrix no formato de string</returns>
    public static string ToMatrixString(Qualitative[,] matrix, bool num = false, string delimiter = ",")
    {
        string s = "{";

        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            s += "{";
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                s += matrix[i, j].ToString(num) + (delimiter);
            }
            s = s.Remove(s.Length - 1, 1);
            s += "}:";
        }
        s = s.Remove(s.Length - 1, 1);

        return s += "}";
    }

    public static Distance CalculeDistance(Vector3 a, Vector3 b, bool useAStar = false, bool verbose = false, string description = null)
    {
        float distance; 
        if (useAStar)
            distance = GetDistanceByAStarPath(a, b);
        else
            distance = Vector3.Distance(a, b);

        if (verbose)
        {
            if (useAStar)
                Debug.Log("AStar distance between " + description + " is " + distance);
            else
                Debug.Log("Distance between " + description + " is " + distance);
        }


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

        return AStar.GetJustCost(pointA, pointB); 
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

    public static string[,] StringToMatrix(string str)
    {
        string[,] matrix;

        // Removendo o {} de fora da matriz
        string aux = str.Remove(0, 1);
        aux = aux.Remove(aux.Length - 1, 1);

        // Separando em linhas
        var lines = aux.Split(':');

        // Pegando a quantidade de valores que tem na matrix
        var columnsSize = CountOccurrences(lines[0], ',');

        // Iniciando a matriz
        matrix = new string[lines.Length, columnsSize];

        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            // Exemplo: { VF - RB,A - LB,,,,,,,,,}

            var line = lines[i];

            // Removendo o {}
            line = line.Remove(0, 1);
            line = line.Remove(line.Length - 1, 1);

            var values = line.Split(',');

            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                matrix[i, j] = values[j];
            }
        }

        return matrix;
    }

    private static int CountOccurrences(string str, char delimiter)
    {
        int count = 0;
        foreach (char c in str)
            if (c == delimiter) count++;

        return count;
    }

    public static float Octile(Vector2 a, Vector2 b)
    {
        var dx = Mathf.Abs(a.x - b.x);
        var dy = Mathf.Abs(a.y - b.y);

        return Mathf.Max(dx, dy) + ((Mathf.Sqrt(2) - 1) * Mathf.Min(dx, dy));
    }


    public static float Euclidian(Vector2 a, Vector2 b)
    {
        return Vector2.Distance(a, b);
    }

    public static Vector2 PointByDistanceAndAngle(double angle, double distance, Vector2 point)
    {
        // Co (x) = sen * D
        float x = (float)(Math.Sin(angle) * distance);
        // Ca (y) = cos * D
        float y = (float)(Math.Cos(angle) * distance);

        //offset em relação ao ponto
        x += point.x;
        y += point.y;

        return new Vector2(x,y);
    }
}
