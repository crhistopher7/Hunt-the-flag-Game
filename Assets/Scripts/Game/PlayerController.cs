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
    public string clientController;
    public int Points;
    public int numberOfAgents = 5;
    public Text PointsPainel;
    private Client clientOfExecution;
    CaseConstructor caseConstructor;
    System.Random prng;

    bool hasPlan = false;
    private DateTime dateStartPlan;
    CaseConstructor.Plan plan;

    // Start is called before the first frame update
    void Start()
    {
        ClickedAgents = new List<AgentController>();
        Agents = new List<AgentController>();
        Points = 0;
        PointsPainel = GameObject.Find(gameObject.tag+" Points").GetComponent<Text>();
        PointsPainel.text = gameObject.tag + " Points: " + Points;
        clientOfExecution = GameObject.Find("Client").GetComponent<Client>();
        caseConstructor = GameObject.Find("CaseConstructor").GetComponent<CaseConstructor>();
        prng = new System.Random(clientOfExecution.seed);
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
        // A ia controla, se tem um plano executa
        if (hasPlan)
        {
            var action = plan.actions.Peek();

            if (action.time <= (dateStartPlan - DateTime.Now).TotalSeconds)
            {
                // Pode executar a próxima ação do plano
                action = plan.actions.Dequeue();

                ExecuteAction(action);
            }

        }
        else if (Input.GetMouseButtonDown(0) && clientController.Equals(clientOfExecution.getClientName()))
        {
            if (isClickedinAgent())
            {
                // caso precise fazer algo quando clico em um agente
            }
            else
            {
                // existe um agente e não cliquei em outro, vou fazer eles irem até o clique
                if (ClickedAgents.Count > 0)
                {
                    Vector3Int positionClick = Vector3Int.FloorToInt(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                    //Debug.Log(positionClick / 10);

                    string send = "";
                    foreach (AgentController agent in ClickedAgents)
                    {
                        agent.BuildPath(new Vector3Int(positionClick.x, positionClick.y, 0) / 10);

                        send += ("Moves|" + gameObject.tag + "|" + agent.name + "|" + positionClick.x / 10 + "|" + positionClick.y / 10 + "#");
                        StartCoroutine(SendActionToCase("Move", agent, positionClick));
                        //SendWithTime("Move|" + gameObject.tag + "|" + agent.name + "|" + positionClick.x / 10 + "|" + positionClick.y / 10);
                    }
                    clientOfExecution.Send(send);
                }
            }
        }
        else if (Input.GetMouseButtonDown(1) && clientController.Equals(clientOfExecution.getClientName()))
        {
            ClickedAgents.Clear();
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
                        //usar a localização do objetivo
                        objetivePosition = GetPositionByName(action.objetive);
                    }

                    // TODO Verificar o actionDefinition para ação enganosa

                    // Mandando movimentação
                    ReceiveMove(action.agent, objetivePosition.x, objetivePosition.y);
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
        Debug.Log("Objetivo: " + objetive);

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
            Debug.Log("Point.Walk:" + point);
        } while (point == null || !point.Walkable);

        return position;
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

        return Vector3Int.FloorToInt(gameObject.transform.position);
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
        action.actionDefinition = TypeCase.NORMAL;

        //distance_direction
        Distance distance = caseConstructor.CalculeDistance(agent.transform.position, positionClick);
        Direction direction = caseConstructor.CalculeDirection(positionClick.x, positionClick.y);

        action.distance_direction = distance.ToString() + '-' + direction.ToString();

        //time
        action.time = (caseConstructor.initTime - DateTime.Now).TotalSeconds;

        //send to plan
        caseConstructor.PlanAddAction(action);

        yield return null;
    }

    private IEnumerator SendWithTime(string send)
    {
        // doing something
        clientOfExecution.Send(send);
        // waits 5 seconds
        yield return new WaitForSeconds(0.5f);
    }

    public void ReceiveMove(string name, int x, int y)
    {
        foreach (AgentController agent in Agents)
            if (agent.name == name)
            {
                agent.BuildPath(new Vector3Int(x, y, 0));
                return;
            }
            
    }

    private bool isClickedinAgent()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, 10000.0f))
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
}
