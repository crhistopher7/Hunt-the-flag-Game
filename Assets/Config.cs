using System.Collections;
using UnityEngine;

public static class Config
{
    public static int NUMBER_OF_AGENTS = 5;
    public static int POINTS_TEAM_1 = 0;
    public static int POINTS_TEAM_2 = 0;
    public static int MAX_DISTANCE = 1414;
    public static int MAP_OFFSET = 10; // Valor que divide a estala do mapa

    public static int[] LIMITS_X_AGENT_TEAM_1 = {0, 0};
    public static int[] LIMITS_X_AGENT_TEAM_2 = {0, 0};
    public static int[] LIMITS_Y_AGENT_TEAM_1 = {0, 0};
    public static int[] LIMITS_Y_AGENT_TEAM_2 = {0, 0};



    public static char SPLITTER = ';';
    public static string DATA_BASE = "CaseDataBase.txt";

    public static string PLAYER_CONTROLLER_1 = "PlayerController";
    public static string PLAYER_CONTROLLER_2 = "Player2Controller";
    public static string TAG_TEAM_1 = "Team1";
    public static string TAG_TEAM_2 = "Team2";
    public static string TAG_FLAG = "Flag";

}
