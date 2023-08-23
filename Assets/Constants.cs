using System.Collections;
using UnityEngine;

public static class Constants
{
    public static int NUMBER_OF_AGENTS = 5;
    public static int POINTS_TEAM_1 = 0;
    public static int POINTS_TEAM_2 = 0;
    public static int MAX_DISTANCE = 3072;
    public static int MAP_OFFSET = 10; // Valor que divide a estala do mapa  

    public static string MAP_HEIGHTMAP_FILE = @"C:\Users\crisl\OneDrive\Documentos\GitHub\Hunt the flag Game\Assets\Resources\Mapas\pngs\recorte300x300A.png";
    //public static string MAP_HEIGHTMAP_FILE = @"C:\Users\crisl\OneDrive\Documentos\GitHub\Hunt the flag Game\Assets\Resources\Mapas\MapasCortados\Heightmap\Heightmap_0_0.png";
    public static string MAP_SATELLITE_FILE = @"C:\Users\crisl\OneDrive\Documentos\GitHub\Hunt the flag Game\Assets\Resources\Mapas\pngs\recorte300x300A.png";
    //public static string MAP_SATELLITE_FILE = @"C:\Users\crisl\OneDrive\Documentos\GitHub\Hunt the flag Game\Assets\Resources\Mapas\MapasCortados\Satelite\Satelite_0_0.png";

    public static int[] REAL_GOAL_POSITION = { 889, 1111 };
    public static int[] DECEPTIVE_GOAL_POSITION = { -1002, 1032 };

    public static int[] LIMITS_X_AGENT_TEAM_1 =  { -1536, 1536 }; // { -1340, -670 };
    public static int[] LIMITS_X_AGENT_TEAM_2 = { -1536, 1536 }; // { -1340, 1340 };
    public static int[] LIMITS_Y_AGENT_TEAM_1 = { -1536, -384 }; //{ -1340, -270 };
    public static int[] LIMITS_Y_AGENT_TEAM_2 = { -384, 1536 }; // { 270, 1340 };
    public static int[] SECTOR_LIMITS = { -1382, 1382 }; // 45% do mapa def or ofens. 55% do centro é neutro
    public static int CLICK_POSITION_OFFSET = -154;
    public static int[] IMAGE_SIZE = { 100, 100 };

    public static bool USE_P4_CODE = true;

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
    public static string TAG_BUTTON_CASE = "ButtonCase";
    public static string TAG_LINE_OF_PATH = "LineOfPath";
    public static string MAP_GENERATOR = "Map Generator";
    public static string SIMULATION_CONTROLLER = "SimulationController";

    public static string MATERIAL_AGENT_TEAM_1 = "Agents/UnitType/Friend/Materials/infantaria";
    public static string MATERIAL_AGENT_TEAM_2 = "Agents/UnitType/Hostile/Materials/infantaria";
    public static int AGENT_SCALE = 60;
    public static int AGENT_POSITION_Z = -10;

    public static float HEURISTIC_MULTIPLIER = 1;
    public static string REAL_GOAL = "Real_Team2";

    //CENAS
    public static string MAIN_SCENE = "MainSystem";

    public static float MAX_WALKABLE = 2f;
    public static float MIN_WALKABLE = 0f;

    public static float CAMERA_LIMIT_PAN = 1538f;

    public static string CAMERA_SCREEN_SHOT = "CameraScreenShot";

    public static bool Walkable(float value)
    {
        return (value > MIN_WALKABLE && value <= MAX_WALKABLE);
    }
}
