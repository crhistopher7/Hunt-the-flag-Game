using System.Collections;
using System;
using System.Collections.Generic;

/// <summary>
/// Classe que contém os parâmetros da consulta que serão utilizados para a análise de similaridade entre dois casos.
/// </summary>
public class ConsultParams {

	/// <summary>
	/// Peso da consulta na análise de similaridade.
	/// </summary>
	public float weight;

	/// <summary>
	/// Função de similaridade local que será utilizada na consulta.
	/// </summary>
	public AbstractLocalSimilarity localSimilarity;

	/// <summary>
	/// Índice dos atributos utilizados na consulta.
	/// </summary>
	public List<int> indexes;

	/// <summary>
	/// Construtor da classe ConsultParams.
	/// </summary>
	/// <param name="indexes">Índice dos atributos utilizados na consulta.</param>
	/// <param name="weight">Peso da consulta na análise de similaridade.</param>
	/// <param name="localSimilarity">Função de similaridade local que será utilizada na consulta.</param>
	public ConsultParams(List<int> indexes, float weight, AbstractLocalSimilarity localSimilarity)
	{
		this.weight = weight;
		this.localSimilarity = localSimilarity;
		this.indexes = indexes;
	}

	/// <summary>
	/// Verifica se algum atributo do caso de consulta está vazio
	/// </summary>
	/// <param name="searchCase">Caso de consulta</param>
	/// <returns>O atributo está vazio ou não?</returns>
	public bool HasBlankFeature(Case searchCase)
	{
		for(int i = 0; i < indexes.Count; i++)
		{
			if (searchCase.caseDescription[indexes[i]].value == "")
				return true;
		}

		return false;
	}
}
