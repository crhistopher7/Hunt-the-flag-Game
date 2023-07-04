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
        //friendAgentsRelationships
        caso.caseDescription.Add(new CaseFeature(2, features[2], typeof(string), values[2]));
        //enemyAgentsRelationships
        //caso.caseDescription.Add(new CaseFeature(3, features[3], typeof(string), values[3]));
        //friendAgentsGoalsRelationships
        caso.caseDescription.Add(new CaseFeature(3, features[3], typeof(string), values[3]));
        //enemyAgentsGoalsRelationships
        //caso.caseDescription.Add(new CaseFeature(5, features[5], typeof(string), values[5]));
        //friendAgentsRelationships_distance_angle
        caso.caseDescription.Add(new CaseFeature(4, features[4], typeof(string), values[4]));
        //enemyAgentsRelationships_distance_angle
        //caso.caseDescription.Add(new CaseFeature(7, features[7], typeof(string), values[7]));
        //friendAgentsBattleFieldLocalization
        caso.caseDescription.Add(new CaseFeature(5, features[5], typeof(string), values[5]));
        //enemyAgentsBattleFieldLocalization
        //caso.caseDescription.Add(new CaseFeature(9, features[9], typeof(string), values[9]));
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
        str += "friendAgentsRelationships" + Constants.SPLITTER;
        //str += "enemyAgentsRelationships" + Constants.SPLITTER;
        str += "friendAgentsGoalsRelationships" + Constants.SPLITTER;
        //str += "enemyAgentsGoalsRelationships" + Constants.SPLITTER;
        str += "friendAgentsRelationships_distance_angle" + Constants.SPLITTER;
        //str += "enemyAgentsRelationships_distance_angle" + Constants.SPLITTER;
        str += "friendAgentsBattleFieldLocalization" + Constants.SPLITTER;
        //str += "enemyAgentsBattleFieldLocalization" + Constants.SPLITTER;
        str += "deceptiveLevel" + Constants.SPLITTER;
        str += "strategy" + Constants.SPLITTER;
        str += "description" + Constants.SPLITTER;
        str += "result" + Constants.SPLITTER;

        str += "Planning";
        //salvando 
        Save(str);
    }

    public void UpdateWhoEndtheCase(string agent, string goal)
    {

        currentCase.plan.UpdateLastAction(agent, goal);
    }

    public void ConstructInitCase(List<AgentController> agentsTeam1, List<AgentController> agentsTeam2, int id)
    {
        currentCase = new CaseCBDP();

        // id do caso
        currentCase.caseId = id;

        // Seed do mapa
        currentCase.seedMap = 1; //GameObject.Find("Client").GetComponent<Client>().seed;

        //matriz de distancia/direção entre os agentes e agentes (qualitative e dist_angle)
        List<AgentController> friend_agents_list = new List<AgentController>();
        friend_agents_list.AddRange(CBDPUtils.OrderAgentList(agentsTeam1));
        //friend_agents_list.AddRange(agentsTeam1);
        currentCase.matrix_friend_agents = new Qualitative[friend_agents_list.Count, friend_agents_list.Count];
        currentCase.matrix_friend_agents_distance_angle = new Qualitative[friend_agents_list.Count, friend_agents_list.Count];

        //friend
        for (int i = 0; i < friend_agents_list.Count; i++)
        {
            for (int j = 0; j < friend_agents_list.Count; j++)
            {
                if (i <= j)
                {
                    //vazio a parte superior da matrix 
                    currentCase.matrix_friend_agents[i, j] = new Qualitative();
                    currentCase.matrix_friend_agents_distance_angle[i, j] = new Qualitative();
                    continue;
                }

                float[] sensorDirection = Sensor.CheckDirection(friend_agents_list[i].transform, friend_agents_list[j].transform);

                Distance distance = CBDPUtils.CalculeDistance(friend_agents_list[i].transform.position, friend_agents_list[j].transform.position);
                Direction direction = CBDPUtils.CalculeDirection(sensorDirection[0], sensorDirection[1]);

                //float distance_float = CBDPUtils.GetDistanceByAStarPath(agents_list[i].transform.position, agents_list[j].transform.position);
                float distance_float = Vector3.Distance(friend_agents_list[i].transform.position, friend_agents_list[j].transform.position);
                float angle = Vector3.Angle(friend_agents_list[i].transform.position, friend_agents_list[j].transform.position);

                currentCase.matrix_friend_agents[i, j] = new Qualitative(distance, direction);
                currentCase.matrix_friend_agents_distance_angle[i, j] = new Qualitative(angle, distance_float);
            }
        }

        /*List<AgentController> enemy_agents_list = new List<AgentController>();
        enemy_agents_list.AddRange(CBDPUtils.OrderAgentList(agentsTeam2));
        currentCase.matrix_enemy_agents = new Qualitative[enemy_agents_list.Count, enemy_agents_list.Count];
        currentCase.matrix_enemy_agents_distance_angle = new Qualitative[enemy_agents_list.Count, enemy_agents_list.Count];

        //enemy
        for (int i = 0; i < enemy_agents_list.Count; i++)
        {
            for (int j = 0; j < enemy_agents_list.Count; j++)
            {
                if (i <= j)
                {
                    //vazio a parte superior da matrix 
                    currentCase.matrix_enemy_agents[i, j] = new Qualitative();
                    currentCase.matrix_enemy_agents_distance_angle[i, j] = new Qualitative();
                    continue;
                }

                float[] sensorDirection = Sensor.CheckDirection(enemy_agents_list[i].transform, enemy_agents_list[j].transform);

                Distance distance = CBDPUtils.CalculeDistance(enemy_agents_list[i].transform.position, enemy_agents_list[j].transform.position);
                Direction direction = CBDPUtils.CalculeDirection(sensorDirection[0], sensorDirection[1]);

                //float distance_float = CBDPUtils.GetDistanceByAStarPath(agents_list[i].transform.position, agents_list[j].transform.position);
                float distance_float = Vector3.Distance(enemy_agents_list[i].transform.position, enemy_agents_list[j].transform.position);
                float angle = Vector3.Angle(enemy_agents_list[i].transform.position, enemy_agents_list[j].transform.position);

                currentCase.matrix_enemy_agents[i, j] = new Qualitative(distance, direction);
                currentCase.matrix_enemy_agents_distance_angle[i, j] = new Qualitative(angle, distance_float);
            }
        }*/


        //matriz de distancia/direção entre os agentes e objetivos (Qualitative)
        GameObject[] objectives = GameObject.FindGameObjectsWithTag(Constants.TAG_FLAG);
        List<GameObject> friend_objectives = CBDPUtils.Filter(objectives, Constants.TAG_TEAM_2);

        currentCase.matrix_friend_objetives = new Qualitative[friend_agents_list.Count, friend_objectives.Count];
        
        for (int i = 0; i < friend_agents_list.Count; i++)
        {
            for (int j = 0; j < friend_objectives.Count; j++)
            {
                float[] sensorDirection = Sensor.CheckDirection(friend_agents_list[i].transform, friend_objectives[j].transform);
                Distance distance = CBDPUtils.CalculeDistance(friend_agents_list[i].transform.position, friend_objectives[j].transform.position,
                    false, false, friend_agents_list[i].tag + " - " + friend_agents_list[i].name + " and " + friend_objectives[j].name);
                Direction direction = CBDPUtils.CalculeDirection(sensorDirection[0], sensorDirection[1]);

                currentCase.matrix_friend_objetives[i, j] = new Qualitative(distance, direction);
            }
        }

        /*List<GameObject> enemy_objectives = CBDPUtils.Filter(objectives, Constants.TAG_TEAM_1);
        currentCase.matrix_enemy_objetives = new Qualitative[enemy_agents_list.Count, enemy_objectives.Count];

        for (int i = 0; i < enemy_agents_list.Count; i++)
        {
            for (int j = 0; j < enemy_objectives.Count; j++)
            {
                float[] sensorDirection = Sensor.CheckDirection(enemy_agents_list[i].transform, enemy_objectives[j].transform);
                Distance distance = CBDPUtils.CalculeDistance(enemy_agents_list[i].transform.position, enemy_objectives[j].transform.position,
                    false, false, enemy_agents_list[i].tag + " - " + enemy_agents_list[i].name + " and " + enemy_objectives[j].name);
                Direction direction = CBDPUtils.CalculeDirection(sensorDirection[0], sensorDirection[1]);

                currentCase.matrix_enemy_objetives[i, j] = new Qualitative(distance, direction);
            }
        }*/

        //vetor setores
        currentCase.vector_friend_sector = new Sector[friend_agents_list.Count];
        for (int i = 0; i < friend_agents_list.Count; i++)
            currentCase.vector_friend_sector[i] = CalculeSector(friend_agents_list[i]);

        //currentCase.vector_enemy_sector = new Sector[enemy_agents_list.Count];
        //for (int i = 0; i < enemy_agents_list.Count; i++)
        //    currentCase.vector_enemy_sector[i] = CalculeSector(enemy_agents_list[i]);
        
        currentCase.strategy = Strategy.OFENSIVE;
        currentCase.solutionType = DeceptiveLevel.NOT_DECEPTIVE;

        //init plan
        currentCase.plan = new Plan
        {
            caseid = currentCase.caseId,
            actions = new Queue<Action>()
        };

        //tirar print do caso, sem plano
        SimulationController simulation = GameObject.Find("SimulationController").GetComponent<SimulationController>();
        simulation.TakeAPicture("Case");
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


    private List<Result> GetResultsOfSimilarCases(Case searchCase)
    {
        List<ConsultStructure> structsList = Structures();
        List<Result> allResults = new List<Result>();

        foreach (var structure in structsList)
        {
            List<Result> results = cbr.Retrieve(searchCase, structure);

            foreach (var result in results)
                Debug.Log("Usando a estrutura ("+structure.description+"), o Caso " + result.matchCase.caseDescription[0].value + " teve " + (result.matchPercentage * 100).ToString("0.00") + "% de similaridade");

            allResults.AddRange(results);
        }

        return allResults;
    }

    private List<ConsultStructure> Structures()
    {
        List<ConsultStructure> list = new List<ConsultStructure>();
        ConsultStructure consultStructure;

        //friendAgentsRelationships 2
        List<int> matrixList = new List<int> { 2 };
        //friendAgentsGoalsRelationships 3
        List<int> matrixList2 = new List<int> { 3 };
        //friendAgentsRelationships_distance_angle 4
        List<int> numMatrixList = new List<int> { 4 };
        //friendAgentsBattleFieldLocalization 5
        List<int> vetorList = new List<int> { 5 };
        //deceptiveLevel 6, strategy 7
        List<int> equalsList = new List<int> { 6, 7 };
        

        /*//----------------------------Qualitative Matriz and Cosine---------------------------------
        consultStructure = new ConsultStructure();
        consultStructure.globalSimilarity = new EuclideanDistance(consultStructure);
        consultStructure.description = "Qualitative Matriz and Cosine";
        consultStructure.consultParams.Add(new ConsultParams(matrixList, 1f, new MatrixSimilarity()));
        consultStructure.consultParams.Add(new ConsultParams(matrixList2, 1f, new MatrixSimilarity()));
        consultStructure.consultParams.Add(new ConsultParams(numMatrixList, 1f, new CosineSimilarity()));
        //consultStructure.consultParams.Add(new ConsultParams(vetorList, 1f, new SectorSimilarity()));
        //consultStructure.consultParams.Add(new ConsultParams(equalsList, 1f, new Equals()));
        list.Add(consultStructure);

        //----------------------------Qualitative Matriz and Canberra---------------------------------
        consultStructure = new ConsultStructure();
        consultStructure.globalSimilarity = new EuclideanDistance(consultStructure);
        consultStructure.description = "Qualitative Matriz and Canberra";
        consultStructure.consultParams.Add(new ConsultParams(matrixList, 1f, new MatrixSimilarity()));
        consultStructure.consultParams.Add(new ConsultParams(matrixList2, 1f, new MatrixSimilarity()));
        consultStructure.consultParams.Add(new ConsultParams(numMatrixList, 1f, new CanberraSimilarity()));
        //consultStructure.consultParams.Add(new ConsultParams(vetorList, 1f, new SectorSimilarity()));
        //consultStructure.consultParams.Add(new ConsultParams(equalsList, 1f, new Equals()));
        list.Add(consultStructure);

        //----------------------------Jaccard and Cosine---------------------------------
        consultStructure = new ConsultStructure();
        consultStructure.globalSimilarity = new EuclideanDistance(consultStructure);
        consultStructure.description = "Jaccard and Cosine";
        consultStructure.consultParams.Add(new ConsultParams(matrixList, 1f, new JaccardSimilarity()));
        consultStructure.consultParams.Add(new ConsultParams(matrixList2, 1f, new JaccardSimilarity()));
        consultStructure.consultParams.Add(new ConsultParams(numMatrixList, 1f, new CosineSimilarity()));
        //consultStructure.consultParams.Add(new ConsultParams(vetorList, 1f, new SectorSimilarity()));
        //consultStructure.consultParams.Add(new ConsultParams(equalsList, 1f, new Equals()));
        list.Add(consultStructure);

        //----------------------------Jaccard and Canberra---------------------------------
        consultStructure = new ConsultStructure();
        consultStructure.globalSimilarity = new EuclideanDistance(consultStructure);
        consultStructure.description = "Jaccard and Canberra";
        consultStructure.consultParams.Add(new ConsultParams(matrixList, 1f, new JaccardSimilarity()));
        consultStructure.consultParams.Add(new ConsultParams(matrixList2, 1f, new JaccardSimilarity()));
        consultStructure.consultParams.Add(new ConsultParams(numMatrixList, 1f, new CanberraSimilarity()));
        //consultStructure.consultParams.Add(new ConsultParams(vetorList, 1f, new SectorSimilarity()));
        //consultStructure.consultParams.Add(new ConsultParams(equalsList, 1f, new Equals()));
        list.Add(consultStructure);

        //----------------------------Sorensen and Cosine---------------------------------
        consultStructure = new ConsultStructure();
        consultStructure.globalSimilarity = new EuclideanDistance(consultStructure);
        consultStructure.description = "Sorensen and Cosine";
        consultStructure.consultParams.Add(new ConsultParams(matrixList, 1f, new SorensenSimilarity()));
        consultStructure.consultParams.Add(new ConsultParams(matrixList2, 1f, new SorensenSimilarity()));
        consultStructure.consultParams.Add(new ConsultParams(numMatrixList, 1f, new CosineSimilarity()));
        //consultStructure.consultParams.Add(new ConsultParams(vetorList, 1f, new SectorSimilarity()));
        //consultStructure.consultParams.Add(new ConsultParams(equalsList, 1f, new Equals()));
        list.Add(consultStructure);

        //----------------------------Sorensen and Canberra---------------------------------
        consultStructure = new ConsultStructure();
        consultStructure.globalSimilarity = new EuclideanDistance(consultStructure);
        consultStructure.description = "Sorensen and Canberra";
        consultStructure.consultParams.Add(new ConsultParams(matrixList, 1f, new SorensenSimilarity()));
        consultStructure.consultParams.Add(new ConsultParams(matrixList2, 1f, new SorensenSimilarity()));
        consultStructure.consultParams.Add(new ConsultParams(numMatrixList, 1f, new CanberraSimilarity()));
        //consultStructure.consultParams.Add(new ConsultParams(vetorList, 1f, new SectorSimilarity()));
        //consultStructure.consultParams.Add(new ConsultParams(equalsList, 1f, new Equals()));
        list.Add(consultStructure);


        //----------------------------Hamming and Cosine---------------------------------
        consultStructure = new ConsultStructure();
        consultStructure.globalSimilarity = new EuclideanDistance(consultStructure);
        consultStructure.description = "Hamming and Cosine";
        consultStructure.consultParams.Add(new ConsultParams(matrixList, 1f, new HammingSimilarity()));
        consultStructure.consultParams.Add(new ConsultParams(matrixList2, 1f, new HammingSimilarity()));
        consultStructure.consultParams.Add(new ConsultParams(numMatrixList, 1f, new CosineSimilarity()));
        //consultStructure.consultParams.Add(new ConsultParams(vetorList, 1f, new SectorSimilarity()));
        //consultStructure.consultParams.Add(new ConsultParams(equalsList, 1f, new Equals()));
        list.Add(consultStructure);

        //----------------------------Hamming and Canberra---------------------------------
        consultStructure = new ConsultStructure();
        consultStructure.globalSimilarity = new EuclideanDistance(consultStructure);
        consultStructure.description = "Hamming and Canberra";
        consultStructure.consultParams.Add(new ConsultParams(matrixList, 1f, new HammingSimilarity()));
        consultStructure.consultParams.Add(new ConsultParams(matrixList2, 1f, new HammingSimilarity()));
        consultStructure.consultParams.Add(new ConsultParams(numMatrixList, 1f, new CanberraSimilarity()));
        //consultStructure.consultParams.Add(new ConsultParams(vetorList, 1f, new SectorSimilarity()));
        //consultStructure.consultParams.Add(new ConsultParams(equalsList, 1f, new Equals()));
        list.Add(consultStructure);

        //----------------------------Hamming Adapted and Cosine---------------------------------
        consultStructure = new ConsultStructure();
        consultStructure.globalSimilarity = new EuclideanDistance(consultStructure);
        consultStructure.description = "Hamming Adapted and Cosine";
        consultStructure.consultParams.Add(new ConsultParams(matrixList, 1f, new HammingSimilarityAdapted()));
        consultStructure.consultParams.Add(new ConsultParams(matrixList2, 1f, new HammingSimilarityAdapted()));
        consultStructure.consultParams.Add(new ConsultParams(numMatrixList, 1f, new CosineSimilarity()));
        //consultStructure.consultParams.Add(new ConsultParams(vetorList, 1f, new SectorSimilarity()));
        //consultStructure.consultParams.Add(new ConsultParams(equalsList, 1f, new Equals()));
        list.Add(consultStructure);

        //----------------------------Hamming Adapted and Canberra---------------------------------
        consultStructure = new ConsultStructure();
        consultStructure.globalSimilarity = new EuclideanDistance(consultStructure);
        consultStructure.description = "Hamming Adapted and Canberra";
        consultStructure.consultParams.Add(new ConsultParams(matrixList, 1f, new HammingSimilarityAdapted()));
        consultStructure.consultParams.Add(new ConsultParams(matrixList2, 1f, new HammingSimilarityAdapted()));
        consultStructure.consultParams.Add(new ConsultParams(numMatrixList, 1f, new CanberraSimilarity()));
        //consultStructure.consultParams.Add(new ConsultParams(vetorList, 1f, new SectorSimilarity()));
        //consultStructure.consultParams.Add(new ConsultParams(equalsList, 1f, new Equals()));
        list.Add(consultStructure);
        */
        //----------------------------ALL---------------------------------
        consultStructure = new ConsultStructure();
        consultStructure.globalSimilarity = new EuclideanDistance(consultStructure);
        consultStructure.description = "All functions";
        consultStructure.consultParams.Add(new ConsultParams(matrixList, 1f, new MatrixSimilarity()));
        consultStructure.consultParams.Add(new ConsultParams(matrixList, 1f, new JaccardSimilarity()));
        consultStructure.consultParams.Add(new ConsultParams(matrixList, 1f, new HammingSimilarity()));
        consultStructure.consultParams.Add(new ConsultParams(matrixList, 1f, new HammingSimilarityAdapted()));
        consultStructure.consultParams.Add(new ConsultParams(matrixList, 1f, new SorensenSimilarity()));
        consultStructure.consultParams.Add(new ConsultParams(matrixList2, 1f, new MatrixSimilarity()));
        consultStructure.consultParams.Add(new ConsultParams(matrixList2, 1f, new JaccardSimilarity()));
        consultStructure.consultParams.Add(new ConsultParams(matrixList2, 1f, new HammingSimilarity()));
        consultStructure.consultParams.Add(new ConsultParams(matrixList2, 1f, new HammingSimilarityAdapted()));
        consultStructure.consultParams.Add(new ConsultParams(matrixList2, 1f, new SorensenSimilarity()));
        consultStructure.consultParams.Add(new ConsultParams(numMatrixList, 1f, new CosineSimilarity()));
        consultStructure.consultParams.Add(new ConsultParams(numMatrixList, 1f, new CanberraSimilarity()));
        //consultStructure.consultParams.Add(new ConsultParams(vetorList, 1f, new SectorSimilarity()));
        //consultStructure.consultParams.Add(new ConsultParams(equalsList, 1f, new Equals()));
        list.Add(consultStructure);

        return list;
    } 
}
