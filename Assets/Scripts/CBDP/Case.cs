using System.Collections.Generic;
using UnityEngine;

public partial class CaseConstructor
{
    public class Case
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

        public override string ToString()
        {
            string str = "";
            char splitter = ';';

            str += id.ToString() + splitter;
            str += seedMap.ToString() + splitter;


            str += ToMatrixString(matrix_agents) + splitter;

            str += ToMatrixString(matrix_objetives) + splitter;

            //str += ToMatrixString(matrix_base) + splitter;

            str += ToMatrixString(int_matrix_agents) + splitter;

            str += ToMatrixString(int_matrix_objetives) + splitter;

            //str += ToMatrixString(int_matrix_base) + splitter;

            str += "{";
            for (int i = 0; i < vector_sector.Length; i++)
            {
                str += vector_sector[i].ToString();
                if (i != vector_sector.Length - 1)
                    str += ",";
            }
            str += "}" + splitter;

            str += solutionType.ToString() + splitter;
            str += strategy.ToString() + splitter;
            str += result.ToString() + splitter;
            str += plan.ToString();

            return str;
        }

        public string ToMatrixString(string[,] matrix, string delimiter = ",")
        {
            string s = "{";

            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                s += "{";
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    s += matrix[i, j].ToString() + (delimiter);
                }
                s = s.Remove(s.Length - 1, 1);
                s += "}:";
            }
            s = s.Remove(s.Length - 1, 1);

            return s += "}";
        }

        public string ToMatrixString(int[,] matrix, string delimiter = ",")
        {
            string s = "{";

            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                s += "{";
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    s += matrix[i, j].ToString() + (delimiter);
                }
                s = s.Remove(s.Length - 1, 1);
                s += "}:";
            }
            s = s.Remove(s.Length - 1, 1);

            return s += "}";
        }
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
