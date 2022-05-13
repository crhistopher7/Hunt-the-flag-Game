using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class CBDP : MonoBehaviour
{
    private int idOfLast = 0;
    CaseCBDP currentCase;
    public DateTime initTime;


    // Start is called before the first frame update
    void Awake()
    {
        StartFile();
        Invoke(nameof(ConstructInitCase), 0.5f);
    }

    public CaseCBDP GetCase()
    {
        return currentCase;
    }

    public void StartFile()
    {
        //criar cabe�alhos 
        string str = "";

        //id
        str += "caseID" + Config.SPLITTER;

        //seed
        str += "seed" + Config.SPLITTER;

        //matriz de dist�ncia/dire��o dos agentes em rela��o aos agentes
        str += "agentsRelationships" + Config.SPLITTER;

        //matriz de dist�ncia/dire��o dos agentes em rela��o aos objetivos
        str += "agentsGoalsRelationships" + Config.SPLITTER;

        //matriz de dist�ncia/dire��o dos agentes em rela��o as bases
        //str += "agentsBasesRelationships" + Config.SPLITTER;

        //matriz de dist�ncia/dire��o dos agentes em rela��o aos agentes
        str += "agentsRelationships_int" + Config.SPLITTER;

        //matriz de distância/dire��o dos agentes em rela��o aos objetivos
        str += "agentsGoalsRelationships_int" + Config.SPLITTER;

        //matriz de dist�ncia/dire��o dos agentes em rela��o as bases
        //str += "agentsBasesRelationships_int" + Config.SPLITTER;

        //vetor de setor dos agentes
        str += "agentsBattleFieldLocalization" + Config.SPLITTER;

        //deceptive
        str += "deceptiveLevel" + Config.SPLITTER;

        //tipo do plano
        str += "strategy" + Config.SPLITTER;

        //description
        str += "description" + Config.SPLITTER;

        //resultado do plano
        str += "result" + Config.SPLITTER;

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
            // primeiro caso da execu��o, tem q pegar o ultimo
            idOfLast = 0;
            //int.TryParse(aData[0], out idOfLast);
        }*/

        return idOfLast++;
    }

    public void ConstructInitCase()
    {
        currentCase = new CaseCBDP();

        // id do caso
        currentCase.caseId = GetId();

        // Seed do mapa
        currentCase.seedMap = GameObject.Find("Client").GetComponent<Client>().seed;

        //matriz de distancia/dire��o entre os agentes e agentes (string e int)
        List<AgentController> agents_list = new List<AgentController>();
        //agents_list.AddRange(OrderAgentList(pcTeam1.Agents));
        //agents_list.AddRange(OrderAgentList(pcTeam2.Agents));

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

                Distance distance = CBDPUtils.CalculeDistance(agents_list[i].transform.position, agents_list[j].transform.position);
                Direction direction = CBDPUtils.CalculeDirection(sensorDirection[0], sensorDirection[1]);

                currentCase.matrix_agents[i, j] = distance.ToString() + '-' + direction.ToString();
                currentCase.int_matrix_agents[i, j] = (int)distance + (int)direction;
            }
        }

        //matriz de distancia/dire��o entre os agentes e objetivos (string e int)
        GameObject[] flags = GameObject.FindGameObjectsWithTag("Flag");

        currentCase.matrix_objetives = new string[agents_list.Count, flags.Length];
        currentCase.int_matrix_objetives = new int[agents_list.Count, flags.Length];

        for (int i = 0; i < agents_list.Count; i++)
        {
            for (int j = 0; j < flags.Length; j++)
            {
                float[] sensorDirection = Sensor.CheckDirection(agents_list[i].transform, flags[j].transform);

                Distance distance = CBDPUtils.CalculeDistance(agents_list[i].transform.position, flags[j].transform.position);
                Direction direction = CBDPUtils.CalculeDirection(sensorDirection[0], sensorDirection[1]);

                currentCase.matrix_objetives[i, j] = distance.ToString() + '-' + direction.ToString();
                currentCase.int_matrix_objetives[i, j] = (int)distance + (int)direction;
            }
        }

        //matriz de distancia/dire��o entre os agentes e bases (string e int)
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
            caseid = currentCase.caseId,
            actions = new Queue<Action>()
        };

        //iniciar o contador de tempo (pegar o time)
        this.initTime = DateTime.Now;

        //temporario
        currentCase.solutionType = DeceptiveLevel.NOT_DECEPTIVE;
        currentCase.plan.solutionType = DeceptiveLevel.NOT_DECEPTIVE;
    }

    private List<AgentController> OrderAgentList(List<AgentController> agents)
    {
        return agents.OrderBy(p => p.transform.position.x).ThenBy(p => p.transform.position.y).ToList();
    }

    public void ConstructEndCase()
    {
        var matchObjects = FindObjectsOfType<MatchBehaviour>();

        foreach (var matchObject in matchObjects)
            Destroy(matchObject.gameObject);

        //pcTeam1.enabled = false;
        //pcTeam2.enabled = false;

        //mostrar os paineis e esperar resposta
        canvasType.SetActive(true);

    }

    public void SetSolutionTypeInCase(string type)
    {
        if (type.Equals("DECEPTIVE"))
        {
            //currentCase.solutionType = DeceptiveLevel.DECEPTIVE;
            //currentCase.plan.solutionType = DeceptiveLevel.DECEPTIVE;
        }    
        else
        {
            currentCase.solutionType = DeceptiveLevel.NOT_DECEPTIVE;
            currentCase.plan.solutionType = DeceptiveLevel.NOT_DECEPTIVE;
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
        canvasDescription.SetActive(true);
    }

    public void SetDescriptionInCase()
    {
        currentCase.description = inputDescription.text;

        canvasDescription.SetActive(false);
        canvasResult.SetActive(true);
    }

    public void SetResultInCase(bool result)
    {
        currentCase.result = result;
        canvasResult.SetActive(false);
        
        //save 
        SaveCase(currentCase);

        //RestartGame
        //RestartGame();
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

    

    public void PlanAddAction(Action action)
    {
        currentCase.plan.actions.Enqueue(action);
    }

    public void PlanAddAction(string str_action)
    {

        string[] str = str_action.Split(',');

        Action action = new Action();

        action.action = str[0];
        action.agent = str[1];
        action.objetive = str[2];

        Enum.TryParse(str[3], out action.actionDefinition);
        Enum.TryParse(str[4], out action.pathType);

        action.distance_direction = str[5];
        action.distance_directionDeceptive = str[6];
        action.time = int.Parse(str[7]);

        currentCase.plan.actions.Enqueue(action);
    }

    private void SaveCase(CaseCBDP c)
    {
        Debug.Log("salvando o caso");
        string str = c.ToString();

        Save(str);
        Debug.Log("Caso salvo!");
    }

    private void Save(string str)
    {
        //Open File
        TextWriter tw = new StreamWriter(Config.DATA_BASE, true);

        //Write to file
        tw.WriteLine(str);

        //Close File
        tw.Flush();
        tw.Close();
    }
}
