using UnityEngine;
using System.Collections;

/// <summary>
/// Classe que representa o resultado da consulta realizada na biblioteca CBR.
/// </summary>
public class Result  {

	/// <summary>
	/// Identificador do resultado.
	/// </summary>
	public int id;

	/// <summary>
	/// Caso da base de casos que foi comparado.
	/// </summary>
	public Case matchCase;

	/// <summary>
	/// Porcentagem de similaridade entre o caso de consulta e o caso da base de casos.
	/// </summary>
	public float matchPercentage;


	/// <summary>
	/// Construtor da classe Result.
	/// </summary>
	/// <param name="id">Identificador do resultado.</param>
	/// <param name="matchCase">Caso da base de casos que foi comparado.</param>
	/// <param name="matchPercentage">Porcentagem de similaridade entre o caso de consulta e o caso da base de casos.</param>
	public Result(int id, Case matchCase, float matchPercentage)
	{
		this.id = id;
		this.matchCase = matchCase;
		this.matchPercentage = matchPercentage;
	}
}
