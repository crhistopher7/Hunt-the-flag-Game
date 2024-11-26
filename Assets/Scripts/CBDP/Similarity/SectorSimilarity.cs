using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Classe utilizada como função de similaridade local na qual retorna a similaridade de dois vetores de setor.
/// </summary>
public class SectorSimilarity : AbstractLocalSimilarity
{
    /// <summary>
    /// Construtor da classe SectorSimilarity.
    /// </summary>
    public SectorSimilarity()
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

        var A = StringToVecSector(value1);
        var B = StringToVecSector(value2);

        int[] count_A = { 0, 0, 0 };
        int[] count_B = { 0, 0, 0 };

        // Ter a quantidade em cada setor
        for (int i = 0; i < A.Length; i++)
        {
            count_A[(int)A[i]]++;
            count_B[(int)B[i]]++;
        }

        int count = 0;
        for (int i = 0; i < count_A.Length; i++)
        {
            count += Math.Abs(count_A[i] - count_B[i]);
        }
        float distance_sector = count / A.Length;

        // ter a distância de similaridade entre os vetores, vendo se são iguais
        count = 0;
        for (int i = 0; i < A.Length; i++) //Usando hamming
            if (A[i] != B[i])
                count++;
        float distance_hamming = count / A.Length;

        //Usando similaridade criada por distância dos setores
        float count_qualitative = 0;
        for (int i = 0; i < A.Length; i++)
        {
            if (A[i] == B[i])
                continue;
            else if (A[i].Equals(Sector.NEUTRAL) || B[i].Equals(Sector.NEUTRAL)) //Um dos dois está no meio, ou seja, mais próximo que completamente do outro lado
                count_qualitative += 0.5f;
            else
                count_qualitative += 1; //Completamente oposta
        }
        float distance_qualitative = count_qualitative / A.Length;


        Debug.Log("Similaridade do Vetor do caso " + retrieveCase.caseDescription[0].value + ": " + (1f - (distance_sector + distance_hamming + distance_qualitative) / 3));

        return 1f - (distance_sector + distance_hamming + distance_qualitative) / 3; //média normal (poderia ser euclidiana)
    }

    private Sector[] StringToVecSector(string str)
    {
        Sector[] vector_sector;

        // Removendo o {} de fora da matriz
        string aux = str.Remove(0, 1);
        aux = aux.Remove(aux.Length - 1, 1);

        // Separando em valores
        var values = aux.Split(',');

        vector_sector = new Sector[values.Length];

        for (int i = 0; i < vector_sector.Length; i++)
        {
            Enum.TryParse(values[i], out Sector sector);
            vector_sector[i] = sector;
        }

        return vector_sector;
    }

}