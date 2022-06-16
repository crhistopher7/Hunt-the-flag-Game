using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentController : MatchBehaviour
{
    private AStar AStar;
    private DeceptiveAStar_1 DeceptiveAStar_1;
    private DeceptiveAStar_2 DeceptiveAStar_2;
    private DeceptiveAStar_3 DeceptiveAStar_3;
    private DeceptiveAStar_4 DeceptiveAStar_4;

   
    protected Vector3 CurrentPosition;
    private float distanceToChangeWayPoint = 0.5f;
    List<LogicMap> path;
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
    Rigidbody rb;


    // Start is called before the first frame update
    void Start()
    {
        sensor = GetComponent<Sensor>();
        rend = GetComponent<Renderer>();
        rb = GetComponent<Rigidbody>();
        rend.enabled = true;
        level = new AgentLevel(1);
        followingpath = false;
        isCarryingFlag = false;
        lifeBar.localScale = new Vector3(level.life, 1, 1);

        AStar = GameObject.Find(Constants.PATHFINDER).GetComponent<AStar>();
        DeceptiveAStar_1 = GameObject.Find(Constants.PATHFINDER).GetComponent<DeceptiveAStar_1>();
        DeceptiveAStar_2 = GameObject.Find(Constants.PATHFINDER).GetComponent<DeceptiveAStar_2>();
        DeceptiveAStar_3 = GameObject.Find(Constants.PATHFINDER).GetComponent<DeceptiveAStar_3>();
        DeceptiveAStar_4 = GameObject.Find(Constants.PATHFINDER).GetComponent<DeceptiveAStar_4>();
    }

    public void InitPosition(int seed)
    {
        AStar = GameObject.Find(Constants.PATHFINDER).GetComponent<AStar>();
        var limits_x = CompareTag(Constants.TAG_TEAM_1) ? Constants.LIMITS_X_AGENT_TEAM_1 : Constants.LIMITS_X_AGENT_TEAM_2;
        var limits_y = CompareTag(Constants.TAG_TEAM_1) ? Constants.LIMITS_Y_AGENT_TEAM_1 : Constants.LIMITS_Y_AGENT_TEAM_2;

        prng = new System.Random(seed);
        LogicMap point;
        Vector3Int position;
        do
        {
            int x = prng.Next(limits_x[0], limits_x[1]);
            int y = prng.Next(limits_y[0], limits_y[1]);
            int z = Constants.AGENT_POSITION_Z;

            position = new Vector3Int(x, y, z);

            point = AStar.GetTileByPosition(Vector3Int.FloorToInt(new Vector3Int(x, y, 0)) / Constants.MAP_OFFSET);
        } while (!point.Walkable);

        this.transform.position = position;
    }


    // Update is called once per frame
    void Update()
    {
        //verificar se esse agente morreu
        //if(level.life <= 0)
            //Die();


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
        if (transform.CompareTag(Constants.TAG_TEAM_1))
            pc = GameObject.Find(Constants.PLAYER_CONTROLLER_1).GetComponent<PlayerController>();
        else
            pc = GameObject.Find(Constants.PLAYER_CONTROLLER_2).GetComponent<PlayerController>();
       
        pc.Agents.Remove(this);

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
                if(hit.transform.CompareTag(Constants.TAG_FLAG))
                {
                    // é uma bandeira, se for do inimigo e não estou carregando nada, devo carregar 
                    FlagController flagController = hit.transform.GetComponent<FlagController>();
                    if (!transform.CompareTag(flagController.team) && !isCarryingFlag && !flagController.beingCarried)
                    {
                        SimulationController simulation = GameObject.Find("SimulationController").GetComponent<SimulationController>();
                        simulation.EndCase();


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
                }
                //verificar se são inimigos
                else if (!CheckFriendAgent(hit))
                {
                    // este agente é inimigo, setar um dano para ele
                    //AgentController EnemyAgentController = hit.transform.GetComponent<AgentController>();
                    //Invoke(nameof(EnemyAgentController.SetDamage), 0.1f);
                } 
                
            }
        }
    }
    private void SetDamage()
    {
        level.life -= 1;
        lifeBar.localScale = new Vector3(level.life, 1, 1);
    }

    bool CheckFriendAgent(RaycastHit hit)
    {
        Rigidbody rigidbody;
        if (rigidbody = hit.transform.GetComponent<Rigidbody>())
            return rigidbody.transform.IsChildOf(this.transform.parent);
        return true;
    }

    public void BuildPath(Vector3Int objectivePosition, Vector3Int deceptivePosition, PathType pathType)
    {

        LogicMap current = AStar.GetTileByPosition(new Vector3Int((int)Math.Round(rb.position.x) / Constants.MAP_OFFSET, (int)Math.Round(rb.position.y) / Constants.MAP_OFFSET, 0));
        LogicMap objective = AStar.GetTileByPosition(Vector3Int.FloorToInt(objectivePosition));
        LogicMap deceptive = AStar.GetTileByPosition(Vector3Int.FloorToInt(deceptivePosition));

        if (!objective.Walkable)
            return;

        if (pathType == PathType.NORMAL)
        {
            indexPath = 0;
            AStar.Search(current, objective);
            path = AStar.BuildPath(objective);
        }
        else
        {
            LogicMap deceptiveObjective = AStar.GetTileByPosition(Vector3Int.FloorToInt(deceptivePosition));

            if (!deceptiveObjective.Walkable)
                return;

            indexPath = 0;
            if (pathType == PathType.DECEPTIVE_1)
            {
                // Funcionando 
                AStar.Search(current, deceptiveObjective);
                path = AStar.BuildPath(deceptiveObjective);
                AStar.Search(deceptiveObjective, objective);
                var secondPath = AStar.BuildPath(objective);

                path.AddRange(secondPath);
            }
            else if (pathType == PathType.DECEPTIVE_2)
            {
                // WIP: está dando erro no FindTarget

                //DeceptiveAStar_2.Search(current, objective, deceptiveObjective);
                LogicMap target = FindTarget(current, objective, deceptive);

                AStar.Search(current, target);
                path = AStar.BuildPath(deceptiveObjective);
                AStar.Search(deceptiveObjective, objective);
                var secondPath = AStar.BuildPath(objective);
                path.AddRange(secondPath);
            }
            else if (pathType == PathType.DECEPTIVE_3)
            {
                // WIP: está dando erro no FindTarget
                
                //DeceptiveAStar_3.Search(current, objective, deceptiveObjective);
                LogicMap target = FindTarget(current, objective, deceptive);

                AStar.Search(current, target);
                path = AStar.BuildPath(deceptiveObjective);
                AStar.Search(deceptiveObjective, objective);
                var secondPath = AStar.BuildPath(objective);

                path.AddRange(secondPath);
            }
            else
            {
                // Não esta Implementado 
                DeceptiveAStar_4.Search(current, objective, deceptiveObjective);
                path = DeceptiveAStar_4.BuildPath(objective);
            }
        }
       
        if(path.Count == 1)
        {
            path.Clear();
        } 
        else
        {
            followingpath = true;
            //TODO Desenhar path no mapa com lines
        }
    }

    private LogicMap FindTarget(LogicMap current, LogicMap objective, LogicMap deceptive)
    {
        AStar.Search(current, deceptive);
        var path1 = AStar.BuildPath(deceptive);
        var rmp = AStar.CostPath(current); // Aqui vai o custo do path do current -> deceptive
        AStar.Search(deceptive, objective);
        var pathDecptiveObjective = AStar.BuildPath(objective);
        var cost = AStar.CostPath(deceptive); ; // Aqui vai o custo do path do deceptive -> objective
        var targetCost = cost - rmp;

        LogicMap currentPosition = deceptive;
        var costSoFar = 0f;
        var next = 1;
        while(costSoFar < targetCost)
        {
            AStar.Search(currentPosition, pathDecptiveObjective[next]);
            cost = AStar.CostPath(currentPosition); // ^^^^ 
            costSoFar = costSoFar + cost;
            currentPosition = pathDecptiveObjective[next];
            next = next + 1;
        }

        while(pathDecptiveObjective[next].Walkable)
        {
            next = next - 1;
        }
        return pathDecptiveObjective[next-1];
    }

    private void Move()
    {
        Vector3 targetDirection = path[indexPath].ClickPosition - Vector3Int.FloorToInt(rb.position / Constants.MAP_OFFSET);

        rb.MovePosition(rb.position + (targetDirection * Time.fixedDeltaTime * level.speed));
        //transform.position = Vector3.MoveTowards(transform.position, targetDirection, level.speed * Time.fixedDeltaTime);
    }
    
    private void CheckWayPoint()
    {
        Vector2 agentPosition = Vector2Int.FloorToInt(new Vector2(rb.position.x / Constants.MAP_OFFSET, rb.position.y / Constants.MAP_OFFSET));
        Vector2 newPosition =  new Vector2(path[indexPath].ClickPosition.x, path[indexPath].ClickPosition.y);
        

        if (Vector2.Distance(newPosition, agentPosition) < distanceToChangeWayPoint)
        {
            CurrentPosition = path[indexPath].ClickPosition * Constants.MAP_OFFSET;
            indexPath++;
            if (indexPath == path.Count)
            {
                followingpath = false;
                rb.angularVelocity.Set(0, 0, 0);
            }
           
        }

    }
}
