using System;
using UnityEngine;
/// <summary>
/// Classe utilizada como função de similaridade local na qual retorna a similaridade de duas matrizes.
/// </summary>
public class CanberraSimilarity : AbstractLocalSimilarity
{

   
    /// <summary>
    /// Construtor da classe MatrixSimilarity.
    /// </summary>
    public CanberraSimilarity()
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

        float similarityX = 0;
        float similarityY = 0;
        int zX = 0;
        int zY = 0;
        int n = vectorA.Count;

        // Se for de angulo e distancia  
        if (vectorA[0].angle != null)
        {
            // Can(A, B) = Sum |A - B| / |A| + |B| 
            for (int i = 0; i < vectorA.Count; i++)
            {
                Vector2 vA = CBDPUtils.PointByDistanceAndAngle((double)vectorA[i].angle, (double)vectorA[i].numericDistance, Vector2.zero);
                Vector2 vB = CBDPUtils.PointByDistanceAndAngle((double)vectorB[i].angle, (double)vectorB[i].numericDistance, Vector2.zero);

                similarityX += Math.Abs(vA.x - vB.x) / (Math.Abs(vA.x) + Math.Abs(vB.x)); // X
                similarityY += Math.Abs(vA.y - vB.y) / (Math.Abs(vA.y) + Math.Abs(vB.y)); // Y

                if (vA.x == 0) zX++;
                if (vA.y == 0) zY++;
                if (vB.x == 0) zX++;
                if (vB.y == 0) zY++;

            }
        }
        float similarity = ((similarityX / n - zX) + (similarityY / n - zY));
        Debug.Log("Similaridade da canberra id " + consultParams.indexes[0] + " entre caso " + searchCase.caseDescription[0].value + " e caso " + retrieveCase.caseDescription[0].value + ": " + (Math.Abs(similarity - 1f) * 100).ToString("0.00"));

        return Math.Abs(similarity - 1f);
	}

    

    

    
}