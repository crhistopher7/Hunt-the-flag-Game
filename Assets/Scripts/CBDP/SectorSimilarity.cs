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
        float similarity;

        int countDefensive_A = 0;
        int countNeutral_A = 0;
        int countOfensive_A = 0;

        int countDefensive_B = 0;
        int countNeutral_B = 0;
        int countOfensive_B = 0;

        var A = StringToVecSector(value1);
        var B = StringToVecSector(value2);

        for (int i = 0; i < A.Length; i++)
        {
            if (A[i].Equals(Sector.DEFENSIVE))
                countDefensive_A++;
            else if (A[i].Equals(Sector.NEUTRAL))
                countNeutral_A++;
            else
                countOfensive_A++;

            if (B[i].Equals(Sector.DEFENSIVE))
                countDefensive_B++;
            else if (B[i].Equals(Sector.NEUTRAL))
                countNeutral_B++;
            else
                countOfensive_B++;
        }

        int diff = Math.Abs(countOfensive_A - countOfensive_B);
        diff += Math.Abs(countDefensive_A - countDefensive_B); 
        diff += Math.Abs(countNeutral_A - countNeutral_B);

        similarity = diff / A.Length;

        Debug.Log("Similaridade do Vetor do caso " + retrieveCase.caseDescription[0].value + ": " + (1f - similarity));

        return 1f - similarity;
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