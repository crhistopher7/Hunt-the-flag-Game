using System;
using System.Collections.Generic;

/// <summary>
/// Classe que representa um caso que compõem a base de casos. Composta por um identificador do caso e sua descrição do problema e solução.
/// </summary>
[Serializable]
public class Case {

	/// <summary>
	/// Identificador do caso.
	/// </summary>
	public int id;

	/// <summary>
	/// Descrição do problema a ser resolvido.
	/// </summary>
	public List<CaseFeature> caseDescription;

	/// <summary>
	/// Descrição da solução utilizada para resolver o problema.
	/// </summary>
	public List<CaseFeature> caseSolution;

	/// <summary>
	/// Construtor da classe Case.
	/// </summary>
	public Case()
	{
		caseDescription = new List<CaseFeature>();
		caseSolution = new List<CaseFeature>();
	}
}
