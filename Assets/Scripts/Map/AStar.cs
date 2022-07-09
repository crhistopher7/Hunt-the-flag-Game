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

        while(openSet.Count > 0)
        {
            // Ordenar a lista pelo Score
            openSet.Sort((x, y) => x.Score.CompareTo(y.Score));
            current = openSet[0];

            if (current == objective)
            {
                TilesSearch.Add(current);
                //Debug.Log("Achou o Objetivo");
                break;
            }
            openSet.RemoveAt(0);
            TilesSearch.Add(current);

            for(int i = 0; i < MapGenerator.Directions.Length; i++)
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

    public void SearchOctile(LogicMap start, LogicMap objective, LogicMap deceptiveObjective = null)
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
                //Debug.Log("Achou o Objetivo");
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
                // Heuristica
                next.CostToObjective = deceptile(start, objective, deceptiveObjective, next);
                next.Score = next.CostToObjective + next.CostFromOrigin;

                if (!TilesSearch.Contains(next))
                {
                    openSet.Add(next);
                }
            }
        }
    }
    private float deceptile(LogicMap target, LogicMap objective, LogicMap deceptiveObjective, LogicMap adj)
    {
        var targetH = octile(adj, target);
        var realH = octile(adj, objective);
        var argminH = octile(adj, deceptiveObjective);
        if (realH < argminH)
            targetH = targetH * 1.5f;
        return targetH;
    }

    private float octile(LogicMap current, LogicMap objective)
    {
        var D = 1; // Custo Movimento normal
        var D2 = 2; // Custo Movimento Diagonal
        var dx = Mathf.Abs(current.ClickPosition.x - objective.ClickPosition.x);
        var dy = Mathf.Abs(current.ClickPosition.y - objective.ClickPosition.y);
        return D * (dx + dy) + (D2 - 2 * D) * Mathf.Min(dx, dy);
    }
}
