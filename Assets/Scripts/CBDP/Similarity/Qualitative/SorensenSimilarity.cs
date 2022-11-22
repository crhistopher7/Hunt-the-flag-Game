﻿/// <summary>
/// Classe utilizada como função de similaridade local na qual retorna a similaridade de duas matrizes.
/// </summary>
public class SorensenSimilarity : AbstractLocalSimilarity
{
    /// <summary>
    /// Construtor da classe MatrixSimilarity.
    /// </summary>
    public SorensenSimilarity()
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

        var vectorA = CBDPUtils.ToQualitative(CBDPUtils.Flatten(CBDPUtils.StringToMatrix(value1)));
        var vectorB = CBDPUtils.ToQualitative(CBDPUtils.Flatten(CBDPUtils.StringToMatrix(value2)));
        var intersection = CBDPUtils.Intersection(vectorA, vectorB);

        float similarity = (2 * intersection.Count) / (vectorA.Count + vectorB.Count); // 2 * |A ∩ B| / |A| + |B|

		//Debug.Log("Similaridade da Matriz id " + consultParams.indexes[0] + " do caso " + retrieveCase.caseDescription[0].value + ": " + (1f - similarity));
		return similarity;
	}
}