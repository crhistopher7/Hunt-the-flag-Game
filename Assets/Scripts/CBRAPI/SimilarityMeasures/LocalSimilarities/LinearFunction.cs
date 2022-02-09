using System;
using System.Collections.Generic;

/// <summary>
/// Classe que utilizar uma função linear para calcular a similaridade local entre dois atributos do caso.
/// </summary>
public class LinearFunction : AbstractLocalSimilarity
{
	/// <summary>
	/// Valor mínimo do domínio do atributo.
	/// </summary>
	float minVal;

	/// <summary>
	/// Valor máximo do domínio do atributo.
	/// </summary>
	float maxVal;

	/// <summary>
	/// Construtor da classe LinearFunction.
	/// </summary>
	/// <param name="minVal">Valor mínimo do domínio do atributo.</param>
	/// <param name="maxVal">Valor máximo do domínio do atributo.</param>
	public LinearFunction(float minVal, float maxVal)
	{
		this.minVal = minVal;
		this.maxVal = maxVal;
	}

	/// <summary>
	/// Método que retorna o valor de similaridade entre dois números.
	/// </summary>
	/// <param name="consultParams">Parâmetros da consulta.</param>
	/// <param name="searchCase">Caso utilizado como consulta.</param>
	/// <param name="retriveCase">Caso recuperado da base de casos.</param>
	/// <returns>Valor de similaridade entre dois números.</returns>
	public override float GetSimilarity(ConsultParams consultParams, Case searchCase, Case retriveCase)
	{
		float num1 = float.Parse(searchCase.caseDescription[consultParams.indexes[0]].value);
		float num2 = float.Parse(retriveCase.caseDescription[consultParams.indexes[0]].value);

		return 1f - (Math.Abs(Convert.ToSingle(num1) - Convert.ToSingle(num2)) / (maxVal - minVal));
	}
}





