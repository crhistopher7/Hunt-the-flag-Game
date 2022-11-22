using Assets.Scripts.CBDP;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;



public class CBDP : MonoBehaviour
{
    private int idOfLast = 0;
    private CaseCBDP currentCase;
    public DateTime initTime;
    private CBRAPI cbr;
    private string[] features;

    // Start is called before the first frame update
    void Start()
    {
        //iniciar o contador de tempo (pegar o time)
        this.initTime = DateTime.Now;
        cbr = new CBRAPI();
        //StartFile();
        ConvertCSVToCaseBase();
    }

    public CaseCBDP GetCase()
    {
        return currentCase;
    }

    private void ConvertCSVToCaseBase()
    {
        using var reader = new StreamReader(Constants.DATA_BASE);

        if (!reader.EndOfStream)
        {
            var header = reader.ReadLine();
            features = header.Split(Constants.SPLITTER);
            //Debug.Log("Cabeçalho: " + header);

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                //Debug.Log("Lendo linha: " + line);

                var values = line.Split(Constants.SPLITTER);
                Case c = StringCaseToCase(values);
                cbr.AddCase(c);
            }
        }
        Debug.Log("Base de casos carregada!");
    }


    private Case StringCaseToCase(string[] values)
    {

        Case caso = new Case();

        //--------------- Estruturacao da descricao do problema do caso
        //caseID
        caso.caseDescription.Add(new CaseFeature(0, features[0], typeof(int), values[0]));
        //seed
        caso.caseDescription.Add(new CaseFeature(1, features[1], typeof(int), values[1]));
        //agentsRelationships
        caso.caseDescription.Add(new CaseFeature(2, features[2], typeof(string), values[2]));
        //agentsGoalsRelationships
        caso.caseDescription.Add(new CaseFeature(3, features[3], typeof(string), values[3]));
        //agentsRelationships_distance_angle
        caso.caseDescription.Add(new CaseFeature(4, features[4], typeof(string), values[4]));
        //agentsRelationships_int
        //caso.caseDescription.Add(new CaseFeature(5, features[5], typeof(string), values[5]));
        //agentsGoalsRelationships_int
        //caso.caseDescription.Add(new CaseFeature(6, features[6], typeof(string), values[6]));
        //agentsBattleFieldLocalization
        caso.caseDescription.Add(new CaseFeature(5, features[5], typeof(string), values[5]));
        //deceptiveLevel
        caso.caseDescription.Add(new CaseFeature(6, features[6], typeof(string), values[6]));
        //strategy
        caso.caseDescription.Add(new CaseFeature(7, features[7], typeof(string), values[7]));
        //description
        caso.caseDescription.Add(new CaseFeature(8, features[8], typeof(string), values[8]));
        //result
        caso.caseDescription.Add(new CaseFeature(9, features[9], typeof(string), values[9]));

        //--------------- Estruturacao da descricao da solucao do caso
        //Planning
        caso.caseSolution.Add(new CaseFeature(0, features[10], typeof(string), values[10]));

        return caso;
    }

    /// <summary>
    /// Função que retorna o nível enganoso do caso atual a partir da quantidade de ações enganosas no plano
    /// </summary>
    /// <returns>Nível enganoso</returns>
    public DeceptiveLevel CalculateDeceptionLevel()
    {
        int countTotalActions = currentCase.plan.actions.Count;
        int countDeceptiveActions = 0;

        if (countTotalActions == 0)
            return DeceptiveLevel.NOT_DECEPTIVE;

        foreach (Action action in currentCase.plan.actions)
        {
            if (!action.actionDefinition.Equals(DeceptiveLevel.NOT_DECEPTIVE))
                countDeceptiveActions++;
        }

        if (countDeceptiveActions / countTotalActions >= Constants.LIMIAR_HIGHLY_DECEPTIVE)
            return DeceptiveLevel.HIGHLY_DECEPTIVE;
        if (countDeceptiveActions / countTotalActions >= Constants.LIMIAR_PARTIALLY_DECEPTIVE)
            return DeceptiveLevel.PARTIALLY_DECEPTIVE;
        if (countDeceptiveActions / countTotalActions >= Constants.LIMIAR_LITTLE_DECEPTIVE)
            return DeceptiveLevel.LITTLE_DECEPTIVE;

        return DeceptiveLevel.NOT_DECEPTIVE;
    }

    public void StartFile()
    {
        string str = "";
        str += "caseID" + Constants.SPLITTER;
        str += "seed" + Constants.SPLITTER;
        str += "agentsRelationships" + Constants.SPLITTER;
        str += "agentsGoalsRelationships" + Constants.SPLITTER;
        str += "agentsRelationships_distance_angle" + Constants.SPLITTER;
        //str += "agentsRelationships_int" + Constants.SPLITTER;
        //str += "agentsGoalsRelationships_int" + Constants.SPLITTER;
        str += "agentsBattleFieldLocalization" + Constants.SPLITTER;
        str += "deceptiveLevel" + Constants.SPLITTER;
        str += "strategy" + Constants.SPLITTER;
        str += "description" + Constants.SPLITTER;
        str += "result" + Constants.SPLITTER;

        str += "Planning";
        //salvando 
        Save(str);
    }

    public void ConstructInitCase(List<AgentController> agentsTeam1, List<AgentController> agentsTeam2)
    {
        currentCase = new CaseCBDP();

        // id do caso
        currentCase.caseId = idOfLast++;

        // Seed do mapa
        currentCase.seedMap = 0; //GameObject.Find("Client").GetComponent<Client>().seed;

        //matriz de distancia/dire��o entre os agentes e agentes (string e int)
        List<AgentController> agents_list = new List<AgentController>();
        agents_list.AddRange(CBDPUtils.OrderAgentList(agentsTeam1));
        agents_list.AddRange(CBDPUtils.OrderAgentList(agentsTeam2));

        currentCase.matrix_agents = new Qualitative[agents_list.Count, agents_list.Count];
        currentCase.matrix_agents_distance_angle = new Qualitative[agents_list.Count, agents_list.Count];
        //currentCase.int_matrix_agents = new int[agents_list.Count, agents_list.Count];

        for (int i = 0; i < agents_list.Count; i++)
        {
            for (int j = 0; j < agents_list.Count; j++)
            {
                if (i <= j)
                {
                    //vazio a parte superior da matrix 
                    currentCase.matrix_agents[i, j] = new Qualitative();
                    currentCase.matrix_agents_distance_angle[i, j] = new Qualitative();
                    //currentCase.int_matrix_agents[i, j] = 0;
                    continue;
                }

                float[] sensorDirection = Sensor.CheckDirection(agents_list[i].transform, agents_list[j].transform);

                Distance distance = CBDPUtils.CalculeDistance(agents_list[i].transform.position, agents_list[j].transform.position);
                Direction direction = CBDPUtils.CalculeDirection(sensorDirection[0], sensorDirection[1]);

                //float distance_float = CBDPUtils.GetDistanceByAStarPath(agents_list[i].transform.position, agents_list[j].transform.position);
                float distance_float = Vector3.Distance(agents_list[i].transform.position, agents_list[j].transform.position);
                float angle = Vector3.Angle(agents_list[i].transform.position, agents_list[j].transform.position);

                currentCase.matrix_agents[i, j] = new Qualitative(distance, direction);
                currentCase.matrix_agents_distance_angle[i, j] = new Qualitative(angle, distance_float);
                //currentCase.int_matrix_agents[i, j] = (int)distance + (int)direction;
            }
        }

        //matriz de distancia/dire��o entre os agentes e objetivos (string e int)
        GameObject[] objectives = GameObject.FindGameObjectsWithTag(Constants.TAG_FLAG);

        currentCase.matrix_objetives = new Qualitative[agents_list.Count, objectives.Length];
        //currentCase.int_matrix_objetives = new int[agents_list.Count, objectives.Length];

        for (int i = 0; i < agents_list.Count; i++)
        {
            for (int j = 0; j < objectives.Length; j++)
            {
                float[] sensorDirection = Sensor.CheckDirection(agents_list[i].transform, objectives[j].transform);

                Distance distance = CBDPUtils.CalculeDistance(agents_list[i].transform.position, objectives[j].transform.position);
                Direction direction = CBDPUtils.CalculeDirection(sensorDirection[0], sensorDirection[1]);

                currentCase.matrix_objetives[i, j] = new Qualitative(distance, direction);
                //currentCase.int_matrix_objetives[i, j] = (int)distance + (int)direction;
            }
        }

        //vetor setores
        currentCase.vector_sector = new Sector[agents_list.Count];

        for (int i = 0; i < agents_list.Count; i++)
        {
            currentCase.vector_sector[i] = CalculeSector(agents_list[i]);
        }

        currentCase.strategy = Strategy.OFENSIVE;

        //init plan
        currentCase.plan = new Plan
        {
            caseid = currentCase.caseId,
            actions = new Queue<Action>()
        };
    }

    internal void NormalizeTimeInPlan()
    {
        int normalizerTime = currentCase.plan.actions.Peek().time - 1;

        foreach (Action action in currentCase.plan.actions)
        {
            action.time -= normalizerTime;
        }
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
            if (a.CompareTag(Constants.TAG_TEAM_1))
                return Sector.DEFENSIVE;
            else
                return Sector.OFENSIVE;
        }

        if (a.transform.position.y > 100)
        {
            if (a.CompareTag(Constants.TAG_TEAM_2))
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
        action.cost = float.Parse(str[8]);
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
        TextWriter tw = new StreamWriter(Constants.DATA_BASE, true);

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

        //Debug.Log("CASO ATUAL: " + case_str);
        Case currentCase = StringCaseToCase(case_str.Split(Constants.SPLITTER));
        List<Result> similiarCases = GetResultsOfSimilarCases(currentCase);

        foreach (Result similarCase in similiarCases)
        {
            //Lista de [Descrição, Plano, Percentage, ID]
            cases.Add(new string[] { similarCase.matchCase.caseDescription[8].value,
                similarCase.matchCase.caseSolution[0].value,
                (similarCase.matchPercentage * 100).ToString("0.00"),
                similarCase.matchCase.caseDescription[0].value 
            });
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
        consultStructure.consultParams.Add(new ConsultParams(new List<int> { 1 }, 1f, new Equals()));                //Seed 1
        consultStructure.consultParams.Add(new ConsultParams(new List<int> { 6 }, 1f, new Equals()));                //deceptiveLevel 6
        consultStructure.consultParams.Add(new ConsultParams(new List<int> { 7 }, 1f, new Equals()));                //strategy 7
        consultStructure.consultParams.Add(new ConsultParams(new List<int> { 2 }, 1f, new MatrixSimilarity()));      //agentsRelationships 2
        consultStructure.consultParams.Add(new ConsultParams(new List<int> { 3 }, 1f, new MatrixSimilarity()));      //agentsGoalsRelationships 3
        consultStructure.consultParams.Add(new ConsultParams(new List<int> { 4 }, 1f, new CosineSimilarity()));      //agentsRelationships_distance_angle 4
        consultStructure.consultParams.Add(new ConsultParams(new List<int> { 5 }, 1f, new SectorSimilarity()));      //agentsBattleFieldLocalization 5

        // Realizando uma consulta na base de casos (lista já ordenada por maior score)
        List<Result> results = cbr.Retrieve(currentCase, consultStructure);

        foreach (var result in results)
            Debug.Log("Caso recuperado: " + result.matchCase.caseDescription[0].value + " com " + (result.matchPercentage * 100).ToString("0.00") + "% de similaridade");

        
        return results;
    }
}
