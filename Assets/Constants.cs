using System.Collections;
using UnityEngine;

public static class Constants
{
    public static int NUMBER_OF_AGENTS = 5;
    public static int POINTS_TEAM_1 = 0;
    public static int POINTS_TEAM_2 = 0;
    public static int MAX_DISTANCE = 1414;
    public static int MAP_OFFSET = 10; // Valor que divide a estala do mapa  

    
    public static string MAP_FILE = @"\Heightmap_0_0.png";
    public static string MAP_SATELLITE_FILE = @"\Satelite_0_0.png";
    public static string MAP_HEIGHTMAP_DIRECTORY = @"C:\Users\crisl\OneDrive\Documentos\GitHub\Hunt the flag Game\Assets\Resources\Mapas\MapasCortados\Heightmap";
    public static string MAP_SATELLITE_DIRECTORY = @"C:\Users\crisl\OneDrive\Documentos\GitHub\Hunt the flag Game\Assets\Resources\Mapas\MapasCortados\Satelite";

    public static int[] LIMITS_X_AGENT_TEAM_1 = { -1340, 1340 };
    public static int[] LIMITS_X_AGENT_TEAM_2 = { -1340, 1340 };
    public static int[] LIMITS_Y_AGENT_TEAM_1 = { -1340, -270 };
    public static int[] LIMITS_Y_AGENT_TEAM_2 = { 270, 1340 };
    public static int CLICK_POSITION_OFFSET = -134;


    public static float LIMIAR_HIGHLY_DECEPTIVE = 0.75f;
    public static float LIMIAR_PARTIALLY_DECEPTIVE = 0.3f;
    public static float LIMIAR_LITTLE_DECEPTIVE = 0.001f;

    public static char SPLITTER = ';';
    public static string DATA_BASE = "CaseDataBase.txt";

    public static string PLAYER_CONTROLLER_1 = "PlayerController";
    public static string PLAYER_CONTROLLER_2 = "Player2Controller";
    public static string PATHFINDER = "Pathfinder";
    public static string TAG_TEAM_1 = "Team1";
    public static string TAG_TEAM_2 = "Team2";
    public static string TAG_FLAG = "Flag";
    public static string MAP_GENERATOR = "Map Generator";
    public static string SIMULATION_CONTROLLER = "SimulationController";


    public static string MATERIAL_AGENT_TEAM_1 = "Agents/UnitType/Friend/Materials/infantaria";
    public static string MATERIAL_AGENT_TEAM_2 = "Agents/UnitType/Hostile/Materials/infantaria";
    public static int AGENT_SCALE = 25;
    public static int AGENT_POSITION_Z = -10;

    public static float HEURISTIC_MULTIPLIER = 10;

    //CENAS
    public static string MAIN_SCENE = "MainSystem";

    public static float MAX_WALKABLE = 0.8f;
    public static float MIN_WALKABLE = 0.1f;

    public static float CAMERA_LIMIT_PAN = 1360f;

    public static bool Walkable(float value)
    {
        return (value > MIN_WALKABLE && value <= MAX_WALKABLE);
    }
}
