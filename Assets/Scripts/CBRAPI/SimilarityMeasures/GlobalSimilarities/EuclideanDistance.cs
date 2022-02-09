using System;
using System.Collections.Generic;

/// <summary>
/// Classe que utiliza da função da Distância Euclidiana para calcular a similaridade global entre dois casos.
/// </summary>
public class EuclideanDistance : AbstractGlobalSimilarity {

	/// <summary>
	/// Construtor da classe EuclideanDistance.
	/// </summary>
	/// <param name="consultStructure">Estrutura de consulta que será utilizada na análise de similaridade.</param>
	public EuclideanDistance(ConsultStructure consultStructure) : base(consultStructure)
	{

	}

	/// <summary>
	/// Método que retorna o valor (entre 0 e 1) de similaridade entre dois casos utilizando a função da Distância Euclidiana.
	/// </summary>
	/// <param name="searchCase">Caso utilizado como consulta.</param>
	/// <param name="retrieveCase">Casos recuperado da base de casos.</param>
	/// <returns>Valor (entre 0 e 1) de similaridade entre dois casos</returns>
	public override float GetSimilarity(Case searchCase, Case retrieveCase)
	{
		float similarity = 0f;
		float sumWeights = 0f;

		for (int i = 0; i < base.consultStructure.consultParams.Count; i++)
		{
			ConsultParams consultParams = base.consultStructure.consultParams[i];

			if (consultParams.localSimilarity != null && !consultParams.HasBlankFeature(searchCase))
			{
				similarity += (float)Math.Pow(consultParams.localSimilarity.GetSimilarity(consultParams, searchCase, retrieveCase),2) * consultParams.weight;

				sumWeights += consultParams.weight;
			}
		}

		if (sumWeights == 0f)
			return 0f;

		return (float)Math.Sqrt(similarity/sumWeights);
	}
}



