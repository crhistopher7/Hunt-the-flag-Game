using System.Collections.Generic;
using UnityEngine;

public partial class CaseConstructor
{
    class Case
    {
        public int id;
        public int seedMap;
        public string[,] matrix_agents;
        public string[,] matrix_objetives;
        public string[,] matrix_base;

        public int[,] int_matrix_agents;
        public int[,] int_matrix_objetives;
        public int[,] int_matrix_base;

        public Sector[] vector_sector;

        public TypeCase solutionType;
        public Strategy strategy;
        public bool result;
        public Plan plan;
    }

    public class Plan
    {
        public class Action
        {
            public string action;
            public string agent;
            public string objetive;
            public TypeCase actionDefinition;
            public string distance_direction;
            public double time;

            public override string ToString()
            {
                return action + "," + agent + "," + objetive + "," + actionDefinition + "," + distance_direction + "," + time;
            }
        }

        public int caseid;
        public TypeCase solutionType;
        public Queue<Action> actions;


        public override string ToString()
        {
            string str = "(case_id:" + caseid + "|solutionType:" + solutionType + "|actions:";

            //add cada ação
            while (actions.Count != 0)
            {
                Action action = actions.Dequeue();
                str += "<" + action.ToString() + ">";
                if (actions.Count != 0)
                    str += ",";
            }

            str += ")";
            return str;	                       
        }
    }

}
