using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public List<AgentController> Agents;
    public List<AgentController> EnemyAgents;
    public List<AgentController> ClickedAgents;
    public string clientCanBeController;
    public int Points;
    public int numberOfAgents = 5;
    public Text PointsPainel;
    private Client clientOfExecution;
    private string clientName;
    CaseConstructor caseConstructor;
    System.Random prng;

    public GameObject canvasSelectPathfinder;
    public Dropdown dropdown;
    public RectTransform rectPanel;
    public RectTransform rectCanvas;
    public Camera cam;

    public DrawArrowLine line;
    private bool isDrawLine;
    private int indexTypePath;

    bool hasPlan = false;
    private DateTime dateStartPlan;
    CaseConstructor.Plan plan;

    bool dragSelect;

    Vector3Int deceptivePosition;
    bool hasDeceptivePosition;


    Vector3 p1;
    Vector3 p2;
    //the corners of our 2d selection box
    Vector2[] corners;

    // Start is called before the first frame update
    void Start()
    {
        ClickedAgents = new List<AgentController>();
        Agents = new List<AgentController>();
        Points = 0;
        PointsPainel = GameObject.Find(gameObject.tag+" Points").GetComponent<Text>();
        PointsPainel.text = gameObject.tag + " Points: " + Points;
        clientOfExecution = GameObject.Find("Client").GetComponent<Client>();
        clientName = clientOfExecution.getClientName();

        caseConstructor = GameObject.Find("CaseConstructor").GetComponent<CaseConstructor>();
        prng = new System.Random(clientOfExecution.seed);
        isDrawLine = false;
        hasDeceptivePosition = false;
        indexTypePath = 0;
        StartAgents();
    }

    public void StartAgents()
    {
        Agents.Clear();
        ClickedAgents.Clear();

        GameObject prefab;
        GameObject go;
        string name;

        if (this.CompareTag("Team1"))
        {
            prefab = Resources.Load("Prefabs/Agent") as GameObject;
            name = "Agent";
        }
        else
        {
            prefab = Resources.Load("Prefabs/AgentEnimy") as GameObject;
            name = "AgentEnimy";
        }

        int i = 1;
        //prng = new System.Random(clientOfExecution.seed);
        do
        {
            go = Instantiate(prefab);
            AgentController agent = go.GetComponent<AgentController>();
            agent.name = name + i.ToString();
            agent.transform.parent = this.transform;
            agent.InitPosition(prng.Next());

            //Debug.Log("criando o " + agent.name);
            this.Agents.Add(agent);
            i++;
        } while (i <= numberOfAgents);
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!isDrawLine)
        {
            // A ia controla, se tem um plano executa
            if (clientName.Equals("IA"))
            {
                if (hasPlan)
                {
                    ExecutePlan();
                }
            }
            else if (clientCanBeController.Equals(clientName))
            {
                //1. Quando pressiona o botão, pega o ponto
                if (Input.GetMouseButtonDown(0))
                    p1 = Input.mousePosition;

                //2. Se continuou pressionando e moveu um ponto, pode criar a box
                if (Input.GetMouseButton(0))
                    if ((p1 - Input.mousePosition).magnitude > 40)
                        dragSelect = true;

                //3. Quando soltar o botão...
                if (Input.GetMouseButtonUp(0))
                {
                    if (dragSelect == false) //Não moveu o mouse
                    {
                        if (IsClickedinAgent(p1)) //Clicou em um agente. Perguntar Path
                            ShowCanvasPath(p1);
                    }
                    else // Selecionou pela box
                    {
                        p2 = Input.mousePosition;
                        corners = getBoundingBox(p1, p2);

                        //Quais agentes estão dentro dos limites da box
                        foreach (AgentController agent in Agents)
                        {
                            Vector3 positon = agent.transform.position;
                            if (VerifyPointInLimits(positon, corners))
                            {
                                // O agente esta dentro da box, adicionar na lista
                                if (!ClickedAgents.Contains(agent))
                                {
                                    ClickedAgents.Add(agent);
                                }
                            }
                        }

                        if (ClickedAgents.Count >= 1)
                        {
                            // Tem agentes, perguntar o path
                            ShowCanvasPath(p2);
                        }

                    }//end marquee select
                    dragSelect = false;
                }
            }
        } 
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (indexTypePath == 0)
                {
                    DestroyLineDrawer();
                    Vector3Int positionClick = Vector3Int.FloorToInt(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                    
                    string send = "";
                    foreach (AgentController agent in ClickedAgents)
                    {
                        agent.BuildPath(new Vector3Int(positionClick.x, positionClick.y, 0) / 10, Vector3Int.forward);
                    
                        send += ("Moves|" + gameObject.tag + "|" + agent.name + "|" + positionClick.x / 10 + "|" + positionClick.y / 10 + "#");
                        StartCoroutine(SendActionToCase("Move", agent, positionClick));
                    }
                    clientOfExecution.Send(send);

                    ClickedAgents.Clear();
                    isDrawLine = false;
                }
                else
                {
                    DestroyLineDrawer();
                    if (!hasDeceptivePosition)
                    {
                        deceptivePosition = Vector3Int.FloorToInt(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                        InstantiateLineDrawer(deceptivePosition, Color.green);
                        hasDeceptivePosition = true;
                    }
                    else
                    {
                        Vector3Int positionClick = Vector3Int.FloorToInt(Camera.main.ScreenToWorldPoint(Input.mousePosition));

                        string send = "";
                        foreach (AgentController agent in ClickedAgents)
                        {
                            agent.BuildPath(new Vector3Int(positionClick.x, positionClick.y, 0) / 10, Vector3Int.forward);

                            send += ("Moves|" + gameObject.tag + "|" + agent.name + "|" + positionClick.x / 10 + "|" + positionClick.y / 10 + "#");
                            StartCoroutine(SendActionToCase("Move", agent, positionClick));
                        }
                        clientOfExecution.Send(send);

                        ClickedAgents.Clear();
                        isDrawLine = false;
                        hasDeceptivePosition = false;
                    }
                }
            }
        }
    }

    private bool VerifyPointInLimits(Vector3 positon, Vector2[] corners)
    {
        if (positon.x <= corners[3].x && positon.x >= corners[0].x
            && positon.y >= corners[3].y && positon.y <= corners[0].y)
            return true;
        return false;
    }

    //create a bounding box (4 corners in order) from the start and end mouse position
    public Vector2[] getBoundingBox(Vector2 p1, Vector2 p2)
    {
        // Min and Max to get 2 corners of rectangle regardless of drag direction.
        var bottomLeft = Vector3.Min(p1, p2);
        var topRight = Vector3.Max(p1, p2);

        // 0 = top left; 1 = top right; 2 = bottom left; 3 = bottom right;
        Vector2[] corners =
        {
            new Vector2(bottomLeft.x, topRight.y),
            new Vector2(topRight.x, topRight.y),
            new Vector2(bottomLeft.x, bottomLeft.y),
            new Vector2(topRight.x, bottomLeft.y)
        };
        return corners;
    }

    private void ShowCanvasPath(Vector3 point)
    {
        // Mostrar Canvas do path 
        canvasSelectPathfinder.SetActive(true);
        Vector2 anchoredPos;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectCanvas, Input.mousePosition, cam, out anchoredPos);

        anchoredPos = new Vector2(anchoredPos.x + 80, anchoredPos.y - 30);
        rectPanel.anchoredPosition = anchoredPos;
    }

    public void SetPathfinder()
    {
        string pathSelected = dropdown.options[dropdown.value].text;
        Debug.Log("Path selected: " + pathSelected);

        indexTypePath = dropdown.value;

        foreach (AgentController agent in ClickedAgents)
        {
            agent.indexTypePath = indexTypePath;
        }

        canvasSelectPathfinder.SetActive(false);
        ShowArrowPathConstructor();
        dropdown.value = 0;
    }

    private void ShowArrowPathConstructor()
    {
        PathType path = (PathType)indexTypePath;

        //Se o path é normal, precisamos de somente uma seta
        if (path.Equals(PathType.NORMAL))
        {
            foreach (AgentController agent in ClickedAgents)
            {
                InstantiateLineDrawer(agent.transform.position, Color.green);
            }
        }
        else
        {
            foreach (AgentController agent in ClickedAgents)
            {
                InstantiateLineDrawer(agent.transform.position, Color.red);
            }
            // Qualquer outro, preciso de um passo a mais, o passo enganoso
        }
    }

    private void InstantiateLineDrawer(Vector3 position, Color color)
    {
        var drawLine = Instantiate(line);
        drawLine.initialPosition = position;
        drawLine.canDraw = true;
        drawLine.LineRenderer.startColor = color;
        drawLine.LineRenderer.endColor = color;
    }

    private void DestroyLineDrawer()
    {
        var lines = FindObjectsOfType<DrawArrowLine>();

        foreach (var line in lines)
            Destroy(line.gameObject);
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

    private void ExecuteAction(CaseConstructor.Plan.Action action)
    {
        switch (action.action)
        {
            case "Move":
                {
                    Vector3Int objetivePosition;

                    // Verificar se existe um objetivo
                    if (action.objetive.Equals(""))
                    {
                        Vector3 position = new Vector3();
                        foreach (AgentController agent in Agents)
                            if (agent.name == action.agent)
                            {
                                position = agent.transform.position;
                                break;
                            }
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

                    // Mandando movimentação
                    //ReceiveMove(action.agent, objetivePosition.x, objetivePosition.y);
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
        var AStar = GameObject.Find("A*").GetComponent<AStar>();
        var random = new System.Random();
        do
        {

            double h = random.Next(minDistance, maxDistance);
            double angle = random.Next(minDirection, maxDirection);

            position = PositionByDistanceAndAngle(angle, h, new Vector2(positionAgent.x, positionAgent.y));
            point = AStar.GetTileByPosition(Vector3Int.FloorToInt(new Vector3Int(position.x, position.y, 0)) / 10);

        } while (point == null || !point.Walkable);

        return position / 10;
    }

    private Vector3Int PositionByDistanceAndAngle(double angle, double distance, Vector2 point)
    {
        // Co (x) = sen * D
        double x = Math.Sin(angle) * distance;
        // Ca (y) = cos * D
        double y = Math.Cos(angle) * distance;

        //offset em relação ao ponto
        x += point.x;
        y += point.y;

        return new Vector3Int((int)x, (int)y, 0);
    }

    private Vector3Int GetPositionByName(string objetive)
    {
        var gameObject = GameObject.Find(objetive);

        return Vector3Int.FloorToInt(new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, 0)/10);
    }

    public void ReceivePlan(string plan)
    {
        this.plan = new CaseConstructor.Plan(plan);
        dateStartPlan = DateTime.Now;
        hasPlan = true;
    }

    private IEnumerator SendActionToCase(string str_action, AgentController agent, Vector3Int positionClick)
    {
        CaseConstructor.Plan.Action action = new CaseConstructor.Plan.Action();

        //action
        action.action = str_action;

        //agent
        action.agent = agent.name;

        // objetive
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, 10000.0f))
        {
            if (hit.transform != null)
            {
                if (hit.transform.CompareTag("Team1") || hit.transform.CompareTag("Team2"))
                {
                    //tem um gameobject, mandar no action
                    action.objetive = hit.transform.gameObject.name;

                }
                else
                    action.objetive = "";
            }
        }

        //actionDefinition
        action.actionDefinition = CaseType.NORMAL;

        //distance_direction
        Distance distance = caseConstructor.CalculeDistance(agent.transform.position, positionClick);
        Direction direction = caseConstructor.CalculeDirection(positionClick.x, positionClick.y);

        action.distance_direction = distance.ToString() + '-' + direction.ToString();

        //time
        action.time = (int) Math.Round((DateTime.Now - caseConstructor.initTime).TotalSeconds);

        //send to plan
        caseConstructor.PlanAddAction(action);

        yield return null;
    }

    public void ReceiveMove(string name, int x, int y)
    {
        foreach (AgentController agent in Agents)
            if (agent.name == name)
            {
                agent.BuildPath(new Vector3Int(x, y, 0), Vector3Int.zero);
                return;
            }
            
    }

    private bool IsClickedinAgent(Vector3 point)
    {
        Ray ray = Camera.main.ScreenPointToRay(point);

        if (Physics.Raycast(ray, out RaycastHit hit, 50000.0f))
        {
             foreach (AgentController agent in Agents)
             {
                 if (agent && hit.transform == agent.transform)
                 {   
                     if (!ClickedAgents.Contains(agent))
                     {
                         ClickedAgents.Add(agent);
                         return true;
                     }
                     break;
                 }
                         
             }
        }
        return false;
    }

    private void OnGUI()
    {
        if (dragSelect == true)
        {
            var rect = Utils.GetScreenRect(p1, Input.mousePosition);
            Utils.DrawScreenRect(rect, new Color(0.8f, 0.8f, 0.95f, 0.25f));
            Utils.DrawScreenRectBorder(rect, 2, new Color(0.8f, 0.8f, 0.95f));
        }
    }

}
