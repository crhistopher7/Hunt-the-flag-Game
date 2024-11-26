using System;
using UnityEngine;

/// <summary>
/// Classe utilizada como função de similaridade local na qual compara se dois atributos são iguais.
/// </summary>
public class Equals : AbstractLocalSimilarity
{

	/// <summary>
	/// Construtor da classe Equals.
	/// </summary>
	public Equals()
	{

	}

	/// <summary>
	/// Método que retorna o valor de similaridade entre duas strings.
	/// </summary>
	/// <param name="consultParams">Parâmetros da consulta.</param>
	/// <param name="searchCase">Caso utilizado como consulta.</param>
	/// <param name="retriveCase">Caso recuperado da base de casos.</param>
	/// <returns>Valor de similaridade entre duas strings.</returns>
	public override float GetSimilarity(ConsultParams consultParams, Case searchCase, Case retrieveCase)
	{
		string value1 = searchCase.caseDescription[consultParams.indexes[0]].value;
		string value2 = retrieveCase.caseDescription[consultParams.indexes[0]].value;

		if (value1 == value2)
        {
			Debug.Log("Similaridade Iguals id " + consultParams.indexes[0] + " do caso " + retrieveCase.caseDescription[0].value + ": 1");
			return 1f;
		}
        else
        {
			Debug.Log("Similaridade Iguals id " + consultParams.indexes[0] + " do caso " + retrieveCase.caseDescription[0].value + ": 0");
			return 0f;
		}
	}
}
