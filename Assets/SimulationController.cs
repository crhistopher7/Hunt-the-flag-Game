using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimulationController : MonoBehaviour
{
    private Client clientOfExecution;
    private SelectController selectController;
    private PathfinderPointsController pathfinderPointsController;
    private GameObject canvasSelectPathfinder;
    private List<AgentController> selectedAgents;
    private Dropdown dropdown;
    private RectTransform rectPanel;
    private RectTransform rectCanvas;
    private PathType pathType;
    private CBDP cbdp;
    private PlayerController pcTeam1;
    private PlayerController pcTeam2;

    private bool hasPlan = false;
    private DateTime dateStartPlan;
    private Plan plan;

    public Vector3 pointOfCanvasPath;

    // Start is called before the first frame update
    void Start()
    {
        InitPlayers("PlayerController", "Player2Controller");
        FindComponents();
        SetVariables();
    }

    private void Update()
    {
        if (hasPlan)
            ExecutePlan();
    }

    public void InitPlayers(string a, string b)
    {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/" + a);
        GameObject go = Instantiate(prefab);

        pcTeam1 = go.GetComponent<PlayerController>();
        pcTeam1.name = a;

        prefab = Resources.Load<GameObject>("Prefabs/" + b);
        go = Instantiate(prefab);

        pcTeam2 = go.GetComponent<PlayerController>();
        pcTeam2.name = b;
    }

    private void SetVariables()
    {
        gameObject.tag = clientOfExecution.GetPlayerControllerTag();
        selectedAgents = new List<AgentController>();
        clientOfExecution.SearchSimulationController();

        EnableComponentSelectController();
        DesableComponentPathfinderPointsController();
    }

    private void FindComponents()
    {
        clientOfExecution = GameObject.Find("Client").GetComponent<Client>();
        selectController = gameObject.GetComponent<SelectController>();
        pathfinderPointsController = gameObject.GetComponent<PathfinderPointsController>();
        canvasSelectPathfinder = Camera.main.transform.Find("CanvasSelectAStar").gameObject;
        dropdown = canvasSelectPathfinder.transform.Find("Canvas").transform.Find("Panel").transform.Find("Text").GetComponentInChildren<Dropdown>();
        rectPanel = canvasSelectPathfinder.transform.Find("Canvas").transform.Find("Panel").GetComponent<RectTransform>();
        rectCanvas = canvasSelectPathfinder.GetComponent<RectTransform>();
        cbdp = GameObject.Find("CaseConstructor").GetComponent<CBDP>();
    }

    private void SendObjectivesToAgents(Vector3Int objectivePosition, Vector3Int deceptivePosition)
    {
        Message send = new Message();
        foreach (AgentController agent in selectedAgents)
        {
            send.AddMessage("Moves", gameObject.tag, agent, pathType, objectivePosition, deceptivePosition);
            StartCoroutine(SendActionToCase("Move", agent, objectivePosition, deceptivePosition, pathType));
        }
        clientOfExecution.Send(send.ToString());
        selectedAgents.Clear();
    }

    private void RestartGame()
    {
        pcTeam1.enabled = true;
        pcTeam2.enabled = true;
        //reiniciar players
        Debug.Log("start agents team 1");
        pcTeam1.StartAgents();
        Debug.Log("start agents team 2");
        pcTeam2.StartAgents();
    }

    private void ShowCanvasPath()
    {
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
        var Agents = CompareTag("Team1") ? pcTeam1.Agents : pcTeam2.Agents;

        //Quais agentes estão dentro dos limites da box
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
        Debug.Log("Path selected: " + pathType);
        
        canvasSelectPathfinder = Camera.main.transform.Find("CanvasSelectAStar").gameObject;
        canvasSelectPathfinder.SetActive(false);
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
        if (team == "Team1")
            pcTeam1.ReceiveMove(name, objectivePosition, deceptivePosition, pathType);
        else
            pcTeam2.ReceiveMove(name, objectivePosition, deceptivePosition, pathType);
    }

    private void ExecutePlan()
    {
        var action = plan.actions.Peek();

        if (action.time <= (DateTime.Now - dateStartPlan).TotalSeconds)
        {
            // Pode executar a próxima ação do plano
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
                    Vector3Int objetivePosition;

                    // Verificar se existe um objetivo
                    if (action.objetive.Equals(""))
                    {
                        Vector3 position = GetPositionByName(action.agent);
                        //não existe, então usar o distance_direction
                        objetivePosition = GetPositionByDistanceDirection(action.distance_direction, position);
                    }
                    else
                    {
                        Debug.Log("Executando a ação que possui objetivo");
                        //usar a localização do objetivo
                        objetivePosition = GetPositionByName(action.objetive);
                    }

                    // TODO Verificar o actionDefinition para ação enganosa
                    // Mandando movimentação para o servidor
                    clientOfExecution.Send("Moves|" + gameObject.tag + "|" + action.agent + "|" + objetivePosition.x + "|" + objetivePosition.y + "#");
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

        // Pegar valores aleatórios entre os intervalos determinados
        // repetir até que a posição seja possível de atingir
        Vector3Int position;
        LogicMap point;
        var AStar = GameObject.Find("Pathfinder").GetComponent<AStar>();
        var random = new System.Random();
        do
        {

            double h = random.Next(minDistance, maxDistance);
            double angle = random.Next(minDirection, maxDirection);

            position = Utils.PositionByDistanceAndAngle(angle, h, new Vector2(positionAgent.x, positionAgent.y));
            point = AStar.GetTileByPosition(Vector3Int.FloorToInt(new Vector3Int(position.x, position.y, 0)) / 10);

        } while (point == null || !point.Walkable);

        return position / 10;
    }

    private Vector3Int GetPositionByName(string objetive)
    {
        var gameObject = GameObject.Find(objetive);

        return Vector3Int.FloorToInt(new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, 0) / 10);
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

        action.distance_direction = distance.ToString() + '-' + direction.ToString();

        //send to plan
        cbdp.PlanAddAction(action);

        yield return null;
    }
}
