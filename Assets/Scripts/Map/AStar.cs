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
                if (next == null)
                    continue;

                iterationCount++;

                var vNext = new Vector3(next.ClickPosition.x, next.ClickPosition.y, next.heigth);
                var vCurrent = new Vector3(current.ClickPosition.x, current.ClickPosition.y, current.heigth);

                var h = Vector3.Distance(vNext, vCurrent);
                if (next.CostFromOrigin <= current.CostFromOrigin + h)
                    continue;

                next.CostFromOrigin = current.CostFromOrigin + h;
                next.Previous = current;

                var vObjective = new Vector3(objective.ClickPosition.x, objective.ClickPosition.y, objective.heigth);

                // Heuristica
                next.CostToObjective = Vector3.Distance(vNext, vObjective) * Constants.HEURISTIC_MULTIPLIER;
                next.Score = next.CostFromOrigin + next.CostToObjective;

                if (!TilesSearch.Contains(next))
                {
                    openSet.Add(next);
                }
            }
        }
    }

    public void SearchAstarCustom3(LogicMap start, LogicMap deceptiveObjective, LogicMap objective)
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
                if (next == null)
                    continue;
                iterationCount++;

                var vNext = new Vector3(next.ClickPosition.x, next.ClickPosition.y, next.heigth);
                var vCurrent = new Vector3(current.ClickPosition.x, current.ClickPosition.y, current.heigth);

                var h = Vector3.Distance(vNext, vCurrent);
                if (next.CostFromOrigin <= current.CostFromOrigin + h)
                    continue;

                next.CostFromOrigin = current.CostFromOrigin + h;
                next.Previous = current;

                next.CostToObjective = Deceptile(start, objective, deceptiveObjective, next);
                next.Score = next.CostToObjective + next.CostFromOrigin;

                if (!TilesSearch.Contains(next))
                {
                    openSet.Add(next);
                }
            }
        }
    }

    public void SearchAstarCustom4(LogicMap start, LogicMap objective, LogicMap objectiveReal, LogicMap deceptiveObjective,  float costReal, float costDeceptive)
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
                if (next == null)
                    continue;
                iterationCount++;


                var vNext = new Vector3(next.ClickPosition.x, next.ClickPosition.y, next.heigth);
                var vCurrent = new Vector3(current.ClickPosition.x, current.ClickPosition.y, current.heigth);

                var h = Vector3.Distance(vNext, vCurrent);
                if (next.CostFromOrigin <= current.CostFromOrigin + h)
                    continue;

                next.CostFromOrigin = current.CostFromOrigin + h;
                next.Previous = current;

                var vObjective = new Vector3(objective.ClickPosition.x, objective.ClickPosition.y, objective.heigth);

                // Heuristica
                next.CostToObjective = Vector3.Distance(vNext, vObjective) * Constants.HEURISTIC_MULTIPLIER;
                next.Score = next.CostToObjective + next.CostFromOrigin;

                
                if (CheckIsDeceptive(next, objectiveReal, deceptiveObjective, costReal, costDeceptive))
                    openSet.Add(next);
            }
        }
    }

    private bool CheckIsDeceptive(LogicMap next, LogicMap objectiveReal, LogicMap deceptiveObjective, float costReal, float costDeceptive)
    {
        if (next.isDeceptive)
            return true;

        // pegar o custo entre o next e o objetivo real      - costReal
        var cost1 = GetJustCost(next, objectiveReal) - costReal;

        // pegar o custo entre o next e o objetivo enganoso  - costDeceptive
        var cost2 = GetJustCost(next, deceptiveObjective) - costDeceptive;

        // se custo enganoso for maior que o custo real, ele é enganoso
        if (cost2 > cost1)
            next.isDeceptive = true;

        return next.isDeceptive;
    }

    private float GetJustCost(LogicMap s, LogicMap o)
    {
        List<LogicMap> TilesSearch = new List<LogicMap>();
        List<LogicMap> openSet = new List<LogicMap>();

        var navMap = MapGenerator.CopyOfNavMap();

        navMap.TryGetValue(s.ClickPosition, out LogicMap start);
        navMap.TryGetValue(o.ClickPosition, out LogicMap objective);

        Debug.Log("start.CostFromOrigin" + start.CostFromOrigin.ToString());
        Debug.Log("s.CostFromOrigin" + s.CostFromOrigin.ToString());


        if (!objective.Walkable)
        {
            Debug.Log("O objetivo não pode ser alcançado");
            return objective.Score;
        }

        LogicMap current;
        
        openSet.Add(start);
        start.CostFromOrigin = 0;

        Debug.Log("start.CostFromOrigin pos" + start.CostFromOrigin.ToString());
        Debug.Log("s.CostFromOrigin pos" + s.CostFromOrigin.ToString());

        while (openSet.Count > 0)
        {
            // Ordenar a lista pelo Score
            openSet.Sort((x, y) => x.Score.CompareTo(y.Score));
            current = openSet[0];

            if (current == objective)
            {
                TilesSearch.Add(current);
                break;
            }
            openSet.RemoveAt(0);
            TilesSearch.Add(current);

            for (int i = 0; i < MapGenerator.Directions.Length; i++)
            {
                navMap.TryGetValue(current.ClickPosition + MapGenerator.Directions[i], out LogicMap next);
                if (next == null)
                    continue;

                var vNext = new Vector3(next.ClickPosition.x, next.ClickPosition.y, next.heigth);
                var vCurrent = new Vector3(current.ClickPosition.x, current.ClickPosition.y, current.heigth);

                var h = Vector3.Distance(vNext, vCurrent);
                if (next.CostFromOrigin <= current.CostFromOrigin + h)
                    continue;

                next.CostFromOrigin = current.CostFromOrigin + h;
                next.Previous = current;

                var vObjective = new Vector3(objective.ClickPosition.x, objective.ClickPosition.y, objective.heigth);

                // Heuristica
                next.CostToObjective = Vector3.Distance(vNext, vObjective) * Constants.HEURISTIC_MULTIPLIER;
                next.Score = next.CostFromOrigin + next.CostToObjective;

                if (!TilesSearch.Contains(next))
                {
                    openSet.Add(next);
                }
            }
        }

        var path = BuildPath(objective);
        var cost = CostPath(path);

        return cost;
    }

    private float Deceptile(LogicMap start, LogicMap objective, LogicMap deceptiveObjective, LogicMap adj)
    {
        var targetH = Euclidian(adj, start);
        var realH = Euclidian(adj, objective);
        var argminH = Euclidian(adj, deceptiveObjective);
        if (realH < argminH)
            targetH *= 1.5f * Constants.HEURISTIC_MULTIPLIER;
        return targetH;
    }

    private float Octile(LogicMap current, LogicMap objective)
    {
        var dx = Mathf.Abs(current.Position.x - objective.Position.x);
        var dy = Mathf.Abs(current.Position.y - objective.Position.y);

        return Mathf.Max(dx, dy) + ((Mathf.Sqrt(2) - 1) * Mathf.Min(dx, dy));
    }


    private float Euclidian(LogicMap current, LogicMap objective)
    {
        var vCurrent = new Vector3(current.ClickPosition.x, current.ClickPosition.y, current.heigth);
        var vObjective = new Vector3(objective.ClickPosition.x, objective.ClickPosition.y, objective.heigth);

        return Vector3.Distance(vCurrent, vObjective);
    }
}
