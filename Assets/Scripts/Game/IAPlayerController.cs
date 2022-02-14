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
                    break;
            }
            default:
                break;
        }
    }

    private Vector3Int GetPositionByDistanceDirection(string objetive)
    {
        throw new NotImplementedException();
    }

    private Vector3Int GetPositionByName(string objetive)
    {
        throw new NotImplementedException();
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
