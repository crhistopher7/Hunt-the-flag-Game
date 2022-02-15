using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentController : MatchBehaviour
{
    public AStar AStar;
    public int maxX;
    public int minX;
    public int maxY;
    public int minY;
    protected Vector3 CurrentPosition;
    private float DistanceToChangeWayPoint = 0.5f;
    List<LogicMap> path;
    Rigidbody rb;
    int indexPath;
    protected bool followingpath;
    Sensor sensor;
    AgentLevel level;
    public bool isCarryingFlag;
    FlagController flagCarrying;
    public Transform lifeBar;

    public Material[] materials;
    Renderer rend;
    public int seed;
    System.Random prng;
    // Start is called before the first frame update
    void Start()
    {
        sensor = GetComponent<Sensor>();
        rend = GetComponent<Renderer>();
        rend.enabled = true;
        level = new AgentLevel(1);
        rb = GetComponent<Rigidbody>();
        followingpath = false;
        isCarryingFlag = false;
        lifeBar.localScale = new Vector3(level.life, 1, 1);

        AStar = GameObject.Find("A*").GetComponent<AStar>();
    }

    public void InitPosition(int seed)
    {
        prng = new System.Random(seed);
        LogicMap point;
        Vector3Int position;
        do
        {

            int x = prng.Next(minX, maxX);
            int y = prng.Next(minY, maxY);
            int z = (int)this.transform.position.z;

            position = new Vector3Int(x, y, z);

            point = AStar.GetTileByPosition(Vector3Int.FloorToInt(new Vector3Int(x, y, 0)) / 10);

        } while (!point.Walkable);

        this.transform.position = position;
        //Debug.Log("Setou posição do "+this.name);
    }


    // Update is called once per frame
    void Update()
    {
        //verificar se esse agente morreu
        if(level.life <= 0)
        {
            Die();
        }

        //verificar se encontrou agentes ou bandeira ao seu redor 
        List<RaycastHit> listOfHit = sensor.Check();
        CheckHits(listOfHit);

        // Se estiver seguindo um caminho se movimentar
        if (followingpath)
        {
            Move();
            CheckWayPoint();
            if (isCarryingFlag)
                flagCarrying.Agentposition = rb.position;
        }

    }

    private void Die()
    {
        // verificar se estava carregando uma bandeira
        if (isCarryingFlag)
        {
            // dizer q a bandeira não esta mais sendo carregada
            flagCarrying.beingCarried = false;
        }
        //retirar da lista de agentes do controller a referencia desse agente
        PlayerController pc;
        switch (transform.tag)
        {
            case "Team1":
                pc = GameObject.Find("PlayerController").GetComponent<PlayerController>();
                pc.Agents.Remove(this);
                pc.ClickedAgents.Remove(this);
                break;

            case "Team2":
                pc = GameObject.Find("Player2Controller").GetComponent<PlayerController>();
                pc.Agents.Remove(this);
                pc.ClickedAgents.Remove(this);
                break;

            default:
                break;
        }
        //morreu
        Destroy(this.gameObject);
    }

    private void CheckHits(List<RaycastHit> listOfHit)
    {
        //verificar se é vazio a lista
        if (listOfHit.Count != 0)
        {
            // contém algum agente ou bandeira na lista 
            foreach (RaycastHit hit in listOfHit)
            {
                //verificar se é uma bandeira
                if(hit.transform.CompareTag("Flag"))
                {
                    // é uma bandeira, se for do inimigo e não estou carregando nada, devo carregar 
                    FlagController flagController = hit.transform.GetComponent<FlagController>();
                    if (!transform.CompareTag(flagController.team) && !isCarryingFlag && !flagController.beingCarried)
                    {
                        //por enquanto o caso fecha aqui
                        CaseConstructor caseConstructor = GameObject.Find("CaseConstructor").GetComponent<CaseConstructor>();
                        caseConstructor.ConstructEndCase();
                        

                       /* flagController.agentSpeed = level.speed;
                        flagController.Agentposition = rb.position;
                        flagController.beingCarried = true;
                        isCarryingFlag = true;
                        flagCarrying = flagController;
                        rend.sharedMaterial = materials[1];*/
                    }

                }
                //verificar se o hit é a base dele e ele carrega a bandeira
                else if (hit.transform.name.Contains("BaseTeam") && hit.transform.CompareTag(transform.tag) && isCarryingFlag)
                {
                    flagCarrying.RestartPosition();
                    isCarryingFlag = false;
                    rend.sharedMaterial = materials[0];
                    //TODO aumentar de nivel quando pegar uma bandeira

                    //devo dar pontos por ter pego a bandeira
                    PlayerController pc;
                    switch (transform.tag)
                    {
                        case "Team1":
                            pc = GameObject.Find("PlayerController").GetComponent<PlayerController>();
                            pc.Points += flagCarrying.value;
                            pc.PointsPainel.text = pc.gameObject.tag + " Points: " + pc.Points;
                            Debug.Log(pc.Points);
                            break;

                        case "Team2":
                            pc = GameObject.Find("Player2Controller").GetComponent<PlayerController>();
                            pc.Points += flagCarrying.value;
                            pc.PointsPainel.text = pc.gameObject.tag + " Points: " + pc.Points;
                            Debug.Log(pc.Points);
                            break;

                        default:
                            break;
                    }
                }
                //verificar se são inimigos
                else if (!CheckFriendAgent(hit))
                {
                    // este agente é inimigo, setar um dano para ele
                    AgentController EnemyAgentController = hit.transform.GetComponent<AgentController>();
                    Invoke(nameof(EnemyAgentController.setDamage), 0.1f);
                    //TODO implementar o tiro indo até ele
                } 
                
            }
        }
    }
    private void setDamage()
    {
        level.life -= 1;
        lifeBar.localScale = new Vector3(level.life, 1, 1);
        //Debug.Log(level.life);
    }

    bool CheckFriendAgent(RaycastHit hit)
    {
        Rigidbody rigidbody;
        if (rigidbody = hit.transform.GetComponent<Rigidbody>())
            return rigidbody.transform.IsChildOf(this.transform.parent);
        return true;
    }

    public void BuildPath(Vector3Int ObjectivePosition)
    {

        LogicMap current = AStar.GetTileByPosition(new Vector3Int((int)Math.Round(rb.position.x)/10, (int)Math.Round(rb.position.y)/10, 0));
        LogicMap objective = AStar.GetTileByPosition(Vector3Int.FloorToInt(ObjectivePosition));

        if (!objective.Walkable)
            return;

        indexPath = 0;
        AStar.Search(current, objective);
        path = AStar.BuildPath(objective);
        followingpath = true;
    }

    private void Move()
    {
        Vector3 targetDirection = path[indexPath].ClickPosition - Vector3Int.FloorToInt(rb.position/10);

        rb.MovePosition(rb.position + (targetDirection * Time.fixedDeltaTime * level.speed));
    }
    
    private void CheckWayPoint()
    {
        Vector2 agentPosition = Vector2Int.FloorToInt(new Vector2(rb.position.x/10, rb.position.y/10));
        Vector2 newPosition =  new Vector2(path[indexPath].ClickPosition.x, path[indexPath].ClickPosition.y);
        

        if (Vector2.Distance(newPosition, agentPosition) < DistanceToChangeWayPoint)
        {
            CurrentPosition = path[indexPath].ClickPosition * 10;
            indexPath++;
            if (indexPath == path.Count)
            {
                followingpath = false;
                rb.angularVelocity.Set(0, 0, 0);
            }
           
        }

    }

    public Rigidbody GetRigidbody()
    {
        return this.rb;
    }

}
