using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Message
{
    private readonly char splitter = '|';
    private readonly char end = '#';

    public LinkedList<string> message = new LinkedList<string>();

    public void AddMessage(string action, string team, string agent, PathType typePath, Vector3Int objective, Vector3Int deceptiveObjetive) 
    {
        string str = action + splitter;
        str += team + splitter;
        str += agent + splitter;
        str += typePath.ToString() + splitter;
        str += objective.x.ToString() + splitter;
        str += objective.y.ToString() + splitter;
        str += deceptiveObjetive.x.ToString() + splitter;
        str += deceptiveObjetive.y.ToString();

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
