using System.Collections;

/// <summary>
/// Classe abstrata que representa uma medida de similaridade local que será implementada na biblioteca CBR.
/// </summary>
public abstract class AbstractLocalSimilarity {

	/// <summary>
	/// Construtor da classe AbstractLocalSimilarity.
	/// </summary>
	public AbstractLocalSimilarity()
	{
		
	}


	/// <summary>
	/// Método abstrato que deve ser implementado por todas as classes que extenderem a classe AbstractLocalSimilarity.
	/// </summary>
	/// <param name="consultParams">Parâmetros da consulta.</param>
	/// <param name="searchCase">Caso utilizado como consulta.</param>
	/// <param name="retrieveCase">Caso recuperado da base de casos.</param>
	/// <returns></returns>
	public abstract float GetSimilarity(ConsultParams consultParams, Case searchCase, Case retrieveCase);

}
