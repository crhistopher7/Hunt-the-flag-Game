using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Classe que realiza o controle do simulador e a comunicação com o CBDP
/// </summary>
public class SimulationController : MonoBehaviour
{
    private Client clientOfExecution;
    private SelectController selectController;
    private PathfinderPointsController pathfinderPointsController;
    private GameObject canvasSelectPathfinder;
    private GameObject canvasType;
    private GameObject canvasStrategy;
    private GameObject canvasResult;
    private GameObject canvasDescription;
    private GameObject canvasPainels;
    private GameObject textUISimilarCases;
    private InputField inputDescription;
    private InputField similarCaseInput;
    private List<AgentController> selectedAgents;
    private Dropdown dropdown;
    private RectTransform rectPanel;
    private RectTransform rectCanvas;
    private PathType pathType;
    private CBDP cbdp;
    private PlayerController pcTeam1;
    private PlayerController pcTeam2;
    private List<string[]> listOfSimilarCases;
    private bool hasPlan = false;
    private DateTime dateStartPlan;
    private Plan plan;


    public Vector3 pointOfCanvasPath;

    // Start is called before the first frame update
    void Start()
    {
        InitPlayers();
        FindComponents();
        SetVariables();
        Invoke(nameof(SearchSimillarCases), 0.5f);
    }

    void Update()
    {
        if (hasPlan)
            ExecutePlan();
    }

    /// <summary>
    /// Função que istancia os prefabs dos controladores de cada time de agentes
    /// </summary>
    public void InitPlayers()
    {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/" + Config.PLAYER_CONTROLLER_1);
        GameObject go = Instantiate(prefab);

        pcTeam1 = go.GetComponent<PlayerController>();
        pcTeam1.name = Config.PLAYER_CONTROLLER_1;

        prefab = Resources.Load<GameObject>("Prefabs/" + Config.PLAYER_CONTROLLER_2);
        go = Instantiate(prefab);

        pcTeam2 = go.GetComponent<PlayerController>();
        pcTeam2.name = Config.PLAYER_CONTROLLER_2;
    }

    /// <summary>
    /// Função que destrói os agentes do caso corrente e inicia os paineis de informação do caso
    /// </summary>
    public void EndCase()
    {
        //Destruir os agents
        var matchObjects = FindObjectsOfType<MatchBehaviour>();

        foreach (var matchObject in matchObjects)
            Destroy(matchObject.gameObject);

        canvasPainels.SetActive(false);
        canvasType.SetActive(true);
    }

    /// <summary>
    /// Função que recebe o tipo de caso (enganoso ou normal) e envia para o CBDP a informação
    /// </summary>
    /// <param name="type">string que diz 'DECEPTIVE' ou 'NORMAL'</param>
    public void SetSolutionType(string type)
    {
        if (type.Equals("DECEPTIVE"))
        {
            DeceptiveLevel level = cbdp.CalculateDeceptionLevel();
            cbdp.SetSolutionTypeInCase(level);
        }            
        else
            cbdp.SetSolutionTypeInCase(DeceptiveLevel.NOT_DECEPTIVE);

        canvasType.SetActive(false);
        canvasStrategy.SetActive(true);
    }

    /// <summary>
    /// Função que recebe a estratégia do caso (ofensivo ou defensivo) e envia para o CBDP a informação
    /// </summary>
    /// <param name="strategy">string que diz 'OFENSIVE' ou 'DEFENSIVE'</param>
    public void SetStrategyType(string strategy)
    {
        if (strategy.Equals(Strategy.OFENSIVE.ToString()))
            cbdp.SetStrategyTypeInCase(Strategy.OFENSIVE);
        else
            cbdp.SetStrategyTypeInCase(Strategy.DEFENSIVE);

        canvasStrategy.SetActive(false);
        canvasDescription.SetActive(true);
    }

    /// <summary>
    /// Função que recebe a estratégia do caso (ofensivo ou defensivo) e envia para o CBDP a informação
    /// </summary>
    public void SetDescription()
    {
        cbdp.SetDescriptionInCase(inputDescription.text.Replace(Config.SPLITTER, ' '));

        canvasDescription.SetActive(false);
        canvasResult.SetActive(true);
    }

    /// <summary>
    /// Função que recebe o resultado do caso (true ou false) e envia para o CBDP a informação. Chama o inicializador dos players
    /// </summary>
    /// <param name="result">resultado do caso</param>
    public void SetResult(bool result)
    {
        cbdp.SetResultInCase(result);
        canvasResult.SetActive(false);

        //save case
        cbdp.SaveCase(cbdp.GetCase());

        //RestartGame
        InitPlayers();
        canvasPainels.SetActive(true);
    }

    /// <summary>
    /// Função que executa recebe a escolha do plano escolhido e passa para a função que o executa
    /// </summary>
    public void ExecuteSimilarCase()
    {
        string strigIdCase = similarCaseInput.text;

        if (Int32.TryParse(strigIdCase, out int id) && listOfSimilarCases.Count > id && id >= 0)
        {
            Debug.Log("Executando o Plano: " + listOfSimilarCases[id][1]);
            ReceivePlan(listOfSimilarCases[id][1]);
        }
    }

    /// <summary>
    /// Função que seta valores iniciais em variáveis
    /// </summary>
    private void SetVariables()
    {
        gameObject.tag = clientOfExecution.GetPlayerControllerTag();
        selectedAgents = new List<AgentController>();
        clientOfExecution.SearchSimulationController();
        Invoke(nameof(ComandStartCase), 0.5f);
        EnableComponentSelectController();
        DesableComponentPathfinderPointsController();
    }

    /// <summary>
    /// Função que passa para o cbdp construir um caso a partir da lista dos agentes
    /// </summary>
    private void ComandStartCase()
    {
        cbdp.ConstructInitCase(pcTeam1.Agents, pcTeam2.Agents);
    }

    /// <summary>
    /// Função que busca componentes na cena e atribui a variáveis
    /// </summary>
    private void FindComponents()
    {
        clientOfExecution = GameObject.Find("Client").GetComponent<Client>();
        selectController = gameObject.GetComponent<SelectController>();
        pathfinderPointsController = gameObject.GetComponent<PathfinderPointsController>();
        canvasSelectPathfinder = Camera.main.transform.Find("CanvasSelectAStar").gameObject;
        canvasType = Camera.main.transform.Find("CanvasType").gameObject;
        canvasStrategy = Camera.main.transform.Find("CanvasStrategy").gameObject;
        canvasResult = Camera.main.transform.Find("CanvasResult").gameObject;
        canvasDescription = Camera.main.transform.Find("CanvasDescription").gameObject;
        canvasPainels = Camera.main.transform.Find("CanvasPainels").gameObject;
        textUISimilarCases = canvasPainels.transform.Find("Right Panel").transform.Find("Scroll View").transform.Find("Viewport").transform.Find("Content").transform.Find("CaseText").gameObject;
        similarCaseInput = canvasPainels.transform.Find("Right Panel").transform.Find("SimilarCaseInput").GetComponent<InputField>();
        inputDescription = canvasDescription.transform.Find("Panel").transform.Find("Description").GetComponentInChildren<InputField>();
        dropdown = canvasSelectPathfinder.transform.Find("Canvas").transform.Find("Panel").transform.Find("Text").GetComponentInChildren<Dropdown>();
        rectPanel = canvasSelectPathfinder.transform.Find("Canvas").transform.Find("Panel").GetComponent<RectTransform>();
        rectCanvas = canvasSelectPathfinder.GetComponent<RectTransform>();
        cbdp = GameObject.Find("CBDP").GetComponent<CBDP>();
    }

    private void SendObjectivesToAgents(Vector3Int objectivePosition, Vector3Int deceptivePosition)
    {
        Message send = new Message();
        foreach (AgentController agent in selectedAgents)
        {
            send.AddMessage("Moves", gameObject.tag, agent.name, pathType, objectivePosition, deceptivePosition);
            StartCoroutine(SendActionToCase("Move", agent, objectivePosition, deceptivePosition, pathType));
        }
        clientOfExecution.Send(send.ToString());
        selectedAgents.Clear();
    }

    private void ShowCanvasPath()
    {
        canvasPainels.SetActive(false);
        canvasSelectPathfinder.SetActive(true);
        Vector2 anchoredPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectCanvas, pointOfCanvasPath, Camera.main, out anchoredPos);
        anchoredPos = new Vector2(anchoredPos.x, anchoredPos.y);
        rectPanel.anchoredPosition = anchoredPos;
    }

    private void ShowArrowPathConstructor()
    {
        Color color = pathType.Equals(PathType.NORMAL) ? Color.green : Color.red;

        foreach (AgentController agent in selectedAgents)
            Utils.InstantiateLineDrawer(agent.transform.position, color);
        EnableComponentPathfinderPointsController();
    }

    public bool VerifySelectedGroupAgent(Vector2[] corners)
    {
        var Agents = CompareTag(Config.TAG_TEAM_1) ? pcTeam1.Agents : pcTeam2.Agents;

        //Quais agentes est�o dentro dos limites da box
        foreach (AgentController agent in Agents)
        {
            Vector3 positon = agent.transform.position;
            if (Utils.VerifyPointInLimits(positon, corners))
            {
                // O agente esta dentro da box, adicionar na lista
                if (!selectedAgents.Contains(agent))
                    selectedAgents.Add(agent);
            }
        }

        if (selectedAgents.Count >= 1)
            return true;
        return false;
    }

    public void EnableComponentSelectController()
    {
        selectController.enabled = true;
    }

    public void EnableComponentPathfinderPointsController()
    {
        pathfinderPointsController.enabled = true;
    }

    public void DesableComponentSelectController()
    {
        selectController.enabled = false;
        // TODO Se toogle de 'estou consultando planos que usam esses agentes estiver true, pesquisar e não perguntar path'
        ShowCanvasPath();
    }

    public void DesableComponentPathfinderPointsController()
    {
        pathfinderPointsController.enabled = false;
    }

    public bool VerifySelectedSingleAgent(Vector3 point)
    {
        Ray ray = Camera.main.ScreenPointToRay(point);

        if (Physics.Raycast(ray, out RaycastHit hit, 50000.0f))
        {
            if (hit.transform.CompareTag(gameObject.tag))
            {
                if (hit.transform.gameObject.TryGetComponent<AgentController>(out AgentController agent))
                {
                    selectedAgents.Add(agent);
                    return true;
                }
            }
        }
        return false;
    }

    public void SetPathfinder()
    {
        pathType = (PathType)dropdown.value;
        
        canvasSelectPathfinder = Camera.main.transform.Find("CanvasSelectAStar").gameObject;
        canvasSelectPathfinder.SetActive(false);
        canvasPainels.SetActive(true);
        ShowArrowPathConstructor();
        dropdown.value = 0;
    }

    public PathType GetPathType()
    {
        return pathType;
    }

    public void ReceiveObjectivePositions(Vector3Int objectivePosition, Vector3Int deceptivePosition)
    {
        DesableComponentPathfinderPointsController();
        SendObjectivesToAgents(objectivePosition, deceptivePosition);
        EnableComponentSelectController();
    }

    public void ReceiveMove(string name, string team, Vector3Int objectivePosition, Vector3Int deceptivePosition, PathType pathType)
    {
        if (team.Equals(Config.TAG_TEAM_1))
            pcTeam1.ReceiveMove(name, objectivePosition, deceptivePosition, pathType);
        else
            pcTeam2.ReceiveMove(name, objectivePosition, deceptivePosition, pathType);
    }

    private void ExecutePlan()
    {
        var action = plan.actions.Peek();

        if (action.time <= (DateTime.Now - dateStartPlan).TotalSeconds)
        {
            // Pode executar a proxima acao do plano
            action = plan.actions.Dequeue();
            Debug.Log("Executando ação: " + action.ToString());
            ExecuteAction(action);

            if (plan.actions.Count == 0)
            {
                hasPlan = false;
            }
        }
    }

    private void ExecuteAction(Action action)
    {
        switch (action.action)
        {
            case "Move":
                {
                    Vector3Int objetivePosition, deceptivePosition = new Vector3Int();
                    Vector3 position = GetPositionByName(action.agent);
                    // Verificar se existe um objetivo
                    if (action.objetive.Equals(""))
                        //n�o existe, ent�o usar o distance_direction
                        objetivePosition = GetPositionByDistanceDirection(action.distance_direction, position);
                    else
                        //usar a localiza��o do objetivo
                        objetivePosition = GetPositionByName(action.objetive);

                    if (!action.pathType.Equals(PathType.NORMAL))
                        deceptivePosition = GetPositionByDistanceDirection(action.distance_directionDeceptive, position);

                    // Mandando movimenta��o para o servidor
                    Message send = new Message();
                    send.AddMessage("Moves", gameObject.tag, action.agent, action.pathType, objetivePosition, deceptivePosition);
                    clientOfExecution.Send(send.ToString());
                    break;
                }
            default:
                break;
        }
    }

    private Vector3Int GetPositionByDistanceDirection(string objetive, Vector3 positionAgent)
    {
        var distance = objetive.Split('-')[0];
        var direction = objetive.Split('-')[1];

        int maxDistance = 0;
        int minDistance = 0;

        int maxDirection = 0;
        int minDirection = 0;

        switch (distance)
        {
            case "VC":
                maxDistance = 87;
                minDistance = 0;
                break;
            case "C":
                maxDistance = 175;
                minDistance = 88;
                break;
            case "A":
                maxDistance = 352;
                minDistance = 176;
                break;
            case "F":
                maxDistance = 706;
                minDistance = 353;
                break;
            case "VF":
                maxDistance = 1414;
                minDistance = 707;
                break;
            default:
                break;
        }

        switch (direction)
        {
            case "F":
                maxDirection = 112;
                minDirection = 68;
                break;
            case "RF":
                maxDirection = 67;
                minDirection = 23;
                break;
            case "LF":
                maxDirection = 157;
                minDirection = 113;
                break;
            case "R":
                maxDirection = 382;
                minDirection = 338;
                break;
            case "L":
                maxDirection = 202;
                minDirection = 158;
                break;
            case "B":
                maxDirection = 292;
                minDirection = 248;
                break;
            case "RB":
                maxDirection = 337;
                minDirection = 293;
                break;
            case "LB":
                maxDirection = 247;
                minDirection = 203;
                break;
            default:
                break;

        }

        // Pegar valores aleat�rios entre os intervalos determinados
        // repetir at� que a posi��o seja poss�vel de atingir
        Vector3Int position;
        LogicMap point;
        var AStar = GameObject.Find("Pathfinder").GetComponent<AStar>();
        var random = new System.Random();
        do
        {

            double h = random.Next(minDistance, maxDistance);
            double angle = random.Next(minDirection, maxDirection);

            position = Utils.PositionByDistanceAndAngle(angle, h, new Vector2(positionAgent.x, positionAgent.y));
            point = AStar.GetTileByPosition(Vector3Int.FloorToInt(new Vector3Int(position.x, position.y, 0)) / Config.MAP_OFFSET);

        } while (point == null || !point.Walkable);

        return position / Config.MAP_OFFSET;
    }

    private Vector3Int GetPositionByName(string objetive)
    {
        var gameObject = GameObject.Find(objetive);

        return Vector3Int.FloorToInt(new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, 0) / Config.MAP_OFFSET);
    }

    public void ReceivePlan(string plan)
    {
        this.plan = new Plan(plan);
        dateStartPlan = DateTime.Now;
        hasPlan = true;
    }

    private IEnumerator SendActionToCase(string str_action, AgentController agent, Vector3Int objetivePosition, Vector3Int deceptivePosition, PathType pathType)
    {
        Action action = new Action
        {

            //action
            action = str_action,

            //agent
            agent = agent.name,

            //actionDefinition
            pathType = pathType,

            // objetive
            objetive = Utils.GetObjetive(Input.mousePosition),

            //time
            time = (int)Math.Round((DateTime.Now - cbdp.initTime).TotalSeconds)
        };

        //distance_direction
        Distance distance = CBDPUtils.CalculeDistance(agent.transform.position, objetivePosition);
        Direction direction = CBDPUtils.CalculeDirection(objetivePosition.x, objetivePosition.y);

        action.distance_direction = distance.ToString() + '-' + direction.ToString();

        //distance_direction Deceptive objetive
        distance = CBDPUtils.CalculeDistance(agent.transform.position, deceptivePosition);
        direction = CBDPUtils.CalculeDirection(deceptivePosition.x, deceptivePosition.y);

        action.distance_directionDeceptive = distance.ToString() + '-' + direction.ToString();

        //send to plan
        cbdp.PlanAddAction(action);

        yield return null;
    }

    private void SearchSimillarCases()
    {
        //Lista de [Descrição, Plano]
        listOfSimilarCases = cbdp.SearchSimilarCases();
        // preencher o canvasPainels
        Text text = textUISimilarCases.GetComponent<Text>();
        string str = "";
        int i = 0;
        foreach (string[] c in listOfSimilarCases)
        {
            str += "Case " + i.ToString();
            str += " " + c[0];
            str += "\n\n";
            i++;
        }

        text.text = str;
    }
}
