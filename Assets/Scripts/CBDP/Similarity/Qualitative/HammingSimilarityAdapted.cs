using UnityEngine;
/// <summary>
/// Classe utilizada como função de similaridade local na qual retorna a similaridade de duas matrizes.
/// </summary>
public class HammingSimilarityAdapted : AbstractLocalSimilarity
{
    /// <summary>
    /// Construtor da classe MatrixSimilarity.
    /// </summary>
    public HammingSimilarityAdapted()
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

        int count = 0;

        if (A.GetLength(0) != B.GetLength(0) || A.GetLength(1) != B.GetLength(1))
            return 0;

        var vectorA = CBDPUtils.ToQualitative(CBDPUtils.Flatten(A));
        var vectorB = CBDPUtils.ToQualitative(CBDPUtils.Flatten(B));


        for (int i = 0; i < vectorA.Count; i++)
        {
            if (vectorA[i].distance.ToString() != vectorB[i].distance.ToString())
                count++;

            if (vectorA[i].direction.ToString() != vectorB[i].direction.ToString())
                count++;
        }

        float total_size = vectorA.Count * 2;
        float distance = count / total_size;

        Debug.Log("Similaridade da Hamming Adapted id " + consultParams.indexes[0] + " entre caso " + searchCase.caseDescription[0].value + " e caso " + retrieveCase.caseDescription[0].value + ": " + ((1f - distance) * 100).ToString("0.00"));

        return 1f - distance;
	}
}