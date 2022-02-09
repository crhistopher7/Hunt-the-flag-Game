using System.Collections.Generic;

/// <summary>
/// Classe que deve ser instanciada para ser possível utilizar a biblioteca CBR.
/// </summary>
public class CBRAPI
{
	/// <summary>
	/// Responsável por fazer a conexão entre a aplicação e a base de casos.
	/// </summary>
	public CaseBaseConnector caseBaseConnector;


	/// <summary>
	/// Construtor da classe CBRAPI.
	/// </summary>
	public CBRAPI()
	{
		caseBaseConnector = new CaseBaseConnector("CaseBase.json");
		caseBaseConnector.CreateCaseBase();
		caseBaseConnector.LoadCaseBase();
	}

	/// <summary>
	/// Construtor da classe CBRAPI.
	/// </summary>
	/// <param name="fileName">Nome do arquivo que represeta a base de casos.</param>
	public CBRAPI(string fileName)
	{
		caseBaseConnector = new CaseBaseConnector(fileName);
		caseBaseConnector.CreateCaseBase();
		caseBaseConnector.LoadCaseBase();
	}

	/// <summary>
	/// Método que passa um novo caso para a classe CaseBaseConnector adicionar na base de casos.
	/// </summary>
	/// <param name="newCase">Novo caso.</param>
	public void AddCase(Case newCase)
	{
		caseBaseConnector.AddCase(newCase);
	}

	/// <summary>
	/// Método que informa a classe CaseBaseConnector qual caso deve ser removido.
	/// </summary>
	/// <param name="caseID">Identificador do caso a ser removido.</param>
	public void RemoveCase(int caseID)
	{
		caseBaseConnector.RemoveCase(caseID);
	}

	/// <summary>
	/// Método que informa a classe CaseBaseConnector qual caso deve ser editado.
	/// </summary>
	/// <param name="caseID">Identificador do caso e ser editado.</param>
	/// <param name="newCase">Novo caso que irá substituir um já existente.</param>
	public void EditCase(int caseID, Case newCase)
	{
		caseBaseConnector.EditCase(caseID, newCase);
	}

	/// <summary>
	/// Realiza e etapa de recuperação do ciclo de CBR.
	/// </summary>
	/// <param name="searchCase">Caso de consulta.</param>
	/// <returns></returns>
	public List<Result> Retrieve(Case searchCase, ConsultStructure consultStructure)
	{
		if (consultStructure != null)
		{
			Search cbrSearch = new Search();

			return cbrSearch.DoSearch(searchCase, caseBaseConnector, consultStructure);
		}
		return null;
	}


	/// <summary>
	/// Realiza e etapa de recuperação do ciclo de CBR.
	/// </summary>
	/// <param name="searchCase">Caso de consulta.</param>
	/// <returns></returns>
	public List<Result> Retrieve(Case searchCase, ConsultStructure consultStructure, int maxIndex)
	{
		if (consultStructure != null)
		{
			Search cbrSearch = new Search();

			return cbrSearch.DoSearch(searchCase, caseBaseConnector, consultStructure, maxIndex);
		}
		return null;
	}
}
