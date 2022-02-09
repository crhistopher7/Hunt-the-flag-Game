using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Classe que contém a estrutura de consulta que será utilizada na análise de similaridade entre os casos.
/// </summary>
public class ConsultStructure  {

	/// <summary>
	/// Função de similaridade global que será utilizada na análise de similaridade de dois casos.
	/// </summary>
	public AbstractGlobalSimilarity globalSimilarity;

	/// <summary>
	/// Lista de todos os parâmetros de consulta utilizados na análise de similaridade.
	/// </summary>
	public List<ConsultParams> consultParams;

	/// <summary>
	/// Construtor da classe ConsultStructure.
	/// </summary>
	public ConsultStructure()
	{
		consultParams = new List<ConsultParams>();
		globalSimilarity = new EuclideanDistance(this);
	}

	/// <summary>
	/// Construtor da classe ConsultStructure.
	/// </summary>
	public ConsultStructure(AbstractGlobalSimilarity globalSimilarity)
	{
		consultParams = new List<ConsultParams>();
		this.globalSimilarity = globalSimilarity;
	}
}

