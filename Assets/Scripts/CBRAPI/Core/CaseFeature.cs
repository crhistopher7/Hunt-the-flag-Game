using System;

/// <summary>
/// Classe que representa um atributo presente na descrição do problema ou solução de um caso.
/// </summary>
[Serializable]
public class CaseFeature : AbstractFeature {

	/// <summary>
	/// Valor do atributo.
	/// </summary>
	public string value;

	/// <summary>
	/// Construtor da classe CaseFeature.
	/// </summary>
	/// <param name="id">Identificador do atributo.</param>
	/// <param name="name">Nome do atributo.</param>
	/// <param name="type">Tipo de dado do atributo.</param>
	/// <param name="value">Valor do atributo.</param>
	public CaseFeature(int id, string name, Type type, object value) : base (id, name, type)
	{
		this.value = value.ToString();
	}

	/// <summary>
	/// Construtor da classe CaseFeature.
	/// </summary>
	/// <param name="id">Identificador do atributo.</param>
	/// <param name="name">Nome do atributo.</param>
	/// <param name="type">Tipo de dado do atributo.</param>
	public CaseFeature(int id, string name, Type type) : base(id, name, type)
	{
		this.value = "";
	}
}
