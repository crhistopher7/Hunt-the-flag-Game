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

        double divdx = 0;
        double divsAx = 0;
        double divsBx = 0;

        double divdy = 0;
        double divsAy = 0;
        double divsBy = 0;

        // Se for de angulo e distancia  
        if (vectorA[0].angle != null)
        {
            // Cos(A, B) = A.B / || A || * || B ||
            for (int i = 0; i < vectorA.Count; i++)
            {
                Vector2 vA = CBDPUtils.PointByDistanceAndAngle((double)vectorA[i].angle, (double)vectorA[i].numericDistance, Vector2.zero);
                Vector2 vB = CBDPUtils.PointByDistanceAndAngle((double)vectorB[i].angle, (double)vectorB[i].numericDistance, Vector2.zero);

                divdx += vA.x * vB.x; //Dividendo
                divdy +=  vA.y * vB.y; //Dividendo
                divsAx += Math.Pow(vA.x, 2); //Divisor parte ||A||
                divsAy += Math.Pow(vA.y, 2); //Divisor parte ||A||
                divsBx += Math.Pow(vB.x, 2); //Divisor parte ||B||
                divsBy += Math.Pow(vB.y, 2); //Divisor parte ||B||
            }
        }
        else
            return 0;

        float similarity = (float) (((divdx / Math.Sqrt(divsAx * divsBx)) + (divdy / Math.Sqrt(divsAy * divsBy))) / 2);
        similarity += 1;
        similarity /= 2; //normalize to 0 1
        Debug.Log("Similaridade da Cosine id " + consultParams.indexes[0] + " entre caso " + searchCase.caseDescription[0].value + " e caso " + retrieveCase.caseDescription[0].value + ": " + (similarity * 100).ToString("0.00"));

        return similarity;
	}

    

    

    
}