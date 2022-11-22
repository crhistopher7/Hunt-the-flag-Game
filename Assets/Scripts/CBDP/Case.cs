using Assets.Scripts.CBDP;
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
    
    public Qualitative[,] matrix_agents;
    public Qualitative[,] matrix_objetives;
    public Qualitative[,] matrix_agents_distance_angle;

    public string[,] matrix_agents_;
    public string[,] matrix_objetives_;
    public string[,] matrix_agents_distance_angle_;

    //public int[,] int_matrix_agents;
    //public int[,] int_matrix_objetives;
    //public int[,] int_matrix_agents_distance_angle;

    public Sector[] vector_sector;

    public DeceptiveLevel solutionType;
    public Strategy strategy;
    public bool result;
    public string description;
    public List<string> tags;
    public Plan plan;
    
    /// <summary>
    /// Override do ToString
    /// </summary>
    /// <returns>Retorna uma string única contendo todas as variáveis separadas por ','</returns>
    public override string ToString()
    {
        string str = "";

        str += caseId.ToString() + Constants.SPLITTER;
        str += seedMap.ToString() + Constants.SPLITTER;
        str += CBDPUtils.ToMatrixString(matrix_agents) + Constants.SPLITTER;
        str += CBDPUtils.ToMatrixString(matrix_objetives) + Constants.SPLITTER;
        str += CBDPUtils.ToMatrixString(matrix_agents_distance_angle, true) + Constants.SPLITTER;
        //str += CBDPUtils.ToMatrixString(int_matrix_agents) + Constants.SPLITTER;
        //str += CBDPUtils.ToMatrixString(int_matrix_objetives) + Constants.SPLITTER;
        str += ToVectorString(vector_sector) + Constants.SPLITTER;
        str += solutionType.ToString() + Constants.SPLITTER;
        str += strategy.ToString() + Constants.SPLITTER;
        str += description + Constants.SPLITTER;
        str += result.ToString() + Constants.SPLITTER;
        str += plan.ToString();

        return str;
    }

    /// <summary>
    /// Função que reescreve um vetor do tipo Sector em forma de string
    /// </summary>
    /// <param name="vector">Vetor do tipo Sector</param>
    /// /// <param name="delimiter">Delimitador</param>
    /// <returns>Vetor no formato de string</returns>
    private string ToVectorString(Sector[] vector, string delimiter = ",")
    {
        string str = "{";
        for (int i = 0; i < vector.Length; i++)
        {
            str += vector[i].ToString();
            if (i != vector.Length - 1)
                str += delimiter;
        }
        str += "}";
        return str;
    }
}