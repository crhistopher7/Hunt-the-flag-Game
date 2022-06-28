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

    void Start()
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
    }

    /// <summary>
    /// Função que inicializa os agentes de player de acordo com a quantidade de agentes setada em posições randomicas
    /// </summary>
    public void StartAgents()
    {
        Agents.Clear();

        GameObject prefab = Resources.Load("Prefabs/Agent") as GameObject;
        GameObject go;
        string name = "Agent";
        Material material = (CompareTag(Constants.TAG_TEAM_1)) ? 
            Resources.Load(Constants.MATERIAL_AGENT_TEAM_1, typeof(Material)) as Material 
            : Resources.Load(Constants.MATERIAL_AGENT_TEAM_2, typeof(Material)) as Material;

        int i = 1;
        do
        {
            go = Instantiate(prefab);
            AgentController agent = go.GetComponent<AgentController>();
            agent.name = name + i.ToString();
            agent.tag = this.tag;
            agent.GetComponent<Renderer>().sharedMaterial = material;
            agent.transform.SetParent(this.transform);
            agent.transform.localScale = new Vector3(Constants.AGENT_SCALE, Constants.AGENT_SCALE, 1);
            agent.InitPosition(prng.Next());
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
    public void ReceiveMove(string name, Vector3Int objectivePosition, Vector3Int deceptivePosition, PathType pathType)
    {
        foreach (AgentController agent in Agents)
            if (agent.name == name)
            {
                agent.BuildPath(objectivePosition, deceptivePosition, pathType);
                return;
            }
    }
}
