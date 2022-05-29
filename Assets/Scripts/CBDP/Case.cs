using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CBDP Case class
/// </summary>
public class CaseCBDP
{
    public int caseId;
    public int seedMap;
    public string[,] matrix_agents;
    public string[,] matrix_objetives;
    public string[,] matrix_agents_distance_angle;

    public int[,] int_matrix_agents;
    public int[,] int_matrix_objetives;
    public int[,] int_matrix_agents_distance_angle;

    public Sector[] vector_sector;

    public DeceptiveLevel solutionType;
    public Strategy strategy;
    public bool result;
    public string description;
    public Plan plan;
    
    /// <summary>
    /// Override do ToString
    /// </summary>
    /// <returns>Retorna uma string única contendo todas as variáveis separadas por ','</returns>
    public override string ToString()
    {
        string str = "";
        char splitter = ';';

        str += caseId.ToString() + splitter;
        str += seedMap.ToString() + splitter;


        str += ToMatrixString(matrix_agents) + splitter;

        str += ToMatrixString(matrix_objetives) + splitter;

        str += ToMatrixString(matrix_agents_distance_angle) + splitter;

        str += ToMatrixString(int_matrix_agents) + splitter;

        str += ToMatrixString(int_matrix_objetives) + splitter;

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
        str += description + splitter;
        str += plan.ToString();

        return str;
    }

    /// <summary>
    /// Função que reescreve uma matriz[,] do tipo string em forma de string
    /// </summary>
    /// <param name="matrix">A matriz[,] do tipo string</param>
    /// <param name="delimiter">Delimitador</param>
    /// <returns>Matrix no formato de string</returns>
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

    /// <summary>
    /// Função que reescreve uma matriz[,] do tipo int em forma de string
    /// </summary>
    /// <param name="matrix">A matriz[,] do tipo int</param>
    /// <param name="delimiter">Delimitador</param>
    /// <returns>Matrix no formato de string</returns>
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