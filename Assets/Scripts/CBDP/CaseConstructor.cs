using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum Distance
{
    VC = 1, C = 2, A = 3, F = 4, VF = 5
}

public enum Direction
{
    F = 10, RF = 20, LF = 30, R = 40, L = 50, B = 60, RB = 70, LB = 80
}

public enum Strategy
{
    DEFENSIVE, OFENSIVE
}

public enum TypeCase
{
    DECEPTIVE, NORMAL
}

public enum Sector
{
    DEFENSIVE, NEUTRAL, OFENSIVE
}

public partial class CaseConstructor : MonoBehaviour
{
    public string dataBaseName = "CaseDataBase.txt";
    public char splitter = ';';
    private PlayerController pcTeam1;
    private PlayerController pcTeam2;
    private int idOfLast = 0;
    Case currentCase;
    public int maxDistance = 1414;
    public DateTime initTime;

    public GameObject canvasType;
    public GameObject canvasStrategy;
    public GameObject canvasResult;

    // Start is called before the first frame update
    void Awake()
    {

        InitPlayers("PlayerController", "Player2Controller");
        StartFile();
        Invoke(nameof(ConstructInitCase), 0.5f);
    }

    public Case GetCase()
    {
        return currentCase;
    }

    public void StartFile()
    {
        //criar cabeçalhos 
        string str = "";

        //id
        str += "Id" + splitter;

        //seed
        str += "Seed" + splitter;

        //matriz de distância/direção dos agentes em relação aos agentes
        str += "MaQd" + splitter;

        //matriz de distância/direção dos agentes em relação aos objetivos
        str += "MaO" + splitter;

        //matriz de distância/direção dos agentes em relação as bases
        //str += "MaBase" + splitter;

        //matriz de distância/direção dos agentes em relação aos agentes
        str += "MaQd_int" + splitter;

        //matriz de distância/direção dos agentes em relação aos objetivos
        str += "MaO_int" + splitter;

        //matriz de distância/direção dos agentes em relação as bases
        //str += "MaBase_int" + splitter;

        //vetor de setor dos agentes
        str += "VaSec" + splitter;

        //deceptive
        str += "CaseType" + splitter;

        //tipo do plano
        str += "Strategy" + splitter;

        //resultado do plano
        str += "Result" + splitter;

        //planning file
        str += "Planning";

        //salvando 
        Save(str);
    }

    public int GetId()
    {
        //string lastLine = File.ReadLines(dataBaseName).Last();
        //string[] aData = lastLine.Split(splitter);

/*
        if (idOfLast == -1)
        {
            // primeiro caso da execução, tem q pegar o ultimo
            idOfLast = 0;
            //int.TryParse(aData[0], out idOfLast);
        }*/

        return idOfLast++;
    }

    public void ConstructInitCase()
    {
        currentCase = new Case();

        // id do caso
        currentCase.id = GetId();

        // Seed do mapa
        currentCase.seedMap = GameObject.Find("Client").GetComponent<Client>().seed;

        //matriz de distancia/direção entre os agentes e agentes (string e int)
        List<AgentController> agents_list = new List<AgentController>();
        agents_list.AddRange(pcTeam1.Agents);
        agents_list.AddRange(pcTeam2.Agents);

        currentCase.matrix_agents = new string[agents_list.Count, agents_list.Count];
        currentCase.int_matrix_agents = new int[agents_list.Count, agents_list.Count];
        for (int i = 0; i < agents_list.Count; i++)
        {
            for (int j = 0; j < agents_list.Count; j++)
            {
                if (i <= j)
                {
                    //vazio a parte superior da matrix 
                    currentCase.matrix_agents[i, j] = "";
                    currentCase.int_matrix_agents[i, j] = 0;
                    continue;
                }

                float[] sensorDirection = Sensor.CheckDirection(agents_list[i].transform, agents_list[j].transform);

                Distance distance = CalculeDistance(agents_list[i].transform, agents_list[j].transform);
                Direction direction = CalculeDirection(sensorDirection[0], sensorDirection[1]);

                currentCase.matrix_agents[i, j] = distance.ToString() + '-' + direction.ToString();
                currentCase.int_matrix_agents[i, j] = (int)distance + (int)direction;
            }
        }

        //matriz de distancia/direção entre os agentes e objetivos (string e int)
        GameObject[] flags = GameObject.FindGameObjectsWithTag("Flag");

        currentCase.matrix_objetives = new string[agents_list.Count, flags.Length];
        currentCase.int_matrix_objetives = new int[agents_list.Count, flags.Length];

        for (int i = 0; i < agents_list.Count; i++)
        {
            for (int j = 0; j < flags.Length; j++)
            {
                float[] sensorDirection = Sensor.CheckDirection(agents_list[i].transform, flags[j].transform);

                Distance distance = CalculeDistance(agents_list[i].transform, flags[j].transform);
                Direction direction = CalculeDirection(sensorDirection[0], sensorDirection[1]);

                currentCase.matrix_objetives[i, j] = distance.ToString() + '-' + direction.ToString();
                currentCase.int_matrix_objetives[i, j] = (int)distance + (int)direction;
            }
        }

        //matriz de distancia/direção entre os agentes e bases (string e int)
        /*GameObject base1 = GameObject.Find("BaseTeam1");
        GameObject base2 = GameObject.Find("BaseTeam2");

        currentCase.matrix_base = new string[agents_list.Count, 2];
        currentCase.int_matrix_base = new int[agents_list.Count, 2];

        for (int i = 0; i < agents_list.Count; i++)
        {

            float[] sensorDirection = Sensor.CheckDirection(agents_list[i].transform, base1.transform);

            Distance distance = CalculeDistance(agents_list[i].transform, base1.transform);
            Direction direction = CalculeDirection(sensorDirection[0], sensorDirection[1]);

            currentCase.matrix_objetives[i, 0] = distance.ToString() + '-' + direction.ToString();
            currentCase.int_matrix_objetives[i, 0] = (int)distance + (int)direction;


            sensorDirection = Sensor.CheckDirection(agents_list[i].transform, base2.transform);

            distance = CalculeDistance(agents_list[i].transform, base2.transform);
            direction = CalculeDirection(sensorDirection[0], sensorDirection[1]);

            currentCase.matrix_objetives[i, 0] = distance.ToString() + '-' + direction.ToString();
            currentCase.int_matrix_objetives[i, 0] = (int)distance + (int)direction;
        }*/

        //vetor setores
        currentCase.vector_sector = new Sector[agents_list.Count];

        for (int i = 0; i < agents_list.Count; i++)
        {
            currentCase.vector_sector[i] = CalculeSector(agents_list[i]);
        }

        //init plan
        currentCase.plan = new Plan
        {
            caseid = currentCase.id,
            actions = new Queue<Plan.Action>()
        };

        //iniciar o contador de tempo (pegar o time)
        this.initTime = DateTime.Now;

        //temporario
        currentCase.solutionType = TypeCase.NORMAL;
        currentCase.plan.solutionType = TypeCase.NORMAL;
    }

    public void ConstructEndCase()
    {
        var matchObjects = FindObjectsOfType<MatchBehaviour>();

        foreach (var matchObject in matchObjects)
            Destroy(matchObject.gameObject);

        pcTeam1.enabled = false;
        pcTeam2.enabled = false;

        //mostrar os paineis e esperar resposta
        canvasType.SetActive(true);

    }

    public void InitPlayers(string a, string b)
    {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/"+a);
        GameObject go = Instantiate(prefab);

        pcTeam1 = go.GetComponent<PlayerController>();
        pcTeam1.name = a;

        prefab = Resources.Load<GameObject>("Prefabs/"+b);
        go = Instantiate(prefab);

        pcTeam2 = go.GetComponent<PlayerController>();
        pcTeam2.name = b;

        //client playes
        Client client = GameObject.Find("Client").GetComponent<Client>();
        client.startPlayerControllers(a, b);
    }

    public void SetSolutionTypeInCase(string type)
    {
        if (type.Equals(TypeCase.DECEPTIVE.ToString()))
        {
            currentCase.solutionType = TypeCase.DECEPTIVE;
            currentCase.plan.solutionType = TypeCase.DECEPTIVE;
        }    
        else
        {
            currentCase.solutionType = TypeCase.NORMAL;
            currentCase.plan.solutionType = TypeCase.NORMAL;
        }
        canvasType.SetActive(false);
        canvasStrategy.SetActive(true);
    }

    public void SetStrategyTypeInCase(string strategy)
    {
        if (strategy.Equals(Strategy.OFENSIVE.ToString()))
            currentCase.strategy = Strategy.OFENSIVE;
        else
            currentCase.strategy = Strategy.DEFENSIVE;

        canvasStrategy.SetActive(false);
        canvasResult.SetActive(true);
    }

    public void SetResultInCase(bool result)
    {
        currentCase.result = result;

        canvasResult.SetActive(false);
        //save 
        SaveCase(currentCase);

        pcTeam1.enabled = true;
        pcTeam2.enabled = true;
        //reiniciar players
        Debug.Log("start agents team 1");
        pcTeam1.StartAgents();
        Debug.Log("start agents team 2");
        pcTeam2.StartAgents();

        //inicia um novo caso
        Invoke(nameof(ConstructInitCase), 0.5f);
    }

    public Sector CalculeSector(AgentController a)
    {
        if (a.transform.position.y < -100)
        {
            if (a.CompareTag("Team1"))
                return Sector.DEFENSIVE;
            else
                return Sector.OFENSIVE;
        }

        if (a.transform.position.y > 100)
        {
            if (a.CompareTag("Team1"))
                return Sector.OFENSIVE;
            else
                return Sector.DEFENSIVE;
        }

        return Sector.NEUTRAL;
    }

    public Direction CalculeDirection(float x, float y)
    {
        if (x > -0.5 && x < 0.5)
        {
            if (y == 1)
            {
                return Direction.F;
            } 
            else
            {
                return Direction.B;
            }
        }

        if (y > -0.5 && y < 0.5)
        {
            if (x == 1)
            {
                return Direction.R;
            }
            else
            {
                return Direction.L;
            }
        }

        if (x >= 0.5)
        {
            if (y >= 0.5)
            {
                return Direction.RF;
            } 
            else
            {
                return Direction.RB;
            }
        } 

        else
        {
            if (y >= 0.5)
            {
                return Direction.LF;
            }
            else
            {
                return Direction.LB;
            }
        }
    }

    public Distance CalculeDistance(Transform a, Transform b)
    {
        float distance = Vector3.Distance(a.position, b.position);

        //1414 é a maior distancia
        if (distance >= maxDistance / 2)
        {
            return Distance.VF;
        }

        if (distance >= maxDistance / 4)
        {
            return Distance.F;
        }

        if (distance >= maxDistance / 8)
        {
            return Distance.A;
        }

        if (distance >= maxDistance / 16)
        {
            return Distance.C;
        }

        return Distance.VC;
    }

    public Distance CalculeDistance(Vector3 a, Vector3 b)
    {
        float distance = Vector3.Distance(a, b);

        //1414 é a maior distancia
        if (distance >= maxDistance / 2)
        {
            return Distance.VF;
        }

        if (distance >= maxDistance / 4)
        {
            return Distance.F;
        }

        if (distance >= maxDistance / 8)
        {
            return Distance.A;
        }

        if (distance >= maxDistance / 16)
        {
            return Distance.C;
        }

        return Distance.VC;
    }

    public void PlanAddAction(Plan.Action action)
    {
        currentCase.plan.actions.Enqueue(action);
    }

    public void PlanAddAction(string str_action)
    {

        string[] str = str_action.Split(',');

        Plan.Action action = new Plan.Action();

        action.action = str[0];
        action.agent = str[1];
        action.objetive = str[2];
        
        if (str[3].Equals(TypeCase.DECEPTIVE))
            action.actionDefinition = TypeCase.DECEPTIVE;
        else
            action.actionDefinition = TypeCase.NORMAL;

        action.distance_direction = str[4];
        action.time = float.Parse(str[5]);

        currentCase.plan.actions.Enqueue(action);
    }

    private void SaveCase(Case c)
    {
        Debug.Log("salvando o caso");
        string str = c.ToString();

        Save(str);
        Debug.Log("Caso salvo!");
    }

    private void Save(string str)
    {
        //Open File
        TextWriter tw = new StreamWriter(dataBaseName, true);

        //Write to file
        tw.WriteLine(str);

        //Close File
        tw.Flush();
        tw.Close();
    }
}
