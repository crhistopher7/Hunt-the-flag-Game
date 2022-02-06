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
        var matchObjects = FindObjectsOfType<MatchBehaviour>();
        foreach (var matchObject in matchObjects)
        {
            if(matchObject.CompareTag(this.tag))
                Destroy(matchObject.gameObject);
        }

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
        if (Input.GetMouseButtonDown(0) && clientController.Equals(clientOfExecution.getClientName()))
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
                Rigidbody rigidbody;
                if (rigidbody = hit.transform.GetComponent<Rigidbody>())
                {
                    //tem um gameobject, mandar no action
                    action.objetive = rigidbody.transform.gameObject.name;

                }
                else
                    action.objetive = "";
            }
        }

        //actionDefinition
        action.actionDefinition = Type.NORMAL;

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
