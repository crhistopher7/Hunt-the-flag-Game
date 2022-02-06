using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

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

public enum Type
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

    // Start is called before the first frame update
    void Awake()
    {
        InitPlayers();
        StartFile();
        ConstructInitCase();
    }

    private void Update()
    {
        if (Input.GetKeyDown("d"))
        {
            Debug.Log("D pressionado");
            currentCase.solutionType = Type.DECEPTIVE;
            currentCase.plan.solutionType = Type.DECEPTIVE;
        }
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
        str += "MaBase" + splitter;

        //matriz de distância/direção dos agentes em relação aos agentes
        str += "MaQd_int" + splitter;

        //matriz de distância/direção dos agentes em relação aos objetivos
        str += "MaO_int" + splitter;

        //matriz de distância/direção dos agentes em relação as bases
        str += "MaBase_int" + splitter;

        //vetor de setor dos agentes
        str += "VaSec" + splitter;

        //deceptive
        str += "CaseType" + splitter;

        //tipo do plano
        str += "Strategy" + splitter;

        //planning file
        str += "Planning" + splitter;

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
        currentCase.solutionType = Type.NORMAL;
        currentCase.plan.solutionType = Type.NORMAL;
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

    public void InitPlayers()
    {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/PlayerController");
        GameObject go = Instantiate(prefab);

        pcTeam1 = go.GetComponent<PlayerController>();
        pcTeam1.name = "PlayerController";

        prefab = Resources.Load<GameObject>("Prefabs/Player2Controller");
        go = Instantiate(prefab);

        pcTeam2 = go.GetComponent<PlayerController>();
        pcTeam2.name = "Player2Controller";

        //client playes
        Client client = GameObject.Find("Client").GetComponent<Client>();
        client.startPlayerControllers();
    }

    public void SetSolutionTypeInCase(string type)
    {
        if (type.Equals(Type.DECEPTIVE.ToString()))
        {
            currentCase.solutionType = Type.DECEPTIVE;
            currentCase.plan.solutionType = Type.DECEPTIVE;
        }    
        else
        {
            currentCase.solutionType = Type.NORMAL;
            currentCase.plan.solutionType = Type.NORMAL;
        }
        canvasType.SetActive(false);
        canvasStrategy.SetActive(true);
    }

    public void SetStrategyTypeInCase(string strategy)
    {
        Debug.Log("entrou");
        if (strategy.Equals(Strategy.OFENSIVE.ToString()))
            currentCase.strategy = Strategy.OFENSIVE;
        else
            currentCase.strategy = Strategy.DEFENSIVE;

        canvasStrategy.SetActive(false);
        Debug.Log("setou falso");
        //result
        SetResultInCase(true);

        //save 
        SaveCase(currentCase);
        Debug.Log("salvou");

        pcTeam1.enabled = false;
        pcTeam2.enabled = false;
        //reiniciar players
        Debug.Log("start agents team 1");
        pcTeam1.StartAgents();
        Debug.Log("start agents team 2");
        pcTeam2.StartAgents();

        //inicia um novo caso
        ConstructInitCase();
    }

    public void SetResultInCase(bool result)
    {
        currentCase.result = result;
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
        
        if (str[3].Equals(Type.DECEPTIVE))
            action.actionDefinition = Type.DECEPTIVE;
        else
            action.actionDefinition = Type.NORMAL;

        action.distance_direction = str[4];
        action.time = float.Parse(str[5]);

        currentCase.plan.actions.Enqueue(action);
    }

    public string ToMatrixString(string[,] matrix, string delimiter = ",")
    {
        string s = "{";

        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            s += "{";
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                s += matrix[i, j].ToString()   + (delimiter);
            }

            s += "}:";
        }

        return s += "}";
    }

    public string ToMatrixString(int[,] matrix, string delimiter = ",")
    {
        string s = "{";

        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            s += "{";
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                s += matrix[i, j].ToString() + (delimiter);
            }

            s += "}:";
        }

        return s += "}";
    }

    private void SaveCase(Case c)
    {
        Debug.Log("salvando o caso");
        string str = "";

        str += c.id.ToString() + splitter;
        str += c.seedMap.ToString() + splitter;


        str += ToMatrixString(c.matrix_agents) + splitter;     

        str += ToMatrixString(c.matrix_objetives) + splitter;

        //str += ToMatrixString(c.matrix_base) + splitter;

        str += ToMatrixString(c.int_matrix_agents) + splitter;

        str += ToMatrixString(c.int_matrix_objetives) + splitter;

        //str += ToMatrixString(c.int_matrix_base) + splitter;

        str += "{";
        for (int i = 0; i < c.vector_sector.Length; i++) 
        {
            str += c.vector_sector[i].ToString();
            if (i != c.vector_sector.Length - 1)
                str += ",";
        }
        str += "}" + splitter;

        str += c.solutionType.ToString() + splitter;
        str += c.strategy.ToString() + splitter;
        str += c.result.ToString() + splitter;
        str += c.plan.ToString();

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
