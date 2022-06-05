using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeceptiveAStar_2 : Pathfinder
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
                next.CostToObjective = Vector3Int.Distance(next.ClickPosition, objective.ClickPosition) * 10;
                next.Score = next.CostToObjective + next.CostFromOrigin;

                if (!TilesSearch.Contains(next))
                {
                    openSet.Add(next);
                }
            }
        }
    }

}
