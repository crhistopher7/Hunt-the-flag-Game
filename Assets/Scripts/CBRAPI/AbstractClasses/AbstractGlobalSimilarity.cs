
/// <summary>
/// Classe abstrata que representa uma medida de similaridade global que será implementada na biblioteca CBR.
/// </summary>
public abstract class AbstractGlobalSimilarity
{
	/// <summary>
	/// Estrutura da consulta que será informada por cada classe que extender a classe AbstractGlobalSimilarity.
	/// </summary>
	public ConsultStructure consultStructure;

	/// <summary>
	/// Construtor da classe AbstractGlobalSimilarity.
	/// </summary>
	/// <param name="consultStructure">Estrutura da consulta.</param>
	public AbstractGlobalSimilarity(ConsultStructure consultStructure)
	{
		this.consultStructure = consultStructure;
	}

	/// <summary>
	/// Método abstrato que deve ser implementado por todas as classes que extenderem a classe AbstractGlobalSimilarity.
	/// </summary>
	/// <param name="searchCase">Caso utilizado como consulta.</param>
	/// <param name="retrieveCase">Caso recuperado da base de casos.</param>
	/// <returns>Valor de similardade entre os casos c1 e c2.</returns>
	public abstract float GetSimilarity(Case searchCase, Case retrieveCase);
}
