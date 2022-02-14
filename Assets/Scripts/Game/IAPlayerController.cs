using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IAPlayerController : MonoBehaviour
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

            Debug.Log("criando o " + agent.name);
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
                        //não existe, então usar o distance_direction
                        objetivePosition = GetPositionByDistanceDirection(action.objetive);
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

    private Vector3Int GetPositionByDistanceDirection(string objetive)
    {
        var distance = objetive.Split('-')[0];
        var direction = objetive.Split('-')[1];

        int maxDistance;
        int minDistance;

        int maxDirection;
        int minDirection;

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
                maxDirection = 0;
                minDirection = 0;
                break;
            case "RF":
                maxDirection = 0;
                minDirection = 0;
                break;
            case "LF":
                maxDirection = 0;
                minDirection = 0;
                break;
            case "R":
                maxDirection = 0;
                minDirection = 0;
                break;
            case "L":
                maxDirection = 0;
                minDirection = 0;
                break;
            case "B":
                maxDirection = 0;
                minDirection = 0;
                break;
            case "RB":
                maxDirection = 0;
                minDirection = 0;
                break;
            case "LB":
                maxDirection = 0;
                minDirection = 0;
                break;
            default:
                break;

        }

        Vector3Int position = PositionByDistanceAndAngle(0, 0, new Vector2Int(0, 0));

       

        throw new NotImplementedException();
    }

    private Vector3Int PositionByDistanceAndAngle(int v1, int v2, Vector2Int vector2Int)
    {
        double rad = 64.10;
        rad *= Math.PI / 180;

        double angDist = 0.343 / 6371;

        Console.WriteLine("Angular distance:" + angDist);

        double latitude = 3.170620;
        double longitude = 103.151279;

        latitude *= Math.PI / 180;
        longitude *= Math.PI / 180;

        double lat2 = Math.Asin(Math.Sin(latitude) * Math.Cos(angDist) + Math.Cos(latitude) * Math.Sin(angDist) * Math.Cos(rad));

        double forAtana = Math.Sin(rad) * Math.Sin(angDist) * Math.Cos(latitude);
        double forAtanb = Math.Cos(angDist) - Math.Sin(latitude) * Math.Sin(lat2);

        double lon2 = longitude + Math.Atan2(forAtana, forAtanb);


        //double finalLat = latitude + lat2;
        //double finalLon = longitude + lon2;

        lat2 *= 180 / Math.PI;
        lon2 *= 180 / Math.PI;

        return new Vector3Int((int)lat2, (int)lon2, 0);
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

        action.distance_direction = distance.ToString() + ',' + direction.ToString();

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
}
