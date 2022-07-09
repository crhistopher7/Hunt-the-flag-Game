using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStar : Pathfinder
{
    public override void Search(LogicMap start, LogicMap objective, LogicMap deceptiveObjective = null)
    {
        int iterationCount = 0;
        MapGenerator.ClearSearch();
        TilesSearch = new List<LogicMap>();

        if (!objective.Walkable)
        {
            Debug.Log("O objetivo não pode ser alcançado");
            return;
        }

        LogicMap current;

        List<LogicMap> openSet = new List<LogicMap>();
        openSet.Add(start);
        start.CostFromOrigin = 0;

        while (openSet.Count > 0)
        {
            // Ordenar a lista pelo Score
            openSet.Sort((x, y) => x.Score.CompareTo(y.Score));
            current = openSet[0];

            if (current == objective)
            {
                TilesSearch.Add(current);
                Debug.Log("Achou o Objetivo: iteracoes=" + iterationCount.ToString());
                break;
            }
            openSet.RemoveAt(0);
            TilesSearch.Add(current);

            for (int i = 0; i < MapGenerator.Directions.Length; i++)
            {
                LogicMap next = MapGenerator.GetTile(current.ClickPosition + MapGenerator.Directions[i]);
                iterationCount++;


                if (next == null || next.CostFromOrigin <= current.CostFromOrigin + next.MoveCost)
                    continue;

                next.CostFromOrigin = current.CostFromOrigin + next.MoveCost;
                next.Previous = current;

                var v1 = new Vector3(next.ClickPosition.x, next.ClickPosition.y, next.heigth);
                var v2 = new Vector3(objective.ClickPosition.x, objective.ClickPosition.y, objective.heigth);

                // Heuristica
                next.CostToObjective = Vector3.Distance(v1, v2) * Constants.HEURISTIC_MULTIPLIER;
                next.Score = next.CostToObjective + next.CostFromOrigin;

                if (!TilesSearch.Contains(next))
                {
                    openSet.Add(next);
                }
            }
        }
    }

    public void SearchOctile(LogicMap start, LogicMap deceptiveObjective, LogicMap objective)
    {
        int iterationCount = 0;
        MapGenerator.ClearSearch();
        TilesSearch = new List<LogicMap>();

        if (!objective.Walkable)
        {
            Debug.Log("O objetivo não pode ser alcançado");
            return;
        }

        LogicMap current;

        List<LogicMap> openSet = new List<LogicMap>();
        openSet.Add(start);
        start.CostFromOrigin = 0;

        while (openSet.Count > 0)
        {
            // Ordenar a lista pelo Score
            openSet.Sort((x, y) => x.Score.CompareTo(y.Score));
            current = openSet[0];

            if (current == objective)
            {
                TilesSearch.Add(current);
                Debug.Log("Achou o Objetivo: iteracoes=" + iterationCount.ToString());
                break;
            }
            openSet.RemoveAt(0);
            TilesSearch.Add(current);

            for (int i = 0; i < MapGenerator.Directions.Length; i++)
            {
                LogicMap next = MapGenerator.GetTile(current.ClickPosition + MapGenerator.Directions[i]);
                iterationCount++;


                if (next == null || next.CostFromOrigin <= current.CostFromOrigin + next.MoveCost)
                    continue;

                next.CostFromOrigin = current.CostFromOrigin + next.MoveCost;
                next.Previous = current;
                next.CostToObjective = deceptile(start, objective, deceptiveObjective, next);
                next.Score = next.CostToObjective + next.CostFromOrigin;

                if (!TilesSearch.Contains(next))
                {
                    openSet.Add(next);
                }
            }
        }
    }

    public void SearchAstarCustom4(LogicMap start, LogicMap deceptiveObjective)
    {
        int iterationCount = 0;
        MapGenerator.ClearSearch();
        TilesSearch = new List<LogicMap>();

        if (!deceptiveObjective.Walkable)
        {
            Debug.Log("O objetivo não pode ser alcançado");
            return;
        }

        LogicMap current;

        List<LogicMap> openSet = new List<LogicMap>();
        openSet.Add(start);
        start.CostFromOrigin = 0;

        while (openSet.Count > 0)
        {
            // Ordenar a lista pelo Score
            openSet.Sort((x, y) => x.Score.CompareTo(y.Score));
            current = openSet[0];

            if (current == deceptiveObjective)
            {
                TilesSearch.Add(current);
                Debug.Log("Achou o Objetivo: iteracoes=" + iterationCount.ToString());
                break;
            }
            openSet.RemoveAt(0);
            TilesSearch.Add(current);

            for (int i = 0; i < MapGenerator.Directions.Length; i++)
            {
                LogicMap next = MapGenerator.GetTile(current.ClickPosition + MapGenerator.Directions[i]);
                iterationCount++;


                if (next == null || next.CostFromOrigin <= current.CostFromOrigin + next.MoveCost)
                    continue;

                next.CostFromOrigin = current.CostFromOrigin + next.MoveCost;
                next.Previous = current;

                var v1 = new Vector3(next.ClickPosition.x, next.ClickPosition.y, next.heigth);
                var v2 = new Vector3(deceptiveObjective.ClickPosition.x, deceptiveObjective.ClickPosition.y, deceptiveObjective.heigth);

                // Heuristica
                next.CostToObjective = Vector3.Distance(v1, v2) * Constants.HEURISTIC_MULTIPLIER;
                next.Score = next.CostToObjective + next.CostFromOrigin;

                openSet.Add(next);
            }
        }
    }
    private float deceptile(LogicMap start, LogicMap objective, LogicMap deceptiveObjective, LogicMap adj)
    {
        var targetH = octile(adj, start);
        var realH = octile(adj, objective);
        var argminH = octile(adj, deceptiveObjective);
        if (realH < argminH)
            targetH = targetH * 1f;
        return targetH;
    }

    private float octile(LogicMap current, LogicMap objective)
    {
        var dx = Mathf.Abs(current.Position.x - objective.Position.x);
        var dy = Mathf.Abs(current.Position.y - objective.Position.y);

        return Mathf.Max(dx, dy) + ((Mathf.Sqrt(2) - 1) * Mathf.Min(dx, dy));
    }
}
