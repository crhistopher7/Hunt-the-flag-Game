using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// CBDP Plan class used in Case
/// </summary>
public class Plan
{
    public int caseid;
    public DeceptiveLevel solutionType;
    public Queue<Action> actions;

    public Plan() 
    {
        this.actions = new Queue<Action>();
    }

    public Plan(string plan)
    {
        plan = plan.Remove(0, 1);
        plan = plan.Remove(plan.Length - 1, 1);

        var aux = plan.Split('|');

        this.caseid = int.Parse(aux[0].Split(':')[1]);
        Enum.TryParse(aux[1].Split(':')[1], out this.solutionType);

        // actions
        var actions_str = aux[2].Split(':')[1];
        actions_str = actions_str.Remove(actions_str.Length - 1, 1);

        actions_str = actions_str.Replace(">,", ">");
        var actions = actions_str.Split('>');

        this.actions = new Queue<Action>();

        for (int i = 0; i < actions.Length; i++)
        {
            var action_str = actions[i];

            action_str = action_str.Replace("<", "");

            var features = action_str.Split(',');

            Action action = new Action();

            action.action = features[0];
            action.agent = features[1];
            action.objetive = features[2];
            Enum.TryParse(features[3], out action.actionDefinition);
            Enum.TryParse(features[4], out action.pathType);
            action.distance_direction = features[5];
            action.distance_directionDeceptive = features[6];
            action.time = int.Parse(features[7]);

            this.actions.Enqueue(action);
        }

    }

    public override string ToString()
    {
        string str = "(case_id:" + caseid + "|solutionType:" + solutionType + "|actions:";

        Debug.Log("tamanho da fila de ações: " + this.actions.Count);

        foreach (Action action in this.actions)
        {
            str += "<" + action.ToString() + ">,";
        }

        str += str.Remove(str.Length - 1, 1) + ")";
        return str;
    }

}
