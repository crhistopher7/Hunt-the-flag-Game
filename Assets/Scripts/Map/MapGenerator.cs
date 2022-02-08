using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public struct TerrainType
{
    public string name;
    public float heigth;
    public Color color;
}


public class MatchBehaviour : MonoBehaviour
{

}

public class MapGenerator : MonoBehaviour
{
    public static MapGenerator Instance;
    public int mapWidth;
    public int mapHeigth;
    public float noiseScale;

    public enum DrawMode { NoiseMap, ColourMap};
    public DrawMode drawMode;

    //public int seed;
    public Vector2 offset;

    public int octaves;
    [Range(0,1)]
    public float persistance;
    public float lacunarity;

    public bool autoUpdate;

    public TerrainType[] regions;
    Color[] colourMap;
    MapDisplay display;

    public static Vector3Int[] Directions = new Vector3Int[4]
    {
        Vector3Int.up,
        Vector3Int.right,
        Vector3Int.down,
        Vector3Int.left
    };

    Dictionary<Vector3Int, LogicMap> NavMap;
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        Instance = this;
    }

    public void GenerateMap(int seed)
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeigth, seed, noiseScale, octaves, persistance, lacunarity, offset);
        NavMap = new Dictionary<Vector3Int, LogicMap>();
        display = FindObjectOfType<MapDisplay>();
        colourMap = new Color[mapHeigth * mapWidth];

        for (int y = 0; y < mapHeigth; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float currentHeigth = noiseMap[x, y];

                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentHeigth <= regions[i].heigth)
                    {
                        colourMap[y * mapWidth + x] = regions[i].color;
                        int moveCost = int.MaxValue;

                        //pode andar em tudo que não for agua
                        if (currentHeigth > 0.34f && currentHeigth <= 0.74f)
                        {
                            moveCost = 1;
                        }
                        
                        LogicMap logicMap = new LogicMap
                        {
                            Position = new Vector3Int(x, y, 0),             
                            ClickPosition = new Vector3Int(-49+x, -49+y, 0),
                            Walkable = (currentHeigth > 0.34f && currentHeigth <= 0.74f),
                            MoveCost = moveCost,
                            ColorMapIndex = y * mapWidth + x
                        };
                        //Debug.Log(logicMap.ClickPosition);
                        NavMap.Add(logicMap.ClickPosition, logicMap);

                        break;
                    }
                }
            }
        }
         
        

        if (drawMode == DrawMode.NoiseMap)
        {
            display.DrawnTexture(TextureGenerator.TextureFromHeigthMap(noiseMap));
        } else if (drawMode == DrawMode.ColourMap)
        {
            display.DrawnTexture(TextureGenerator.TextureFromColourMap(colourMap, mapWidth, mapHeigth));
        }
    }

    public void UpdateTexture()
    {
        display.DrawnTexture(TextureGenerator.TextureFromColourMap(colourMap, mapWidth, mapHeigth));
    }

    public void PaintTile(LogicMap tile, Color color)
    {
        colourMap[tile.ColorMapIndex] = color;
    }

    public LogicMap GetTile(Vector3Int position)
    {
        if (NavMap.TryGetValue(position, out LogicMap tile))
            return tile;
        return null;
    }

    

    public void ClearSearch()
    {
        foreach (LogicMap t in NavMap.Values)
        {
            t.CostFromOrigin = int.MaxValue;
            t.CostToObjective = int.MaxValue;
            t.Score = int.MaxValue;
            t.Previous = null;
        }
    }

    private void OnValidate()
    {
        if (mapWidth < 1)
        {
            mapWidth = 1;
        }
        if (mapHeigth < 1)
        {
            mapHeigth = 1;
        }
        if (lacunarity < 1)
        {
            lacunarity = 1;
        }
        if (octaves < 0)
        {
            octaves = 0;
        }
    }
}

