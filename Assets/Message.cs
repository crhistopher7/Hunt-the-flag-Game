using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Message
{
    private readonly char splitter = '|';
    private readonly char end = '#';

    public LinkedList<string> message = new LinkedList<string>();

    public void AddMessage(string action, string team, AgentController agent, PathType typePath, Vector3Int objective, Vector3Int deceptiveObjetive) 
    {
        string str = action + splitter;
        str += team + splitter;
        str += agent.name + splitter;
        str += typePath + splitter;
        str += objective.x + splitter;
        str += objective.y + splitter;
        str += deceptiveObjetive.x + splitter;
        str += deceptiveObjetive.y;

        message.AddLast(str);
    }

    public override string ToString()
    {
        string finalMessage = "";

        foreach (string s in message)
        {
            finalMessage += s;
            finalMessage += end;
        }

        finalMessage = finalMessage.Remove(finalMessage.Length - 1, 1); // Remove the last 'end'
        return finalMessage;
    }
}
