using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Pathfinder : MonoBehaviour
{
    protected MapGenerator MapGenerator;
    protected List<LogicMap> TilesSearch;

    private void Start()
    {
        MapGenerator = GameObject.Find("Map Generator").GetComponent<MapGenerator>();
    }


    // Métodos
    public abstract void Search(LogicMap start, LogicMap objective, LogicMap deceptiveObjective = null);
    

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

    public LogicMap GetTileByPosition(Vector3Int vector)
    {
        MapGenerator = GameObject.Find("Map Generator").GetComponent<MapGenerator>();
        return MapGenerator.GetTile(vector);
    }
}
