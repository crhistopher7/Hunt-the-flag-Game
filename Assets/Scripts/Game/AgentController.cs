using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System.IO;
using System.Text.RegularExpressions;

public class AgentController : MatchBehaviour
{
    private AStar AStar;

    private GameObject prefabLine;

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
    public Color color;
    Renderer rend;
    public int seed;
    System.Random prng;
    Rigidbody rb;
    public TextMesh nameText;

    private bool alreadyTakenAPicture;


    // Start is called before the first frame update
    void Start()
    {
        sensor = GetComponent<Sensor>();
        rend = GetComponent<Renderer>();
        rb = GetComponent<Rigidbody>();
        rend.enabled = true;
        level = new AgentLevel(3);
        followingpath = false;
        //isCarryingFlag = false;
        //lifeBar.localScale = new Vector3(level.life, 1, 1);

        AStar = GameObject.Find(Constants.PATHFINDER).GetComponent<AStar>();

        prefabLine = Resources.Load("Prefabs/PathLine") as GameObject;
        SpriteRenderer back = transform.Find("Background").GetComponent<SpriteRenderer>();
        back.color = color;
        alreadyTakenAPicture = false;
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
        } while (point == null || !point.Walkable);

        this.transform.position = position;
    }

    internal void SetNameText(string name)
    {
        nameText = transform.Find("Background").GetComponentInChildren<TextMesh>();
        nameText.text = name;
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        //verificar se encontrou agentes ou bandeira ao seu redor 
        if (!alreadyTakenAPicture)
        {
            List<RaycastHit> listOfHit = sensor.Check();
            CheckHits(listOfHit);
        }
            

        // Se estiver seguindo um caminho se movimentar
        if (followingpath)
        {
            Move();
            CheckWayPoint();
            //if (isCarryingFlag)
            //    flagCarrying.Agentposition = rb.position;
        }
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
                if(hit.transform.CompareTag(Constants.TAG_FLAG) && hit.transform.name == Constants.REAL_GOAL)
                {
                    if (!alreadyTakenAPicture)
                    {
                        alreadyTakenAPicture = true;
                        SimulationController simulation = GameObject.Find("SimulationController").GetComponent<SimulationController>();
                        StartCoroutine(simulation.EndCase(transform.name, hit.transform.name));
                    }

                    // é uma bandeira, se for do inimigo e não estou carregando nada, devo carregar 
                    /*FlagController flagController = hit.transform.GetComponent<FlagController>();
                    if (!transform.CompareTag(flagController.team))// && !isCarryingFlag && !flagController.beingCarried)
                    {

                        /* flagController.agentSpeed = level.speed;
                         flagController.Agentposition = rb.position;
                         flagController.beingCarried = true;
                         isCarryingFlag = true;
                         flagCarrying = flagController;
                         rend.sharedMaterial = materials[1];
                    }*/

                }
                //verificar se o hit é a base dele e ele carrega a bandeira
                /*else if (hit.transform.name.Contains("BaseTeam") && hit.transform.CompareTag(transform.tag) && isCarryingFlag)
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
                } */
                
            }
        }
    }

    public void BuildPath(Vector3Int objectivePosition, Vector3Int deceptivePosition, PathType pathType, string MAP_HEIGHTMAP_FILE)
    {

        LogicMap current = AStar.GetTileByPosition(new Vector3Int((int)Math.Round(rb.position.x) / Constants.MAP_OFFSET, (int)Math.Round(rb.position.y) / Constants.MAP_OFFSET, 0));
        LogicMap objective = AStar.GetTileByPosition(Vector3Int.FloorToInt(objectivePosition));
        LogicMap deceptive = AStar.GetTileByPosition(Vector3Int.FloorToInt(deceptivePosition));

        if (!objective.Walkable)
        {
            Debug.Log("not walkable!");
            return;
        }
        else
        {
            if (Constants.USE_P4_CODE)
            {
                indexPath = 0;
                path = RunP4Code(current, objective, deceptive, pathType, MAP_HEIGHTMAP_FILE);
            }

            else if (pathType == PathType.NORMAL)
            {
                indexPath = 0;
                AStar.Search(current, objective);
                path = AStar.BuildPath(objective);
            }
            else
            {
                LogicMap deceptiveObjective = AStar.GetTileByPosition(Vector3Int.FloorToInt(deceptivePosition));

                if (!deceptiveObjective.Walkable)
                {
                    Debug.Log("not walkable!");
                    return;
                }

                indexPath = 0;
                if (pathType == PathType.DECEPTIVE_1)
                {
                    List<Vector2> area = OccupationAreaLimits(current.Position, deceptiveObjective.Position, objective.Position);
                    // Funcionando 
                    AStar.Search(current, deceptiveObjective);
                    path = AStar.BuildPath(deceptiveObjective);
                    AStar.Search(deceptiveObjective, objective);
                    List<LogicMap> secondPath = AStar.BuildPath(objective);

                    path.AddRange(secondPath);
                }
                else if (pathType == PathType.DECEPTIVE_2)
                {
                    //Achar ponto entre enganoso e objetivo
                    LogicMap target = FindTarget(current, objective, deceptive);
                    LogicMap ldpt = FindLDPt(target, current, objective, deceptive, pathType);
                    List<Vector2> area = OccupationAreaLimits(current.Position, deceptiveObjective.Position, objective.Position, target.Position, ldpt.Position);

                    if (target != null)
                    {
                        AStar.Search(current, ldpt);
                        path = AStar.BuildPath(ldpt);
                        AStar.Search(ldpt, objective);
                        List<LogicMap> secondPath = AStar.BuildPath(objective);
                        path.AddRange(secondPath);
                    }
                }
                else if (pathType == PathType.DECEPTIVE_3)
                {
                    // Encontra o target
                    LogicMap target = FindTarget(current, objective, deceptive);
                    LogicMap ldpt = FindLDPt(target, current, objective, deceptive, pathType);
                    List<Vector2> area = OccupationAreaLimits(current.Position, deceptiveObjective.Position, objective.Position, target.Position, ldpt.Position);

                    if (target != null)
                    {
                        // Custom a* (start, target, obj)
                        AStar.SearchAstarCustom3(current, ldpt, objective);
                        path = AStar.BuildPath(ldpt);

                        // Path target to obj
                        AStar.Search(ldpt, objective);
                        List<LogicMap> secondPath = AStar.BuildPath(objective);
                        path.AddRange(secondPath);
                    }
                }
                else
                {
                    Debug.Log("Deceptive 4");
                    LogicMap target = FindTarget(current, objective, deceptive);

                    if (target != null)
                    {
                        //calcular um astar para a char o custo
                        AStar.Search(current, objective);
                        float costReal = AStar.CostPath(AStar.BuildPath(objective));
                        AStar.Search(current, deceptive);
                        float costDeceptive = AStar.CostPath(AStar.BuildPath(deceptive));

                        LogicMap ldpt = FindLDPt(target, current, objective, deceptive, pathType, costReal, costDeceptive);
                        List<Vector2> area = OccupationAreaLimits(current.Position, deceptiveObjective.Position, objective.Position, target.Position, ldpt.Position);

                        // CustomAstar (start , target)
                        AStar.SearchAstarCustom4(current, ldpt, objective, deceptiveObjective, costReal, costDeceptive);
                        path = AStar.BuildPath(ldpt);
                        // Path 2
                        AStar.Search(ldpt, objective);
                        List<LogicMap> secondPath = AStar.BuildPath(objective);
                        path.AddRange(secondPath);
                    }
                }
                      
            }

            if (path.Count != 0)
            {
                followingpath = true;

                GameObject go;
                go = Instantiate(prefabLine);
                go.GetComponent<LineRenderer>().startColor = color;
                go.GetComponent<LineRenderer>().endColor = color;
                LineController line = go.GetComponent<LineController>();

                line.SetUpLine(path, rb.position / Constants.MAP_OFFSET, pathType);
            }
            else
            {
                Debug.Log("Path Vazio, não foi possível construir um caminho");
            }
        }
    }

    private List<LogicMap> RunP4Code(LogicMap current, LogicMap objective, LogicMap deceptive, PathType pathType, string MAP_HEIGHTMAP_FILE)
    {
        string agent = "";
        if (pathType == PathType.NORMAL)
            agent = @"C:\Users\crisl\OneDrive\Documentos\p4 deception project to 3d terrain\p4-simulator-gr-master\src\agents\agent_astar";
        else if (pathType == PathType.DECEPTIVE_1)
            agent = @"C:\Users\crisl\OneDrive\Documentos\p4 deception project to 3d terrain\p4-simulator-gr-master\src\agents\agent_ds1";
        else if (pathType == PathType.DECEPTIVE_2)
            agent = @"C:\Users\crisl\OneDrive\Documentos\p4 deception project to 3d terrain\p4-simulator-gr-master\src\agents\agent_ds2";
        else if (pathType == PathType.DECEPTIVE_3)
            agent = @"C:\Users\crisl\OneDrive\Documentos\p4 deception project to 3d terrain\p4-simulator-gr-master\src\agents\agent_ds3";
        else
            agent = @"C:\Users\crisl\OneDrive\Documentos\p4 deception project to 3d terrain\p4-simulator-gr-master\src\agents\agent_ds4";

        string pathfinder = "astar";
        string start = current.Position.x.ToString() + "," + current.Position.y.ToString();
        string deceptiveGoal = deceptive.Position.x.ToString() + "," + deceptive.Position.y.ToString();
        string realGoal = objective.Position.x.ToString() + "," + objective.Position.y.ToString();
        string map = MAP_HEIGHTMAP_FILE.Replace(".png", ".tif");
        string quotedMapImagePath = $"\"{map}\"";

        // Caminho do executável do Python dentro do ambiente virtual "tcc_ricardo"
        string pythonFilePath = @"C:\Users\crisl\anaconda3\envs\tcc_ricardo\python.exe";
        string pythonArguments = $"-m {quotedMapImagePath} -s {start} -G {deceptiveGoal} -g {realGoal} -a \"{agent}\" -k {pathfinder} -ad";
        string scriptPath = @"C:\Users\crisl\OneDrive\Documentos\p4 deception project to 3d terrain\p4-simulator-gr-master\src\p4.py";

        Debug.Log(pythonArguments);


        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = pythonFilePath,
            Arguments = $"\"{scriptPath}\" {pythonArguments}",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true // Criar sem janela para evitar exibição extra do CMD
        };

        // Iniciando o processo do Python
        Process process = new Process
        {
            StartInfo = psi
        };

        // Iniciar o processo e os redirecionamentos de saída
        process.Start();
        StreamReader standardOutputReader = process.StandardOutput;
        StreamReader standardErrorReader = process.StandardError;

        // Ler a saída do processo Python (saída padrão e saída de erro) após a conclusão
        string output = standardOutputReader.ReadToEnd();
        string errorOutput = standardErrorReader.ReadToEnd();

        process.WaitForExit();

        // Exibindo a saída e saída de erro
        Debug.Log("Saída do Python: \n" + output);
        Debug.Log("Saída de erro do Python: \n" + errorOutput);

        List<LogicMap> logicMapList = new List<LogicMap>();
        if (output.Contains("FULL PATH"))
        {
            string listaString = output.Split(':').Last();
            Debug.Log(listaString);

            // Remover os colchetes e espaços para obter apenas as coordenadas
            string coordinatesString = listaString.Replace("[", "").Replace("]", "").Replace(" ", "");

            // Usar expressão regular para extrair os números de cada coordenada
            Regex regex = new Regex(@"\((\d+),(\d+)\)");
            MatchCollection matches = regex.Matches(coordinatesString);

            // Converter cada par de coordenadas em um Vector3Int e adicioná-lo à lista
            foreach (Match match in matches)
            {
                int x = int.Parse(match.Groups[1].Value);
                int y = int.Parse(match.Groups[2].Value);

                LogicMap point = AStar.GetTileByPosition(new Vector3Int(Constants.CLICK_POSITION_OFFSET + x, Constants.CLICK_POSITION_OFFSET + y, 0));
                logicMapList.Add(point);
            }

        }

        return logicMapList;
    }

    public List<Vector2> OccupationAreaLimits(Vector3Int a, Vector3Int b, Vector3Int c, float tolerance = 5f)
    {
        // Calcula os valores máximos e mínimos permitidos em x e y
        float max_x = Mathf.Min(Mathf.Max(a.x, b.x, c.x) + tolerance, Constants.IMAGE_SIZE[0] - 1);
        float max_y = Mathf.Min(Mathf.Max(a.y, b.y, c.y) + tolerance, Constants.IMAGE_SIZE[1] - 1);
        float min_x = Mathf.Max(Mathf.Min(a.x, b.x, c.x) - tolerance, 1);
        float min_y = Mathf.Max(Mathf.Min(a.y, b.y, c.y) - tolerance, 1);

        return new List<Vector2> { new Vector2(max_x, max_y), new Vector2(min_x, min_y) };
    }

    public List<Vector2> OccupationAreaLimits(Vector3Int a, Vector3Int b, Vector3Int c, Vector3Int d,  float tolerance = 5f)
    {
        // Calcula os valores máximos e mínimos permitidos em x e y
        float max_x = Mathf.Min(Mathf.Max(a.x, b.x, c.x, d.x) + tolerance, Constants.IMAGE_SIZE[0] - 1);
        float max_y = Mathf.Min(Mathf.Max(a.y, b.y, c.y, d.y) + tolerance, Constants.IMAGE_SIZE[1] - 1);
        float min_x = Mathf.Max(Mathf.Min(a.x, b.x, c.x, d.x) - tolerance, 1);
        float min_y = Mathf.Max(Mathf.Min(a.y, b.y, c.y, d.y) - tolerance, 1);

        return new List<Vector2> { new Vector2(max_x, max_y), new Vector2(min_x, min_y) };
    }

    public List<Vector2> OccupationAreaLimits(Vector3Int a, Vector3Int b, Vector3Int c, Vector3Int d, Vector3Int e, float tolerance = 5f)
    {
        // Calcula os valores máximos e mínimos permitidos em x e y
        float max_x = Mathf.Min(Mathf.Max(a.x, b.x, c.x, d.x, e.x) + tolerance, Constants.IMAGE_SIZE[0] - 1);
        float max_y = Mathf.Min(Mathf.Max(a.y, b.y, c.y, d.y, e.y) + tolerance, Constants.IMAGE_SIZE[1] - 1);
        float min_x = Mathf.Max(Mathf.Min(a.x, b.x, c.x, d.x, e.x) - tolerance, 1);
        float min_y = Mathf.Max(Mathf.Min(a.y, b.y, c.y, d.y, e.y) - tolerance, 1);

        return new List<Vector2> { new Vector2(min_x, min_y), new Vector2(max_x, max_y) };
    }


    private LogicMap FindTarget(LogicMap start, LogicMap objective, LogicMap deceptive)
    {
        // calculo o caminho e custo entre o start e o ponto enganoso
        AStar.Search(start, deceptive);
        List<LogicMap> path = AStar.BuildPath(objective);
        float cost1 = AStar.CostPath(path); // Aqui vai o custo do path do current -> deceptive

        // calculo o caminho e custo entre o ponto enganoso e o objetivo real
        AStar.Search(deceptive, objective);
        path = AStar.BuildPath(objective);
        
        // E se não tiver um caminho entre os dois?
        if (path.Count == 0)
        {
            return null;
        }

        float cost2 = AStar.CostPath(path); // Aqui vai o custo do path do deceptive -> objective
        float targetCost = cost2 - cost1;

        LogicMap currentPosition = deceptive;

        int next = 1;
        for (float costSoFar = 0f; costSoFar < targetCost; next++)
        {
            List<LogicMap> tempList = new List<LogicMap>
            {
                currentPosition,
                path[next]
            };
            cost2 = AStar.CostPath(tempList); // Calcula o custo entre um ponto e o próximo
            currentPosition = path[next];
            costSoFar += cost2;
        }

        Debug.Log("Find LDP: " + path[next].Position.ToString());
        return path[next];
    }


    private LogicMap FindLDPt(LogicMap target, LogicMap start, LogicMap goal, LogicMap fakegoal, PathType pathType,  float costToReal=0, float costToDeceptive=0)
    {
        // calcular area  area = model.occupation_area_limits(start, fakegoal, goal, target)
        List<Vector2> area = OccupationAreaLimits(start.Position, fakegoal.Position, goal.Position, target.Position);

        if (pathType == PathType.DECEPTIVE_3)
        {
            AStar.SearchAstarCustom3(start, target, goal);
        }
        else if (pathType == PathType.DECEPTIVE_4)
        {
            AStar.SearchAstarCustom4(start, target, goal, fakegoal, costToReal, costToDeceptive);
        }
        else
        {
            AStar.Search(start, target);
        }

        List<LogicMap> path = AStar.BuildPath(target);
        Dictionary<LogicMap, float> ratios = new Dictionary<LogicMap, float>();

        foreach (LogicMap node in path)
        {
            // Ratio(n) = cost(S, n) / cost(G, LDP)

            float costSP = AStar.GetJustCost(start, node); 
            float costG = AStar.GetJustCost(goal, node);
            if (costG != 0)
            {
                float ratio = costSP / costG;
                ratios[node] = ratio;
            }
        }

        LogicMap ldpt = ratios.OrderBy(kv => kv.Value).First().Key;
        Debug.Log("Find LDP: " + ldpt.Position.ToString());
        return ldpt;
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
}
