﻿using System;
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

        public Plan() { }

        public Plan(string plan)
        {
            //(case_id:1|solutionType:DECEPTIVE|actions:<Move,Agent3,,NORMAL,F,LB,-30,6201883>,<Move,Agent3,,NORMAL,F,LF,-34,3447987>,<Move,Agent3,,NORMAL,F,LF,-38,8167663>,<Move,Agent3,,NORMAL,F,RF,-39,5241194>,<Move,Agent3,,NORMAL,F,LF,-41,01248>,<Move,Agent3,Flag1,NORMAL,F,LF,-46,8955223>)
            
            // removendo os ()
            plan = plan.Remove(0, 1);
            plan = plan.Remove(plan.Length - 1, 1);

            var aux = plan.Split('|');

            this.caseid = int.Parse(aux[0].Split(':')[1]);
            Enum.TryParse(aux[1].Split(':')[1], out this.solutionType);

            // actions
            var actions = aux[2].Split(':')[1].Split(',');

            for (int i = 0; i < actions.Length; i++)
            {
                var action_str = actions[i];

                // removendo os <>
                action_str = action_str.Remove(0, 1);
                action_str = action_str.Remove(action_str.Length - 1, 1);

                var features = action_str.Split(',');

                Action action = new Action();

                action.action = features[0];
                action.agent = features[1];
                action.objetive = features[2];
                Enum.TryParse(features[3], out action.actionDefinition);
                action.distance_direction = features[4];
                action.time = Double.Parse(features[5]);

            }

        }

        public override string ToString()
        {
            string str = "(case_id:" + caseid + "|solutionType:" + solutionType + "|actions:";

            var actions = this.actions;
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
