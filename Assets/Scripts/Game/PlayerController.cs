using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MatchBehaviour
{
    public List<AgentController> Agents;
    public int numberOfAgents;
    System.Random prng;

    Transform realTransform;
    Transform deceptiveTransform;

    void Awake()
    {
        SetVariables();
    }

    /// <summary>
    /// Fun��o que seta valores iniciais em vari�veis
    /// </summary>
    private void SetVariables()
    {
        Agents = new List<AgentController>();
        prng = new System.Random();

        Transform realTransform = FindTransformWithNameContaining("Real");
        Transform deceptiveTransform = FindTransformWithNameContaining("Deceptive");
    }

    public void SetRealGoalPosition(float x, float y)
    {
        realTransform.position = new Vector3(x, y, realTransform.position.z);
    }

    public void SetDeceptiveGoalPosition(float x, float y)
    {
        deceptiveTransform.position = new Vector3(x, y, deceptiveTransform.position.z);
    }

    public Vector3Int GetRealGoalPosition()
    {
        if (realTransform != null)
            return Vector3Int.FloorToInt(new Vector3(realTransform.position.x, realTransform.position.y, 0));
        else
            throw new Exception("N�o encontrado o objetivo real no player controller");
    }

    public Vector3Int GetDeceptiveGoalPosition()
    {
        if (deceptiveTransform != null)
            return Vector3Int.FloorToInt(new Vector3(deceptiveTransform.position.x, deceptiveTransform.position.y, 0));
        else
            throw new Exception("N�o encontrado o objetivo enganoso no player controller");
    }


    private Transform FindTransformWithNameContaining(string partialName)
    {
        Transform[] childTransforms = transform.GetComponentsInChildren<Transform>(true);

        foreach (Transform childTransform in childTransforms)
        {
            if (childTransform.name.Contains(partialName))
            {
                return childTransform;
            }
        }

        return null;
    }

    /// <summary>
    /// Fun��o que inicializa os agentes de player de acordo com a quantidade de agentes setada em posi��es randomicas
    /// </summary>
    public void StartAgents(int id)
    {
        Agents.Clear();
        prng = new System.Random(id);

        GameObject prefab = Resources.Load("Prefabs/Agent") as GameObject;
        GameObject go;
        string name = "Agent";
        Material material = (CompareTag(Constants.TAG_TEAM_1)) ? 
            Resources.Load(Constants.MATERIAL_AGENT_TEAM_1, typeof(Material)) as Material 
            : Resources.Load(Constants.MATERIAL_AGENT_TEAM_2, typeof(Material)) as Material;

        Color color = (CompareTag(Constants.TAG_TEAM_1)) ? new Color(0, 0, 0.5f, 0.5f) : new Color(0.5f,0,0, 0.5f);

        int i = 1;
        do
        {
            go = Instantiate(prefab);
           
            AgentController agent = go.GetComponent<AgentController>();
            agent.name = name + i.ToString();
            agent.tag = this.tag;
            agent.color = color;
            agent.GetComponent<Renderer>().sharedMaterial = material;
            agent.transform.SetParent(this.transform);
            agent.transform.localScale = new Vector3(Constants.AGENT_SCALE, Constants.AGENT_SCALE, 1);
            agent.InitPosition(prng.Next());
            agent.SetNameText(i.ToString());
            this.Agents.Add(agent);
            i++;
        } while (i <= numberOfAgents);
    }

    /// <summary>
    /// Fun��o que recebe o movimento que um determinado agente deve executar
    /// </summary>
    /// <param name="name">Nome do agente</param>
    /// <param name="objectivePosition">Posi��o do objetivo</param>
    /// <param name="deceptivePosition">Posi��o do objetivo enganoso</param>
    /// <param name="pathType">Tipo de Pathfinder usado</param>
    public void ReceiveMove(string name, Vector3Int objectivePosition, Vector3Int deceptivePosition, PathType pathType, string MAP_HEIGHTMAP_FILE)
    {
        foreach (AgentController agent in Agents)
            if (agent.name == name)
            {
                agent.BuildPath(objectivePosition, deceptivePosition, pathType, MAP_HEIGHTMAP_FILE);
                return;
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

        // Caminho do execut�vel do Python dentro do ambiente virtual "tcc_ricardo"
        string pythonFilePath = Constants.PYTHON_FILE_PATH;
        string pythonArguments = $"-m {quotedMapImagePath} -s {start} -G {deceptiveGoal} -g {realGoal} -a \"{agent}\" -k {pathfinder} -ad";
        string scriptPath = Constants.SCRIPT_FILE_PATH;

        Debug.Log(pythonArguments);


        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = pythonFilePath,
            Arguments = $"\"{scriptPath}\" {pythonArguments}",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true // Criar sem janela para evitar exibi��o extra do CMD
        };

        // Iniciando o processo do Python
        Process process = new Process
        {
            StartInfo = psi
        };

        // Iniciar o processo e os redirecionamentos de sa�da
        process.Start();
        StreamReader standardOutputReader = process.StandardOutput;
        StreamReader standardErrorReader = process.StandardError;

        // Ler a sa�da do processo Python (sa�da padr�o e sa�da de erro) ap�s a conclus�o
        string output = standardOutputReader.ReadToEnd();
        string errorOutput = standardErrorReader.ReadToEnd();

        process.WaitForExit();

        // Exibindo a sa�da e sa�da de erro
        Debug.Log("Sa�da do Python: \n" + output);
        Debug.Log("Sa�da de erro do Python: \n" + errorOutput);

        List<LogicMap> logicMapList = new List<LogicMap>();
        if (output.Contains("FULL PATH"))
        {
            string listaString = output.Split(':').Last();
            Debug.Log(listaString);

            // Remover os colchetes e espa�os para obter apenas as coordenadas
            string coordinatesString = listaString.Replace("[", "").Replace("]", "").Replace(" ", "");

            // Usar express�o regular para extrair os n�meros de cada coordenada
            Regex regex = new Regex(@"\((\d+),(\d+)\)");
            MatchCollection matches = regex.Matches(coordinatesString);

            // Converter cada par de coordenadas em um Vector3Int e adicion�-lo � lista
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
}
