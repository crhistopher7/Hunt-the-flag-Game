using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Classe utilizada como função de similaridade local na qual retorna a similaridade de duas matrizes.
/// </summary>
public class MatrixSimilarity : AbstractLocalSimilarity
{
    Dictionary<string, float> DicDistance = new Dictionary<string, float>()
    {
        {"VC-VC", 0f},
        {"VC-C", 0.25f},
        {"VC-A", 0.5f},
        {"VC-F", 0.75f},
        {"VC-VF", 1f},
        {"C-VC", 0.25f},
        {"C-C", 0.0f},
        {"C-A", 0.25f},
        {"C-F", 0.5f},
        {"C-VF", 0.75f},
        {"A-VC", 0.5f},
        {"A-C", 0.25f},
        {"A-A", 0.0f},
        {"A-F", 0.25f},
        {"A-VF", 0.5f},
        {"F-VC", 0.75f},
        {"F-C", 0.5f},
        {"F-A", 0.25f},
        {"F-F", 0.0f},
        {"F-VF", 0.25f},
        {"VF-VC", 1f},
        {"VF-C", 0.75f},
        {"VF-A", 0.5f},
        {"VF-F", 0.25f},
        {"VF-VF", 0f}
    };

    Dictionary<string, float> DicDirection = new Dictionary<string, float>()
    {
        {"F-F", 0},
        {"F-RF", 0.25f},
        {"F-LF", 0.25f},
        {"F-R", 0.5f},
        {"F-L", 0.5f},
        {"F-B", 1},
        {"F-RB", 0.75f},
        {"F-LB", 0.75f},
        {"RF-F", 0.25f},
        {"RF-RF", 0},
        {"RF-LF", 0.5f},
        {"RF-R", 0.25f},
        {"RF-L", 0.75f},
        {"RF-B", 0.75f},
        {"RF-RB", 0.5f},
        {"RF-LB", 1},
        {"LF-F", 0.25f},
        {"LF-RF", 0.5f},
        {"LF-LF", 0},
        {"LF-R", 0.75f},
        {"LF-L", 0.25f},
        {"LF-B", 0.75f},
        {"LF-RB", 1},
        {"LF-LB", 0.5f},
        {"R-F", 0.5f},
        {"R-RF", 0.25f},
        {"R-LF", 0.75f},
        {"R-R", 0},
        {"R-L", 1},
        {"R-B", 0.5f},
        {"R-RB", 0.25f},
        {"R-LB", 0.75f},
        {"L-F", 0.5f},
        {"L-RF", 0.75f},
        {"L-LF", 0.25f},
        {"L-R", 1},
        {"L-L", 0},
        {"L-B", 0.5f},
        {"L-RB", 0.75f},
        {"L-LB", 0.25f},
        {"B-F", 1},
        {"B-RF", 0.75f},
        {"B-LF", 0.75f},
        {"B-R", 0.5f},
        {"B-L", 0.5f},
        {"B-B", 0},
        {"B-RB", 0.25f},
        {"B-LB", 0.25f},
        {"RB-F", 0.75f},
        {"RB-RF", 0.5f},
        {"RB-LF", 1},
        {"RB-R", 0.25f},
        {"RB-L", 0.75f},
        {"RB-B", 0.25f},
        {"RB-RB", 0},
        {"RB-LB", 0.5f},
        {"LB-F", 0.75f},
        {"LB-RF",1},
        {"LB-LF", 0.5f},
        {"LB-R", 0.75f},
        {"LB-L", 0.25f},
        {"LB-B", 0.25f},
        {"LB-RB", 0.5f},
        {"LB-LB", 0}
    };
    /// <summary>
    /// Construtor da classe MatrixSimilarity.
    /// </summary>
    public MatrixSimilarity()
	{

	}

	/// <summary>
	/// Método que retorna o valor de similaridade entre duas strings.
	/// </summary>
	/// <param name="consultParams">Parâmetros da consulta.</param>
	/// <param name="searchCase">Caso utilizado como consulta.</param>
	/// <param name="retriveCase">Caso recuperado da base de casos.</param>
	/// <returns>Valor de similaridade entre duas strings[,].</returns>
	public override float GetSimilarity(ConsultParams consultParams, Case searchCase, Case retrieveCase)
	{
		string value1 = searchCase.caseDescription[consultParams.indexes[0]].value;
		string value2 = retrieveCase.caseDescription[consultParams.indexes[0]].value;

        var A = StringToMatrix(value1);
        var B = StringToMatrix(value2);

        float similarity = 0f;
        int count = 0;

        for (int i = 0; i < A.GetLength(0); i++)
        {
            for (int j = 0; j < A.GetLength(1); j++)
            {
                if (i <= j)
                {
                    //vazio a parte superior da matrix 
                    continue;
                }

                var a = A[i, j].Split('-');
                var b = B[i, j].Split('-');


                DicDistance.TryGetValue((a[0] + '-' + b[0]).ToString(), out float v1);
                DicDirection.TryGetValue((a[1] + '-' + b[1]).ToString(), out float v2);

                similarity += ((v1 + v2)/2);

                count++;
            }
        }
        similarity /= count;

        //Debug.Log("Similaridade da Matriz id " + consultParams.indexes[0] + " do caso " + retrieveCase.caseDescription[0].value + ": " + (1f - similarity));
    	return 1f - similarity;
	}

    private float GetValue(string a, string b)
    {
        DicDirection.TryGetValue(a, out float v1);
        DicDistance.TryGetValue(b, out float v2);

        return v1 + v2;
    }

    private string[,] StringToMatrix(string str)
    {
        string[,] matrix;

        // Removendo o {} de fora da matriz
        string aux = str.Remove(0, 1);
        aux = aux.Remove(aux.Length - 1, 1);

        // Separando em linhas
        var lines = aux.Split(':');

        // Pegando a quantidade de valores que tem na matrix
        var columnsSize = CountOccurrences(lines[0], ',');

        // Iniciando a matriz
        matrix = new string[lines.Length, columnsSize];

        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            // Exemplo: { VF - RB,A - LB,,,,,,,,,}

            var line = lines[i];

            // Removendo o {}
            line = line.Remove(0, 1);
            line = line.Remove(line.Length - 1, 1);

            var values = line.Split(',');

            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                matrix[i, j] = values[j];
            }
        }

        return matrix;
    }

    private int CountOccurrences(string str, char delimiter)
    {
        int count = 0;
        foreach (char c in str)
            if (c == delimiter) count++;

        return count;
    }
}