using System;


/// <summary>
/// Classe abstrata que representa a estrutura básica de um atributo.
/// </summary>
[Serializable]
public abstract class AbstractFeature {

	/// <summary>
	/// Identificador do atributo.
	/// </summary>
	public int id;

	/// <summary>
	/// Nome do atributo.
	/// </summary>
	public string name;

	/// <summary>
	/// Tipo de dado do atributo.
	/// </summary>
	public string type;

	/// <summary>
	/// Construtor da classe AbstractFeature.
	/// </summary>
	/// <param name="id">Identificador do novo atributo.</param>
	/// <param name="name">Nome do novo atributo.</param>
	/// <param name="type">Tipo de dado do novo atributo.</param>
	public AbstractFeature(int id, string name, Type type)
	{
		this.id = id;
		this.name = name;
		this.type = type.ToString();
	}
}
