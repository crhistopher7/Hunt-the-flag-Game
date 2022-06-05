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
        using var reader = new StreamReader(Config.DATA_BASE);
        Debug.Log("Lendo " + Config.DATA_BASE);

        if (!reader.EndOfStream)
        {
            var header = reader.ReadLine();
            features = header.Split(Config.SPLITTER);
            Debug.Log("Cabeçalho: " + header);

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                Debug.Log("Lendo linha: " + line);

                var values = line.Split(Config.SPLITTER);
                Case c = CaseToCase(values);
                cbr.AddCase(c);
            }
        }
        Debug.Log("Base de casos carregada!");
    }


    private Case CaseToCase(string[] values)
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
        caso.caseDescription.Add(new CaseFeature(5, features[5], typeof(string), values[5]));
        //agentsGoalsRelationships_int
        caso.caseDescription.Add(new CaseFeature(6, features[6], typeof(string), values[6]));
        //agentsBattleFieldLocalization
        caso.caseDescription.Add(new CaseFeature(7, features[7], typeof(string), values[7]));
        //deceptiveLevel
        caso.caseDescription.Add(new CaseFeature(8, features[8], typeof(string), values[8]));
        //strategy
        caso.caseDescription.Add(new CaseFeature(9, features[9], typeof(string), values[9]));
        //description
        caso.caseDescription.Add(new CaseFeature(10, features[10], typeof(string), values[10]));
        //result
        caso.caseDescription.Add(new CaseFeature(11, features[11], typeof(string), values[11]));

        //--------------- Estruturacao da descricao da solucao do caso
        //Planning
        caso.caseSolution.Add(new CaseFeature(0, features[12], typeof(string), values[12]));

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

        if (countDeceptiveActions / countTotalActions >= Config.LIMIAR_HIGHLY_DECEPTIVE)
            return DeceptiveLevel.HIGHLY_DECEPTIVE;
        if (countDeceptiveActions / countTotalActions >= Config.LIMIAR_PARTIALLY_DECEPTIVE)
            return DeceptiveLevel.PARTIALLY_DECEPTIVE;
        if (countDeceptiveActions / countTotalActions >= Config.LIMIAR_LITTLE_DECEPTIVE)
            return DeceptiveLevel.LITTLE_DECEPTIVE;

        return DeceptiveLevel.NOT_DECEPTIVE;
    }

    public void StartFile()
    {
        string str = "";
        str += "caseID" + Config.SPLITTER;
        str += "seed" + Config.SPLITTER;
        str += "agentsRelationships" + Config.SPLITTER;
        str += "agentsGoalsRelationships" + Config.SPLITTER;
        str += "agentsRelationships_distance_angle" + Config.SPLITTER;
        str += "agentsRelationships_int" + Config.SPLITTER;
        str += "agentsGoalsRelationships_int" + Config.SPLITTER;
        str += "agentsBattleFieldLocalization" + Config.SPLITTER;
        str += "deceptiveLevel" + Config.SPLITTER;
        str += "strategy" + Config.SPLITTER;
        str += "description" + Config.SPLITTER;
        str += "result" + Config.SPLITTER;

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
        currentCase.seedMap = GameObject.Find("Client").GetComponent<Client>().seed;

        //matriz de distancia/dire��o entre os agentes e agentes (string e int)
        List<AgentController> agents_list = new List<AgentController>();
        agents_list.AddRange(CBDPUtils.OrderAgentList(agentsTeam1));
        agents_list.AddRange(CBDPUtils.OrderAgentList(agentsTeam2));

        currentCase.matrix_agents = new string[agents_list.Count, agents_list.Count];
        currentCase.matrix_agents_distance_angle = new string[agents_list.Count, agents_list.Count];
        currentCase.int_matrix_agents = new int[agents_list.Count, agents_list.Count];

        for (int i = 0; i < agents_list.Count; i++)
        {
            for (int j = 0; j < agents_list.Count; j++)
            {
                if (i <= j)
                {
                    //vazio a parte superior da matrix 
                    currentCase.matrix_agents[i, j] = "";
                    currentCase.matrix_agents_distance_angle[i, j] = "";
                    currentCase.int_matrix_agents[i, j] = 0;
                    continue;
                }

                float[] sensorDirection = Sensor.CheckDirection(agents_list[i].transform, agents_list[j].transform);

                Distance distance = CBDPUtils.CalculeDistance(agents_list[i].transform.position, agents_list[j].transform.position);
                Direction direction = CBDPUtils.CalculeDirection(sensorDirection[0], sensorDirection[1]);

                float distance_float = CBDPUtils.GetDistanceByAStarPath(agents_list[i].transform.position, agents_list[j].transform.position);
                float angle = Vector3.Angle(agents_list[i].transform.position, agents_list[j].transform.position);

                currentCase.matrix_agents[i, j] = distance.ToString() + "-" + direction.ToString();
                currentCase.matrix_agents_distance_angle[i, j] = distance_float.ToString() + "-" + angle.ToString();
                currentCase.int_matrix_agents[i, j] = (int)distance + (int)direction;
            }
        }

        //matriz de distancia/dire��o entre os agentes e objetivos (string e int)
        GameObject[] objectives = GameObject.FindGameObjectsWithTag(Config.TAG_FLAG);

        currentCase.matrix_objetives = new string[agents_list.Count, objectives.Length];
        currentCase.int_matrix_objetives = new int[agents_list.Count, objectives.Length];

        for (int i = 0; i < agents_list.Count; i++)
        {
            for (int j = 0; j < objectives.Length; j++)
            {
                float[] sensorDirection = Sensor.CheckDirection(agents_list[i].transform, objectives[j].transform);

                Distance distance = CBDPUtils.CalculeDistance(agents_list[i].transform.position, objectives[j].transform.position);
                Direction direction = CBDPUtils.CalculeDirection(sensorDirection[0], sensorDirection[1]);

                currentCase.matrix_objetives[i, j] = distance.ToString() + '-' + direction.ToString();
                currentCase.int_matrix_objetives[i, j] = (int)distance + (int)direction;
            }
        }

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
        Debug.Log("Adicionando Action: " + action.ToString());
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
        //consultStructure.consultParams.Add(new ConsultParams(new List<int> { 1 }, 0.1f, new Equals()));            //Seed
        consultStructure.consultParams.Add(new ConsultParams(new List<int> { 8 }, 0.1f, new Equals()));              //deceptiveLevel
        consultStructure.consultParams.Add(new ConsultParams(new List<int> { 2 }, 1f, new MatrixSimilarity()));    //agentsRelationships
        consultStructure.consultParams.Add(new ConsultParams(new List<int> { 3 }, 0.2f, new MatrixSimilarity()));  //agentsGoalsRelationships
        //consultStructure.consultParams.Add(new ConsultParams(new List<int> { 4 }, 0.3f, new MatrixSimilarity()));  //agentsRelationships_distance_angle
        //consultStructure.consultParams.Add(new ConsultParams(new List<int> { 7 }, 0.1f, new SectorSimilarity()));  //agentsBattleFieldLocalization

        // Realizando uma consulta na base de casos (lista já ordenada por maior score)
        List<Result> results = cbr.Retrieve(currentCase, consultStructure);

        //Debug.Log("Caso recuperado: " + result.matchCase.caseDescription[0].value + " com " + (result.matchPercentage * 100).ToString("0.00") + "% de similaridade");
        return results;
    }
}
