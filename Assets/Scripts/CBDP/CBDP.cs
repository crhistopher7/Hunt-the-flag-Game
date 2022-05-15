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
    private CBRAPI cbr;
    private string[] features;

    // Start is called before the first frame update
    void Start()
    {
        cbr = new CBRAPI();
        StartFile();
        ConvertCSVToCaseBase();
    }

    public CaseCBDP GetCase()
    {
        return currentCase;
    }

    private void ConvertCSVToCaseBase()
    {
        using var reader = new StreamReader(Config.DATA_BASE);
        Debug.Log("Lendo " + Config.DATA_BASE);

        if (!reader.EndOfStream)
        {
            var header = reader.ReadLine();
            features = header.Split(Config.SPLITTER);
            //Debug.Log("Cabeçalho: " + header);

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(Config.SPLITTER);

                //Debug.Log("Lendo linha: " + line);

                Case caso = CaseToCase(values);

                // Adicionando um caso na base de casos
                cbr.AddCase(caso);
            }
        }
        Debug.Log("Base de casos carregada!");
    }


    private Case CaseToCase(string[] values)
    {

        Case caso = new Case();

        //--------------- Estruturacao da descricao do problema do caso
        //Id
        caso.caseDescription.Add(new CaseFeature(0, features[0], typeof(int), values[0]));
        //Seed
        caso.caseDescription.Add(new CaseFeature(1, features[1], typeof(int), values[1]));
        //MaQd
        caso.caseDescription.Add(new CaseFeature(2, features[2], typeof(string), values[2]));
        //MaO
        caso.caseDescription.Add(new CaseFeature(3, features[3], typeof(string), values[3]));
        //MaQd_int
        caso.caseDescription.Add(new CaseFeature(4, features[4], typeof(string), values[4]));
        //MaO_int
        caso.caseDescription.Add(new CaseFeature(5, features[5], typeof(string), values[5]));
        //VaSec
        caso.caseDescription.Add(new CaseFeature(6, features[6], typeof(string), values[6]));
        //CaseType
        caso.caseDescription.Add(new CaseFeature(7, features[7], typeof(string), values[7]));
        //Strategy
        caso.caseDescription.Add(new CaseFeature(8, features[8], typeof(string), values[8]));
        //Result
        caso.caseDescription.Add(new CaseFeature(9, features[9], typeof(string), values[9]));
        //Description
        caso.caseDescription.Add(new CaseFeature(9, features[10], typeof(string), values[10]));

        //--------------- Estruturacao da descricao da solucao do caso
        //Planning
        caso.caseSolution.Add(new CaseFeature(0, features[11], typeof(string), values[11]));

        return caso;
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

    public void ConstructInitCase(List<AgentController> agentsTeam1, List<AgentController> agentsTeam2)
    {
        currentCase = new CaseCBDP();

        // id do caso
        currentCase.caseId = GetId();

        // Seed do mapa
        currentCase.seedMap = GameObject.Find("Client").GetComponent<Client>().seed;

        //matriz de distancia/dire��o entre os agentes e agentes (string e int)
        List<AgentController> agents_list = new List<AgentController>();
        agents_list.AddRange(CBDPUtils.OrderAgentList(agentsTeam1));
        agents_list.AddRange(CBDPUtils.OrderAgentList(agentsTeam2));

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
        GameObject[] flags = GameObject.FindGameObjectsWithTag(Config.TAG_FLAG);

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

    

    public void SetSolutionTypeInCase(DeceptiveLevel type)
    {
        currentCase.solutionType = type;
        currentCase.plan.solutionType = type;
    }

    public void SetStrategyTypeInCase(Strategy strategy)
    {
        currentCase.strategy = strategy;
    }

    public void SetDescriptionInCase(string text)
    {
        currentCase.description = text;
    }

    public void SetResultInCase(bool result)
    {
        currentCase.result = result;
    }

    public Sector CalculeSector(AgentController a)
    {
        if (a.transform.position.y < -100)
        {
            if (a.CompareTag(Config.TAG_TEAM_1))
                return Sector.DEFENSIVE;
            else
                return Sector.OFENSIVE;
        }

        if (a.transform.position.y > 100)
        {
            if (a.CompareTag(Config.TAG_TEAM_2))
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

    public void SaveCase(CaseCBDP c)
    {
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

    public List<string[]> SearchSimilarCases()
    {
        List<string[]> cases = new List<string[]>();
        var case_str = this.currentCase.ToString();

        Debug.Log("CASO ATUAL: " + case_str);
        Case currentCase = CaseToCase(case_str.Split(Config.SPLITTER));
        List<Result> similiarCases = GetResultsOfSimilarCases(currentCase);

        foreach (Result similarCase in similiarCases)
        {
            cases.Add(new string[] { similarCase.matchCase.caseDescription[10].value, similarCase.matchCase.caseSolution[0].value });
        }

        return cases;
    }

    private List<Result> GetResultsOfSimilarCases(Case currentCase)
    {
        // Instanciacao da estrutura do caso
        ConsultStructure consultStructure = new ConsultStructure();

        // Informando qual medida de similaridade global utilizar
        consultStructure.globalSimilarity = new EuclideanDistance(consultStructure);

        // Estruturacao de como o caso sera consultado na base de casos
        consultStructure.consultParams.Add(new ConsultParams(new List<int> { 1 }, 0.1f, new Equals()));            //Seed
        //consultStructure.consultParams.Add(new ConsultParams(new List<int> { 7 }, 1f, new Equals()));            //Tipo do caso
        //consultStructure.consultParams.Add(new ConsultParams(new List<int> { 8 }, 1f, new Equals()));            //Estratégia 
        //consultStructure.consultParams.Add(new ConsultParams(new List<int> { 9 }, 1f, new Equals()));            //Resultado
        consultStructure.consultParams.Add(new ConsultParams(new List<int> { 2 }, 1f, new MatrixSimilarity()));  //Matriz de agentes
        consultStructure.consultParams.Add(new ConsultParams(new List<int> { 3 }, 0.2f, new MatrixSimilarity()));  //Matriz de objetivos
        consultStructure.consultParams.Add(new ConsultParams(new List<int> { 6 }, 0.1f, new SectorSimilarity()));  //Vetor de setor dos agentes

        // Realizando uma consulta na base de casos (lista já ordenada por maior score)
        List<Result> results = cbr.Retrieve(currentCase, consultStructure);

        /* Entre os resltados encontrar o melhor que é (Enganoso, ofensivo e Funcionou)
        foreach (Result result in results)
        {
            //DECEPTIVE;OFENSIVE;True
            if (result.matchCase.caseDescription[7].value.Equals("DECEPTIVE") &&
                result.matchCase.caseDescription[8].value.Equals("OFENSIVE") &&
                result.matchCase.caseDescription[9].value.Equals("True"))
            {
                // Exibindo o resultado da consulta
                Debug.Log("Caso recuperado: " + result.matchCase.caseDescription[0].value + " com " + (result.matchPercentage * 100).ToString("0.00") + "% de similaridade");

                //return result.matchCase;
            }
        }*/


        // Não encontrou um que possui essas 3 dependencias 
        //Debug.Log("Caso recuperado: " + results[0].matchCase.caseDescription[0].value + " com " + (results[0].matchPercentage * 100).ToString("0.00") + "% de similaridade");

        return results;
    }
}
