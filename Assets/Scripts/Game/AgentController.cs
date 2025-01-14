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
using UnityEditor.Experimental.GraphView;

public class AgentController : MatchBehaviour
{
    private SimulationController simulation;
    private AStar AStar;
    private GameObject prefabLine;


    protected Vector3 CurrentPosition;
    private float distanceToChangeWayPoint = 0.5f;
    List<LogicMap> path;
    LogicMap ldpt_point;
    int ldptIndex;
    bool has_ldpt;
    int currentGoalIndex;
    int indexPath;
    private bool travelToLDPT;
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
        has_ldpt = false;
        travelToLDPT = false;
        ldptIndex = -1;
        //isCarryingFlag = false;
        //lifeBar.localScale = new Vector3(level.life, 1, 1);

        simulation = GameObject.Find("SimulationController").GetComponent<SimulationController>();
        AStar = GameObject.Find(Constants.PATHFINDER).GetComponent<AStar>();

        prefabLine = Resources.Load("Prefabs/PathLine") as GameObject;
        SpriteRenderer back = transform.Find("Background").GetComponent<SpriteRenderer>();
        back.color = color;
        alreadyTakenAPicture = false;
        //Debug.Log("End Agent Start");
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
        //verificar se � vazio a lista
        if (listOfHit.Count != 0)
        {
            // cont�m algum agente ou bandeira na lista 
            foreach (RaycastHit hit in listOfHit)
            {
                //verificar se � uma bandeira
                if(hit.transform.CompareTag(Constants.TAG_FLAG) && hit.transform.name == Constants.REAL_GOAL)
                {
                    if (!alreadyTakenAPicture)
                    {
                        alreadyTakenAPicture = true;                        
                        StartCoroutine(simulation.EndCase(transform.name, hit.transform.name));
                    }

                    // � uma bandeira, se for do inimigo e n�o estou carregando nada, devo carregar 
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
                //verificar se o hit � a base dele e ele carrega a bandeira
                /*else if (hit.transform.name.Contains("BaseTeam") && hit.transform.CompareTag(transform.tag) && isCarryingFlag)
                {
                    flagCarrying.RestartPosition();
                    isCarryingFlag = false;
                    rend.sharedMaterial = materials[0];
                    //TODO aumentar de nivel quando pegar uma bandeira
                }
                //verificar se s�o inimigos
                else if (!CheckFriendAgent(hit))
                {
                    // este agente � inimigo, setar um dano para ele
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
                Debug.Log(path);
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
                if (simulation.GetAutomaticTravel())
                {
                    currentGoalIndex = path.Count;
                    Debug.Log("Automatic Travel, seguindo rota!");

                    GameObject go;
                    go = Instantiate(prefabLine);
                    go.GetComponent<LineRenderer>().startColor = color;
                    go.GetComponent<LineRenderer>().endColor = color;
                    LineController line = go.GetComponent<LineController>();

                    line.SetUpLine(path, rb.position / Constants.MAP_OFFSET, pathType);
                }
                else
                {
                    Debug.Log("Manual Travelling!");
                    simulation.SetManualTravelling(true, path.Count, this);
                }
            }
            else
            {
                Debug.Log("Path Vazio, n�o foi poss�vel construir um caminho");
            }
        }
    }

    private List<LogicMap> RunP4Code(LogicMap current, LogicMap objective, LogicMap deceptive, PathType pathType, string MAP_HEIGHTMAP_FILE)
    {
        string agent = "";
        if (pathType == PathType.NORMAL)
            agent = Constants.AGENT_NORMAL;
        else if (pathType == PathType.DECEPTIVE_1)
            agent = Constants.AGENT_DS1;
        else if (pathType == PathType.DECEPTIVE_2)
            agent = Constants.AGENT_DS2;
        else if (pathType == PathType.DECEPTIVE_3)
            agent = Constants.AGENT_DS3;
        else
            agent = Constants.AGENT_DS4;

        string pathfinder = "astar";
        string start = current.Position.x.ToString() + "," + current.Position.y.ToString();
        string deceptiveGoal = deceptive.Position.x.ToString() + "," + deceptive.Position.y.ToString();
        string realGoal = objective.Position.x.ToString() + "," + objective.Position.y.ToString();
        string map = MAP_HEIGHTMAP_FILE.Replace(".png", ".tif");
        string quotedMapImagePath = $"\"{map}\"";

        string pythonFilePath = Constants.PYTHON_FILE_PATH;
        string pythonArguments = $"-m {quotedMapImagePath} -s {start} -G {deceptiveGoal} -g {realGoal} -a \"{agent}\" -k {pathfinder} -ad";
        string scriptPath = Constants.SCRIPT_FILE_PATH;

        Debug.Log(pythonArguments);

        // Caminho do script conda.bat e nome do ambiente
        string condaScript = @"C:\Users\Thiago\anaconda3\condabin\conda.bat";
        string condaEnv = "tfenv2";

        // Comando para ativar o ambiente e chamar o script Python
        string command = $"/C \"CALL {condaScript} activate {condaEnv} && python {scriptPath} {pythonArguments}\"";

        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = command,
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
            // Usar expressão regular para extrair os números de cada coordenada
            Regex regex = new Regex(@"\((\d+),\s*(\d+)\)");

            // Se não houver LDPT, o output deve ser FULL PATH: [...] -> splitado em 2 elementos 
            // Se houver LDPT, o output deve ser FULL PATH: [...] :LDPT: (X, y) -> splitado em 4 elementos.
            string[] splitted_output = output.Split('?').Last().Split(':');
            string listaString = splitted_output[1];

            if (output.Contains("LDPT"))
            {
                // Se houver ldpt, ele vai ser o quarto (último) elemento do split.
                string ldptstring = splitted_output[3];
                Debug.Log("LDPT");
                Debug.Log(ldptstring);
                Debug.Log(splitted_output);

                Match match = regex.Match(ldptstring);

                if (match.Success)
                {
                    Debug.Log("Correspondência encontrada: " + match.Value);
                    Debug.Log("Grupo 1: " + match.Groups[1].Value);
                    Debug.Log("Grupo 2: " + match.Groups[2].Value);

                    try
                    {
                        int x1 = int.Parse(match.Groups[1].Value);
                        int y1 = int.Parse(match.Groups[2].Value);
                        // Use x e y conforme necessário
                        Debug.Log("Valores convertidos com sucesso: x=" + x1 + ", y=" + y1);
                    }
                    catch (FormatException e)
                    {
                        Debug.LogError("Erro ao converter os valores para inteiros: " + e.Message);
                    }
                    catch (ArgumentNullException e)
                    {
                        Debug.LogError("Valor nulo encontrado ao tentar converter para inteiros: " + e.Message);
                    }
                }
                else
                {
                    Debug.LogError("Nenhuma correspondência encontrada para a string fornecida.");
                }

                int x = int.Parse(match.Groups[1].Value);
                int y = int.Parse(match.Groups[2].Value);
                ldpt_point = AStar.GetTileByPosition(new Vector3Int(Constants.CLICK_POSITION_OFFSET + x, Constants.CLICK_POSITION_OFFSET + y, 0));
                has_ldpt = true;
                simulation.travelToLDPTToggle.interactable = true;

                Debug.Log(ldptstring);
            }
            else
            {
                ldptIndex = -1;
                has_ldpt = false;
                simulation.travelToLDPTToggle.interactable = false;
            }
            Debug.Log(listaString);

            // Remover os colchetes e espaços para obter apenas as coordenadas
            string coordinatesString = listaString.Replace("[", "").Replace("]", "").Replace(" ", "");


            MatchCollection matches = regex.Matches(coordinatesString);
            // Converter cada par de coordenadas em um Vector3Int e adicioná-lo à lista
            foreach (Match match in matches)
            {
                Debug.Log("MATCH");
                Debug.Log(match);
                int x = int.Parse(match.Groups[1].Value);
                int y = int.Parse(match.Groups[2].Value);

                LogicMap point = AStar.GetTileByPosition(new Vector3Int(Constants.CLICK_POSITION_OFFSET + x, Constants.CLICK_POSITION_OFFSET + y, 0));
                logicMapList.Add(point);
            }

            if (has_ldpt)
            {
                // Encontrar o índice de ldpt_point na lista logicMapList
                ldptIndex = logicMapList.FindIndex(point => point.Equals(ldpt_point));
                Debug.Log("Índice de ldpt_point em logicMapList: " + ldptIndex);
            }

        }

        return logicMapList;
    }



    public List<Vector2> OccupationAreaLimits(Vector3Int a, Vector3Int b, Vector3Int c, float tolerance = 5f)
    {
        // Calcula os valores m�ximos e m�nimos permitidos em x e y
        float max_x = Mathf.Min(Mathf.Max(a.x, b.x, c.x) + tolerance, Constants.IMAGE_SIZE[0] - 1);
        float max_y = Mathf.Min(Mathf.Max(a.y, b.y, c.y) + tolerance, Constants.IMAGE_SIZE[1] - 1);
        float min_x = Mathf.Max(Mathf.Min(a.x, b.x, c.x) - tolerance, 1);
        float min_y = Mathf.Max(Mathf.Min(a.y, b.y, c.y) - tolerance, 1);

        return new List<Vector2> { new Vector2(max_x, max_y), new Vector2(min_x, min_y) };
    }

    public List<Vector2> OccupationAreaLimits(Vector3Int a, Vector3Int b, Vector3Int c, Vector3Int d,  float tolerance = 5f)
    {
        // Calcula os valores m�ximos e m�nimos permitidos em x e y
        float max_x = Mathf.Min(Mathf.Max(a.x, b.x, c.x, d.x) + tolerance, Constants.IMAGE_SIZE[0] - 1);
        float max_y = Mathf.Min(Mathf.Max(a.y, b.y, c.y, d.y) + tolerance, Constants.IMAGE_SIZE[1] - 1);
        float min_x = Mathf.Max(Mathf.Min(a.x, b.x, c.x, d.x) - tolerance, 1);
        float min_y = Mathf.Max(Mathf.Min(a.y, b.y, c.y, d.y) - tolerance, 1);

        return new List<Vector2> { new Vector2(max_x, max_y), new Vector2(min_x, min_y) };
    }

    public List<Vector2> OccupationAreaLimits(Vector3Int a, Vector3Int b, Vector3Int c, Vector3Int d, Vector3Int e, float tolerance = 5f)
    {
        // Calcula os valores m�ximos e m�nimos permitidos em x e y
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
        
        // E se n�o tiver um caminho entre os dois?
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
            cost2 = AStar.CostPath(tempList); // Calcula o custo entre um ponto e o pr�ximo
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
        int localGoalIndex = currentGoalIndex;
        if (travelToLDPT)
        {
            if (has_ldpt && ldptIndex > -1)
                localGoalIndex = ldptIndex;
            else Debug.Log("Está tentando caminhar até o LDPT, mas o LDPT não existe");
        }


        Vector2 agentPosition = Vector2Int.FloorToInt(new Vector2(rb.position.x / Constants.MAP_OFFSET, rb.position.y / Constants.MAP_OFFSET));
        Vector2 newPosition =  new Vector2(path[indexPath].ClickPosition.x, path[indexPath].ClickPosition.y);

        if (Vector2.Distance(newPosition, agentPosition) < distanceToChangeWayPoint)
        {
            CurrentPosition = path[indexPath].ClickPosition * Constants.MAP_OFFSET;

            // Agente pode voltar pra trás por conta do manual travelling!
            if(localGoalIndex > indexPath)
                indexPath++;
            else if (indexPath == localGoalIndex)
            {
                followingpath = false;
                rb.angularVelocity.Set(0, 0, 0);
            }
            else if (indexPath > 0) indexPath--;
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

    public void ChangeCurrentGoal(LogicMap point)
    {
        if (path.Count > 0)
        {
            for (int i = 0; i < path.Count; i++)
            {
                if (path[i] == point)
                {
                    currentGoalIndex = i;
                    break;
                }
            }
            followingpath = true;
        }
    }
    public void ChangeCurrentGoal(int currentGoal)
    {
        if(path.Count > 0)
        {
            currentGoalIndex = currentGoal;
            followingpath = true;
        }
    }
    public void ChangeCurrentGoalPercentage(int currentGoal, int relativeMax)
    {
        double relativeGoal = Math.Round(((double) currentGoal / (double) relativeMax) * (double) path.Count);

        Debug.Log("Relative: " + relativeGoal.ToString() + " Current: " + currentGoal.ToString() + "RelativeMax: " + relativeMax.ToString());
        if (path.Count > 0)
        {
            currentGoalIndex = (int) relativeGoal;
            followingpath = true;
        }
    }


    public void SetTravelToLDPT(bool value)
    {
        if(path.Count <= 0)
            { return; }

        travelToLDPT = value;
        simulation.pathSlider.value = ldptIndex;
        followingpath = true;
    }

    private void Die()
    {
        // verificar se estava carregando uma bandeira
        if (isCarryingFlag)
        {
            // dizer q a bandeira n�o esta mais sendo carregada
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

    public bool HasLDPT() { return has_ldpt; }
}
