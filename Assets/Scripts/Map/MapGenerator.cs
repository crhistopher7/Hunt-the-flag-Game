using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

[System.Serializable]
public struct TerrainType
{
    public string name;
    public float heigth;
    public Color color;
}

public class MapGenerator : MonoBehaviour
{
    public static MapGenerator Instance;
    private int mapWidth = 100;
    private int mapHeigth = 100;
    private float noiseScale = 23;

    public enum DrawMode { NoiseMap, ColourMap};
    private DrawMode drawMode;

    //public int seed;
    private Vector2 offset = new Vector2(0, 0);

    private int octaves = 20;
    [Range(0,1)]
    private float persistance = 0.5f;
    private float lacunarity = 5;

    private bool autoUpdate = false;

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

    public void GenerateRealMap(string path, string heightmap, string realmap)
    {
        byte[] fileData;

        if (!File.Exists(path + heightmap) || !File.Exists(path + realmap))
        {
            Debug.Log(path + heightmap);
            Debug.Log(path + realmap);
            Debug.LogError("Não encontrou as imagens do mapa");
            return;
        }
            

        fileData = File.ReadAllBytes(path + heightmap);
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(fileData);


        float[,] mapBytes = new float[tex.width, tex.height];

        NavMap = new Dictionary<Vector3Int, LogicMap>();
        display = FindObjectOfType<MapDisplay>();
        Debug.Log(tex.height);
        Debug.Log(tex.width);

        for (int y = 0; y < tex.height; y++) 
        { 
            for (int x = 0; x < tex.width; x++)
            {
                Color pixel = tex.GetPixel(x, y);
                float currentHeigth = pixel.grayscale;
                int moveCost = int.MaxValue;

                Debug.Log(currentHeigth);
                if (Config.Walkable(currentHeigth))
                    moveCost = 1;

                LogicMap logicMap = new LogicMap
                {
                    Position = new Vector3Int(x, y, 0),
                    ClickPosition = new Vector3Int(Config.CLICK_POSITION_OFFSET + x, Config.CLICK_POSITION_OFFSET + y, 0),
                    Walkable = Config.Walkable(currentHeigth),
                    MoveCost = moveCost,
                    ColorMapIndex = y * tex.width + x
                };
                //Debug.Log(logicMap.ClickPosition);
                NavMap.Add(logicMap.ClickPosition, logicMap);
            }
        }

        fileData = File.ReadAllBytes(path + realmap);
        Texture2D real_tex = new Texture2D(2, 2);
        real_tex.LoadImage(fileData);

        display.DrawnTexture(real_tex, tex.width, tex.height);
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
                        if (Config.Walkable(currentHeigth))
                        {
                            moveCost = 1;
                        }
                        
                        LogicMap logicMap = new LogicMap
                        {
                            Position = new Vector3Int(x, y, 0),             
                            ClickPosition = new Vector3Int(Config.CLICK_POSITION_OFFSET + x, Config.CLICK_POSITION_OFFSET + y, 0),
                            Walkable = Config.Walkable(currentHeigth),
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
            display.DrawnTexture(TextureGenerator.TextureFromHeigthMap(noiseMap), mapWidth, mapHeigth);
        } else if (drawMode == DrawMode.ColourMap)
        {
            display.DrawnTexture(TextureGenerator.TextureFromColourMap(colourMap, mapWidth, mapHeigth), mapWidth, mapHeigth);
        }
    }

    public void UpdateTexture()
    {
        display.DrawnTexture(TextureGenerator.TextureFromColourMap(colourMap, mapWidth, mapHeigth), mapWidth, mapHeigth);
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

