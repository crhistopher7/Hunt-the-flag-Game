using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Pathfinder : MonoBehaviour
{
    public Vector3Int InitialPosition;
    public Vector3Int ObjectivePosition;
    public int SearchLength;

    protected MapGenerator MapGenerator;

    protected List<LogicMap> TilesSearch;

    private void Start()
    {
        MapGenerator = GameObject.Find("Map Generator").GetComponent<MapGenerator>();
    }


    [ContextMenu("Search")]
    void TriggerSearch()
    {
       Search(MapGenerator.GetTile(InitialPosition), MapGenerator.GetTile(ObjectivePosition));
    }

    [ContextMenu("Print Path")]
    void TriggerPrintPath()
    {
        LogicMap objective = MapGenerator.GetTile(ObjectivePosition);

        if (TilesSearch.Contains(objective))
        {
            List<LogicMap> path = BuildPath(objective);
            PrintPath(path);
        }
        else
        {
            Debug.Log("Objetivo não encontrado");
        }
    }

    // Métodos
    public abstract void Search(LogicMap start, LogicMap objective);
    
    public void PrintPath(List<LogicMap> path)
    {
        foreach (LogicMap t in path)
        {
            Debug.Log(t.Position);
        }
    }

    public List<LogicMap> BuildPath(LogicMap objective)
    {
        List<LogicMap> path = new List<LogicMap>();
        LogicMap temp = objective;

        while (temp.Previous != null)
        {
            path.Add(temp);
            temp = temp.Previous;
        }
        path.Add(temp);
        path.Reverse();
        return path;
    }
}
