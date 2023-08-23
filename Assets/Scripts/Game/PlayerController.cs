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
    /// Função que seta valores iniciais em variáveis
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
            throw new Exception("Não encontrado o objetivo real no player controller");
    }

    public Vector3Int GetDeceptiveGoalPosition()
    {
        if (deceptiveTransform != null)
            return Vector3Int.FloorToInt(new Vector3(deceptiveTransform.position.x, deceptiveTransform.position.y, 0));
        else
            throw new Exception("Não encontrado o objetivo enganoso no player controller");
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
    /// Função que inicializa os agentes de player de acordo com a quantidade de agentes setada em posições randomicas
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
    /// Função que recebe o movimento que um determinado agente deve executar
    /// </summary>
    /// <param name="name">Nome do agente</param>
    /// <param name="objectivePosition">Posição do objetivo</param>
    /// <param name="deceptivePosition">Posição do objetivo enganoso</param>
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
}
