using AnotherFileBrowser.Windows;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
    private GameObject contentButtonsCase;
    private InputField inputDescription;
    private Text thresholdSliderText;
    private List<AgentController> selectedAgents;
    private List<SetNewPosition> repositionObjects;
    private Dropdown dropdown;
    private RectTransform rectPanel;
    private RectTransform rectCanvas;
    private PathType pathType;
    private CBDP cbdp;
    private PlayerController pcTeam1;
    private PlayerController pcTeam2;
    private MapGenerator mapGenerator;
    private List<string[]> listOfSimilarCases;
    private bool hasPlan = false;
    private DateTime dateStartPlan;
    private Plan plan;
    private int selectedSimilarCaseId;

    //Configuration Variables
    public InputField inputRetrivalCasesNumber;
    public Slider sliderRetrivalCasesThreshold;
    public InputField inputNumberOfAgents;
    public string MAP_HEIGHTMAP;
    public string MAP_SATELLITE;
    int[] ids = { 1477686966, 1931235537, 1902959905 };
    int id = 0;
    //Casos q dão erro 471, 277, 209

    // Start is called before the first frame update
    void Start()
    {
        FindComponents();
        InitPlayers();
        SetVariables();
        Invoke(nameof(SearchSimillarCases), 1f);
        Debug.Log("End SimulationController Start");
    }

    void Update()
    {
        if (hasPlan)
            ExecutePlan();
    }

    /// <summary>
    /// Função que abre o FileBrowser para o usuário selecionar as imagens do mapa e manda reiniciar
    /// </summary>
    public void OpenFileBrowser()
    {
        string MAP_HEIGHTMAP_FILE = "";
        string MAP_SATELLITE_FILE = "";

        var bp = new BrowserProperties();
        bp.title = "Select Heightmap Image File";
        bp.filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";
        bp.filterIndex = 0;

        new FileBrowser().OpenFileBrowser(bp, path =>
        {
            Debug.Log(path);
            MAP_HEIGHTMAP_FILE = path;
        });

        bp.title = "Select Satellite Image File";

        new FileBrowser().OpenFileBrowser(bp, path =>
        {
            Debug.Log(path);
            MAP_SATELLITE_FILE = path;
        });

        if (MAP_HEIGHTMAP_FILE.Equals("") || MAP_SATELLITE_FILE.Equals(""))
        {
            Debug.Log("File vazio");
            return;
        }

        //Destroy current case
        DestroyAllMatchObjectsAndLines();

        //Reload Map
        mapGenerator.GenerateRealMap(MAP_HEIGHTMAP_FILE, MAP_SATELLITE_FILE);

        //RestartGame
        InitPlayers();
        Invoke(nameof(ComandStartCase), 0.5f);
        Invoke(nameof(SearchSimillarCases), 1f);
    }

    /// <summary>
    /// Função que istancia os prefabs dos controladores de cada time de agentes
    /// </summary>
    public void InitPlayers()
    {
        System.Random prng = new System.Random();
        int id = 1826; //prng.Next(0, 10000);
        Debug.Log("ID do Random do caso: "+id);

        if (inputNumberOfAgents.text == "" || int.Parse(inputNumberOfAgents.text) <= 0)
        {
            Debug.Log("Numero de agentes invalido");
            return;
        }

        GameObject prefab = Resources.Load<GameObject>("Prefabs/" + Constants.PLAYER_CONTROLLER_1);
        GameObject go = Instantiate(prefab);

        pcTeam1 = go.GetComponent<PlayerController>();
        pcTeam1.numberOfAgents = int.Parse(inputNumberOfAgents.text);
        pcTeam1.name = Constants.PLAYER_CONTROLLER_1;
        pcTeam1.StartAgents(id);

        prefab = Resources.Load<GameObject>("Prefabs/" + Constants.PLAYER_CONTROLLER_2);
        go = Instantiate(prefab);

        pcTeam2 = go.GetComponent<PlayerController>();
        //pcTeam2.numberOfAgents = int.Parse(inputNumberOfAgents.text);
        pcTeam2.name = Constants.PLAYER_CONTROLLER_2;
        //pcTeam2.StartAgents(id+1);

        ReorderAgents();
    }

    /// <summary>
    /// Função que reordena os agentes pela posição e os renomeia;
    /// </summary>
    private void ReorderAgents()
    {
        pcTeam1.Agents = CBDPUtils.OrderAgentList(pcTeam1.Agents);

        for (int i = 0; i < pcTeam1.numberOfAgents; i++)
        {
            pcTeam1.Agents[i].name = "Agent" + i.ToString();
            pcTeam1.Agents[i].SetNameText(i.ToString());
        }
            
        /*pcTeam2.Agents = CBDPUtils.OrderAgentList(pcTeam2.Agents);

        for (int i = 0; i < pcTeam2.Agents.Count; i++)
            pcTeam2.Agents[i].name = "Agent" + i.ToString();*/
    }

    /// <summary>
    /// Função que destrói os agentes do caso corrente, inicia os paineis de informação do caso e manda para o cbdp quem finalizou o plano
    /// </summary>
    public IEnumerator EndCase(string agent, string goal)
    {
        TakeAPicture("Solution");
        yield return new WaitForEndOfFrame();
        DestroyAllMatchObjectsAndLines();
        cbdp.UpdateWhoEndtheCase(agent, goal);
        canvasPainels.SetActive(false);
        canvasType.SetActive(true);
    }

    /// <summary>
    /// Função que destrói todos os game objects referentes ao caso no simulador e as linhas dos paths
    /// </summary>
    private void DestroyAllMatchObjectsAndLines()
    {
        //Destruir os agents
        foreach (var matchObject in FindObjectsOfType<MatchBehaviour>())
            Destroy(matchObject.gameObject);

        //Destruir as linhas dos paths desenhados
        foreach (var line in GameObject.FindGameObjectsWithTag(Constants.TAG_LINE_OF_PATH))
            Destroy(line);
    }

    public void TakeAPicture(string type)
    {
        //Tirar o print
        ScreenShot ss = GameObject.Find(Constants.CAMERA_SCREEN_SHOT).GetComponent<ScreenShot>();
        StartCoroutine(ss.DoScreenShot(id, type));
    }

    /// <summary>
    /// Função que inicia novamente com um novo caso e chama para atualizar seus casos similares
    /// </summary>
    public void RestartWithNewCase()
    {
        DestroyAllMatchObjectsAndLines();

        //RestartGame
        InitPlayers();
        Invoke(nameof(ComandStartCase), 0.5f);
        Invoke(nameof(SearchSimillarCases), 1f);
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
    /// Função chamada na alteração do slider para alterar o valor do campo do texto
    /// </summary>
    public void SetThresholdText()
    {
        thresholdSliderText.text = ">= " + sliderRetrivalCasesThreshold.value.ToString("0.00") + " %";
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
        cbdp.SetDescriptionInCase(inputDescription.text.Replace(Constants.SPLITTER, ' '));

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

        //normalizar o time das ações
        cbdp.NormalizeTimeInPlan();

        //save case
        cbdp.SaveCase(cbdp.GetCase());

        //RestartGame
        InitPlayers();
        Invoke(nameof(ComandStartCase), 0.5f);
        Invoke(nameof(SearchSimillarCases), 1f);
        canvasPainels.SetActive(true);
    }

    /// <summary>
    /// Função que executa recebe a escolha do plano escolhido e passa para a função que o executa
    /// </summary>
    public void ExecuteSimilarCase()
    {
        if (selectedSimilarCaseId == -1)
        {
            Debug.Log("Deve escolher um caso similar primeiro!");
            return;
        }

        ConstructPlanOfSimilarCase();
        Debug.Log("Executando o Plano: " + listOfSimilarCases[selectedSimilarCaseId][1]);
        ReceivePlan(listOfSimilarCases[selectedSimilarCaseId][1]);
    }

    public void ConstructPlanOfSimilarCase()
    {
        this.plan = new Plan(listOfSimilarCases[selectedSimilarCaseId][1]);

        Dictionary<Action, Message> dicActionsMessage = CreateMessagesOfPlanExecution(plan.actions);
    }

    private Dictionary<Action, Message> CreateMessagesOfPlanExecution(Queue<Action> actions)
    {
        Dictionary<Action, Message> dic = new Dictionary<Action, Message>();
        foreach (Action action in actions)
        {

        }
        return dic;
    }

    /// <summary>
    /// Função que 
    /// </summary>
    public void ExecutePreview()
    {
        if (selectedSimilarCaseId == -1)
        {
            Debug.Log("Deve escolher um caso similar primeiro!");
            return;
        }

        ConstructPlanOfSimilarCase();
        // mandar agentes printar
    }


    /// <summary>
    /// Função que recebe o id do plano escolhido como similar e o seta na variável do plano similar
    /// </summary>
    /// <param name="id"></param>
    public void SetIdOfSimilarCaseSelected(int id)
    {
        Debug.Log("Setou o id: " + id.ToString() + " como caso similar");
        selectedSimilarCaseId = id;
    }


    /// <summary>
    /// Função que seta valores iniciais em variáveis
    /// </summary>
    private void SetVariables()
    {
        gameObject.tag = clientOfExecution.GetPlayerControllerTag();
        selectedAgents = new List<AgentController>();
        repositionObjects = new List<SetNewPosition>();
        selectedSimilarCaseId = -1;
        clientOfExecution.SearchSimulationController();
        Invoke(nameof(ComandStartCase), 0.5f);
        EnableComponentSelectController();
        DesableComponentPathfinderPointsController();
        SetThresholdText();
    }

    /// <summary>
    /// Função que passa para o cbdp construir um caso a partir da lista dos agentes
    /// </summary>
    private void ComandStartCase()
    {
        Debug.Log("2.1");
        cbdp.ConstructInitCase(pcTeam1.Agents, pcTeam2.Agents, id);
        Debug.Log("2.2");
    }

    /// <summary>
    /// Função que busca componentes na cena e atribui a variáveis
    /// </summary>
    private void FindComponents()
    {
        clientOfExecution = GameObject.Find("Client").GetComponent<Client>();
        selectController = gameObject.GetComponent<SelectController>();
        pathfinderPointsController = gameObject.GetComponent<PathfinderPointsController>();
        canvasType = Camera.main.transform.Find("CanvasType").gameObject;
        canvasStrategy = Camera.main.transform.Find("CanvasStrategy").gameObject;
        canvasResult = Camera.main.transform.Find("CanvasResult").gameObject;
        canvasDescription = Camera.main.transform.Find("CanvasDescription").gameObject;
        canvasPainels = Camera.main.transform.Find("CanvasPainels").gameObject;
        canvasSelectPathfinder = canvasPainels.transform.Find("Left Panel").transform.Find("PathSelect").gameObject;
        dropdown = canvasSelectPathfinder.transform.Find("Text").GetComponentInChildren<Dropdown>();
        contentButtonsCase = canvasPainels.transform.Find("Right Panel").transform.Find("Scroll View").transform.Find("Viewport").transform.Find("Content").gameObject;
        inputDescription = canvasDescription.transform.Find("Panel").transform.Find("Description").GetComponentInChildren<InputField>();
        cbdp = GameObject.Find("CBDP").GetComponent<CBDP>();
        mapGenerator = GameObject.Find(Constants.MAP_GENERATOR).GetComponent<MapGenerator>();

        inputRetrivalCasesNumber = canvasPainels.transform.Find("Left Panel").transform.Find("ConfigsPanel").transform.Find("RetrivalCasesInput").GetComponentInChildren<InputField>();
        sliderRetrivalCasesThreshold = canvasPainels.transform.Find("Left Panel").transform.Find("ConfigsPanel").transform.Find("RetrivalCasesThreshold").GetComponentInChildren<Slider>();
        thresholdSliderText = canvasPainels.transform.Find("Left Panel").transform.Find("ConfigsPanel").transform.Find("RetrivalCasesThreshold").transform.Find("TextPercentage").GetComponent<Text>();
        inputNumberOfAgents = canvasPainels.transform.Find("Left Panel").transform.Find("ConfigsPanel").transform.Find("NumberOfAgents").GetComponentInChildren<InputField>();
    }

    /// <summary>
    /// Função que envia para o servidor e para o caso no CBDP as posições (enganosa e objetivo real) dos agentes selecionados
    /// </summary>
    /// <param name="objectivePosition">Vector3Int da posição do objetivo real</param>
    /// <param name="deceptivePosition">Vector3Int da posição do objetivo enganoso</param>
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


    private void ShowArrowPathConstructor()
    {
        Color color = pathType.Equals(PathType.NORMAL) ? Color.green : Color.red;

        foreach (AgentController agent in selectedAgents)
            Utils.InstantiateLineDrawer(agent.transform.position, color);
        EnableComponentPathfinderPointsController();
    }

    public bool VerifySelectedGroupAgent(Vector2[] corners)
    {
        var Agents = CompareTag(Constants.TAG_TEAM_1) ? pcTeam1.Agents : pcTeam2.Agents;

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
        canvasSelectPathfinder.SetActive(true);
    }

    public void DesableComponentPathfinderPointsController()
    {
        pathfinderPointsController.enabled = false;
    }

    public bool VerifySelectedSingleAgent(Vector3 point)
    {
        Ray ray = Camera.main.ScreenPointToRay(point);
        Debug.Log(Camera.main.ScreenToWorldPoint(point));

        if (Physics.Raycast(ray, out RaycastHit hit, 50000.0f))
            if (hit.transform.CompareTag(gameObject.tag))
                if (hit.transform.gameObject.TryGetComponent<AgentController>(out AgentController agent))
                {
                    selectedAgents.Add(agent);
                    return true;
                }
        return false;
    }


    public bool VerifySelectedSingleObject(Vector3 point)
    {
        Ray ray = Camera.main.ScreenPointToRay(point);

        if (Physics.Raycast(ray, out RaycastHit hit, 50000.0f))
            if (hit.transform.gameObject.TryGetComponent<SetNewPosition>(out SetNewPosition ob))
            {
                repositionObjects.Add(ob);
                ob.reposition = true;
                return true;
            }
        return false;
    }

    public void ClearSelectedObjects()
    {
        foreach (SetNewPosition ob in repositionObjects)
        {
            ob.reposition = false;
        }

        repositionObjects.Clear();
        //ConstructInitCase
    }

    public void SetPathfinder()
    {
        pathType = (PathType)dropdown.value;
        
        canvasSelectPathfinder = canvasPainels.transform.Find("Left Panel").transform.Find("PathSelect").gameObject;
        canvasSelectPathfinder.SetActive(false);
        ShowArrowPathConstructor();
        // ao inves de construir as linhas para setar os objetivos, utilizar os objetivos fixos
        // todo: posicionar os objetivos
        /*
        bool flag = selectedAgents[0].tag.Equals(Constants.TAG_TEAM_1);
        var deceptivePosition = flag ? pcTeam2.getDeceptiveGoalPosition() : pcTeam1.getDeceptiveGoalPosition();
        var realPosition = flag ?  pcTeam2.getRealGoalPosition() : pcTeam1.getRealGoalPosition();
        Debug.Log(deceptivePosition);
        Debug.Log(realPosition);
        ReceiveObjectivePositions(realPosition, deceptivePosition);
        */
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
        if (team.Equals(Constants.TAG_TEAM_1))
            pcTeam1.ReceiveMove(name, objectivePosition, deceptivePosition, pathType, mapGenerator.MAP_HEIGHTMAP_FILE);
        else
            pcTeam2.ReceiveMove(name, objectivePosition, deceptivePosition, pathType, mapGenerator.MAP_HEIGHTMAP_FILE);
    }

    private void ExecutePlan()
    {
        var action = plan.actions.Peek();

        if (action.time <= (DateTime.Now - dateStartPlan).TotalSeconds)
        {
            action = plan.actions.Dequeue();
            Debug.Log("Executando ação: " + action.ToString());
            ExecuteAction(action);

            if (plan.actions.Count == 0)
                hasPlan = false;
        }
    }

    private void ExecuteAction(Action action)
    {
        switch (action.action)
        {
            case "Move":
                {
                    Vector3Int objectivePosition, deceptivePosition = new Vector3Int();
                    Vector3 position = GetPositionByName(action.agent);
                    // Verificar se existe um objetivo
                    if (action.objetive.Equals(""))
                        //n�o existe, ent�o usar o distance_direction
                        objectivePosition = GetPositionByDistanceDirection(action.distance_direction, position);
                    else
                        //usar a localiza��o do objetivo
                        objectivePosition = GetPositionByName(action.objetive);

                    if (!action.pathType.Equals(PathType.NORMAL))
                        deceptivePosition = GetPositionByDistanceDirection(action.distance_directionDeceptive, position);

                    // Mandando movimenta��o para o servidor
                    Message send = new Message();
                    send.AddMessage("Moves", gameObject.tag, action.agent, action.pathType, objectivePosition, deceptivePosition);

                    AgentController agent = GameObject.Find(action.agent).GetComponent<AgentController>();
                    StartCoroutine(SendActionToCase("Move", agent, objectivePosition, deceptivePosition, action.pathType));
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
                maxDistance = (Constants.MAX_DISTANCE / 16) - 1;
                minDistance = 0;
                break;
            case "C":
                maxDistance = (Constants.MAX_DISTANCE / 8) - 1;
                minDistance = Constants.MAX_DISTANCE / 16;
                break;
            case "A":
                maxDistance = (Constants.MAX_DISTANCE / 4) - 1;
                minDistance = Constants.MAX_DISTANCE / 8;
                break;
            case "F":
                maxDistance = (Constants.MAX_DISTANCE / 2) - 1;
                minDistance = Constants.MAX_DISTANCE / 4;
                break;
            case "VF":
                maxDistance = Constants.MAX_DISTANCE;
                minDistance = Constants.MAX_DISTANCE / 2;
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
        var AStar = GameObject.Find(Constants.PATHFINDER).GetComponent<AStar>();
        var random = new System.Random();
        do
        {

            double h = random.Next(minDistance, maxDistance);
            double angle = random.Next(minDirection, maxDirection);

            position = Utils.PositionByDistanceAndAngle(angle, h, new Vector2(positionAgent.x, positionAgent.y));
            point = AStar.GetTileByPosition(Vector3Int.FloorToInt(new Vector3Int(position.x, position.y, 0)) / Constants.MAP_OFFSET);

        } while (point == null || !point.Walkable);

        return position / Constants.MAP_OFFSET;
    }

    private Vector3Int GetPositionByName(string objetive)
    {
        var gameObject = GameObject.Find(objetive);

        return Vector3Int.FloorToInt(new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, 0) / Constants.MAP_OFFSET);
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
            action = str_action,
            agent = agent.name,
            pathType = pathType,
            objetive = Utils.GetObjetive(Input.mousePosition),
            time = (int)Math.Round((DateTime.Now - cbdp.initTime).TotalSeconds)
        };

        if (pathType.Equals(PathType.NORMAL))
            action.actionDefinition = DeceptiveLevel.NOT_DECEPTIVE;
        else if (pathType.Equals(PathType.DECEPTIVE_1))
            action.actionDefinition = DeceptiveLevel.HIGHLY_DECEPTIVE;
        else if (pathType.Equals(PathType.DECEPTIVE_2) || pathType.Equals(PathType.DECEPTIVE_3))
            action.actionDefinition = DeceptiveLevel.PARTIALLY_DECEPTIVE;
        else
            action.actionDefinition = DeceptiveLevel.LITTLE_DECEPTIVE;

        //distance_direction
        Distance distance = CBDPUtils.CalculeDistance(agent.transform.position, objetivePosition);
        Direction direction = CBDPUtils.CalculeDirection(objetivePosition.x, objetivePosition.y);

        action.distance_direction = distance.ToString() + '-' + direction.ToString();

        //distance_direction Deceptive objetive
        distance = CBDPUtils.CalculeDistance(agent.transform.position, deceptivePosition);
        direction = CBDPUtils.CalculeDirection(deceptivePosition.x, deceptivePosition.y);

        action.distance_directionDeceptive = distance.ToString() + '-' + direction.ToString();

        if(pathType.Equals(PathType.NORMAL))
            action.cost = Vector3.Distance(agent.transform.position, objetivePosition);
        else
            action.cost = Vector3.Distance(agent.transform.position, deceptivePosition) + Vector3.Distance(deceptivePosition, objetivePosition);

        //send to plan
        cbdp.PlanAddAction(action);
        yield return null;
    }

    /// <summary>
    /// Função que realiza a busca no CBDP pelos casos similares ao caso atual e preenche o content com eles
    /// </summary>
    public void SearchSimillarCases()
    {
        Debug.Log("SearchSimillarCases 0");
        float threshold = sliderRetrivalCasesThreshold.value * 100; 
        int.TryParse(inputRetrivalCasesNumber.text, out int maxRetrivelCases);

        if (inputRetrivalCasesNumber.text.Equals("") || maxRetrivelCases <= 0)
        {
            Debug.Log("O número máximo de casos similares deve ser um número válido!");
            return;
        }
        Debug.Log("SearchSimillarCases 1");

        //Lista de [Descrição, Plano, Percentage, ID]
        listOfSimilarCases = cbdp.SearchSimilarCases();

        Debug.Log("SearchSimillarCases 2");

        //Limpar lista de botões de casos similares no content 
        ClearButtonCaseInContent();

        //Debug.Log("maxRetrivelCases = " + maxRetrivelCases.ToString());
        //Debug.Log("threshold = " + threshold.ToString());

        if (listOfSimilarCases.Count == 0)
        {
            Debug.Log("Nenhum caso similar foi encontrado!");
            return;
        }

        float percentageValue = float.Parse(listOfSimilarCases[0][2]);
        //Debug.Log("percentageValue = " + percentageValue.ToString());
        //Debug.Log("bool = " + (percentageValue >= threshold));
        if (percentageValue < threshold)
        {
            Debug.Log("Nenhum caso acima do threshold "+ threshold.ToString() + " foi encontrado!");
            Debug.Log("Caso mais similar obteve: " + percentageValue.ToString() + " % de similaridade");
            return;
        }

        //prefab do botão para listar no content
        GameObject prefab = Resources.Load<GameObject>("Prefabs/ButtonCase");
        // criar e adicionar cada botão referente aos casos similares
        for (int i = 0; i < listOfSimilarCases.Count && i < maxRetrivelCases && percentageValue >= threshold; i++)
        {
            GameObject go = Instantiate(prefab);
            go.transform.SetParent(contentButtonsCase.transform);
            go.transform.localScale = Vector3.one;
            go.transform.position = contentButtonsCase.transform.position;

            Text text = go.transform.Find("Text").GetComponent<Text>();
            Text percentage = go.transform.Find("Percentage").GetComponent<Text>();
            Image image = go.transform.Find("Image").GetComponent<Image>();
            Button button = go.GetComponent<Button>();
            SetButtonOnClickAnswer(button, i);
            text.text = "Description: " + listOfSimilarCases[i][0];
            
            percentageValue = float.Parse(listOfSimilarCases[i][2]);
            percentage.text = "Case " + i.ToString() + ": " + percentageValue.ToString() + " %";

            Sprite aSprite = Resources.Load<Sprite>("Solution_Case_" + listOfSimilarCases[i][3]);
            image.sprite = aSprite;

            Debug.Log("Similaridade Global com Caso "+ listOfSimilarCases[i][3] + " é de " + percentageValue.ToString() + " %");
        }
        Debug.Log("SearchSimillarCases 3");
    }

    /// <summary>
    /// Função que limpa o content que contém os botões de casos similares no canvas
    /// </summary>
    private void ClearButtonCaseInContent()
    {
        foreach (Transform button in contentButtonsCase.GetComponentsInChildren<Transform>())
            if (button.CompareTag(Constants.TAG_BUTTON_CASE))
                Destroy(button.gameObject);

    }

    /// <summary>
    /// Função que seta um action em cada botão adicionado no content de casos similares
    /// </summary>
    /// <param name="button">Botão de caso similar</param>
    /// <param name="value">Index correspondente na lista de casos similares</param>
    private void SetButtonOnClickAnswer(Button button, int value)
    {
        button.onClick.AddListener(() => SetIdOfSimilarCaseSelected(value));
    }
}
