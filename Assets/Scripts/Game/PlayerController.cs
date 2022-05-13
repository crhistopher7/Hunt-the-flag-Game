using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public List<AgentController> Agents;
    public List<AgentController> ClickedAgents;
    public int Points;
    public Text PointsPainel;
    System.Random prng;

    void Start()
    {
        SetVariables();
        StartAgents();
    }

    private void SetVariables()
    {
        Agents = new List<AgentController>();
        prng = new System.Random();
    }

    public void StartAgents()
    {
        Agents.Clear();

        GameObject prefab = Resources.Load("Prefabs/Agent") as GameObject;
        GameObject go;
        string name = "Agent";
        Color color = (CompareTag("Team1")) ? Color.blue : Color.red;

        int i = 1;
        do
        {
            go = Instantiate(prefab);
            AgentController agent = go.GetComponent<AgentController>();
            agent.name = name + i.ToString();
            agent.tag = this.tag;
            agent.GetComponent<Renderer>().material.color = color;
            agent.transform.parent = this.transform;
            agent.InitPosition(prng.Next());
            this.Agents.Add(agent);
            i++;
        } while (i <= Config.NUMBER_OF_AGENTS);
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
