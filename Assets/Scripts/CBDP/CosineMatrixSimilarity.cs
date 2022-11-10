using System;
using System.Collections.Generic;
using UnityEngine;
using AForge.Math.Metrics;
/// <summary>
/// Classe utilizada como função de similaridade local na qual retorna a similaridade de duas matrizes.
/// </summary>
public class CosineMatrixSimilarity : AbstractLocalSimilarity
{

   
    /// <summary>
    /// Construtor da classe MatrixSimilarity.
    /// </summary>
    public CosineMatrixSimilarity()
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

        double[] vecA = new double[vectorA.Count];
        double[] vecB = new double[vectorB.Count];
        // Se for de angulo e distancia  
        if (vectorA[0].angle != null)
        {
            for (int i = 0; i < vectorA.Count; i++)
            {
                double angle = (double)vectorA[i].angle;
                double h = (double)vectorA[i].numericDistance;
                vecA[i] = Math.Sin(angle) * h;

                angle = (double)vectorB[i].angle;
                h = (double)vectorB[i].numericDistance;
                vecB[i] = Math.Sin(angle) * h;
            }

        }
        // Se for de distancia e direção qualitativa
        else
        {
            for (int i = 0; i < vectorA.Count; i++)
            {
                var p = vectorA[i].GetPoint();
                vecA[i] = CBDPUtils.Euclidian(Vector2.zero, p);
                //vecA[i] = CBDPUtils.Octile(Vector2.zero, p);

                p = vectorB[i].GetPoint();
                vecB[i] = CBDPUtils.Euclidian(Vector2.zero, p);
                //vecB[i] = CBDPUtils.Octile(Vector2.zero, p);
            }
        }



        // instantiate new similarity class
        CosineSimilarity sim = new CosineSimilarity();
        float similarityScore = (float) sim.GetSimilarityScore(vecA, vecB);
        return similarityScore;
	}

    

    

    
}