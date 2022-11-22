using System;
using UnityEngine;
/// <summary>
/// Classe utilizada como função de similaridade local na qual retorna a similaridade de duas matrizes.
/// </summary>
public class CosineSimilarity : AbstractLocalSimilarity
{

   
    /// <summary>
    /// Construtor da classe MatrixSimilarity.
    /// </summary>
    public CosineSimilarity()
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

        var A = CBDPUtils.StringToMatrix(value1);
        var B = CBDPUtils.StringToMatrix(value2);

        if (A.GetLength(0) != B.GetLength(0) || A.GetLength(1) != B.GetLength(1))
            return 0;

        var vectorA = CBDPUtils.ToQualitative(CBDPUtils.Flatten(A));
        var vectorB = CBDPUtils.ToQualitative(CBDPUtils.Flatten(B));

        double divd = 0;
        double divsA = 0;
        double divsB = 0;

        // Se for de angulo e distancia  
        if (vectorA[0].angle != null)
        {
            // Cos(A, B) = A.B / || A || * || B ||
            for (int i = 0; i < vectorA.Count; i++)
            {
                Vector2 vA = CBDPUtils.PointByDistanceAndAngle((double)vectorA[i].angle, (double)vectorA[i].numericDistance, Vector2.zero);
                Vector2 vB = CBDPUtils.PointByDistanceAndAngle((double)vectorB[i].angle, (double)vectorB[i].numericDistance, Vector2.zero);

                divd += vA.x * vB.x + vA.y * vB.y; //Dividendo
                divsA += Math.Pow(vA.x, 2) + Math.Pow(vA.y, 2); //Divisor parte ||A||
                divsB += Math.Pow(vB.x, 2) + Math.Pow(vB.y, 2); //Divisor parte ||B||
            }
        }
        else
            return 0;

        float similarity = (float) (divd / Math.Sqrt(divsA) * Math.Sqrt(divsB));
        return similarity;
	}

    

    

    
}