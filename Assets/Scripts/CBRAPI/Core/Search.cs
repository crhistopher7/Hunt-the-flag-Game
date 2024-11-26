using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

/// <summary>
/// Classe responsável por realizar o matching entre o caso de consulta e todos os casos da base de casos.
/// </summary>
public class Search  {

	/// <summary>
	/// Construtor da classe Search.
	/// </summary>
	public Search()
	{

	}

	/// <summary>
	/// Realiza o matching entre o caso de consulta e o caso da base de casos e retorna o resultado desse matching.
	/// </summary>
	/// <param name="searchCase">Caso de consulta.</param>
	/// <param name="caseBaseConnector">Conector da base de casos.</param>
	/// <param name="consultStructure">Estrutura de consulta que será utilizada.</param>
	/// <returns>Lista de Result que contém o resultado de cada matching.</returns>
	public List<Result> DoSearch(Case searchCase, CaseBaseConnector caseBaseConnector, ConsultStructure consultStructure)
	{
		List<Result> matchResults = new List<Result>();

		//for(int i = 0; i < maxCases; i++)
		foreach(Case retrieveCase in caseBaseConnector.caseBase)
		{
			float similarity = consultStructure.globalSimilarity.GetSimilarity(searchCase, retrieveCase);

			Result matchResult = new Result(retrieveCase.id, retrieveCase, similarity);

			matchResults.Add(matchResult);
			
		}
	
		return BubbleSort(matchResults);
	}


	/// <summary>
	/// Realiza o matching entre o caso de consulta e o caso da base de casos e retorna o resultado desse matching.
	/// </summary>
	/// <param name="searchCase">Caso de consulta.</param>
	/// <param name="caseBaseConnector">Conector da base de casos.</param>
	/// <param name="consultStructure">Estrutura de consulta que será utilizada.</param>
	/// <returns>Lista de Result que contém o resultado de cada matching.</returns>
	public List<Result> DoSearch(Case searchCase, CaseBaseConnector caseBaseConnector, ConsultStructure consultStructure, int maxIndex)
	{
		List<Result> matchResults = new List<Result>();

		for(int i = 0; i < maxIndex; i++)
		{
			float similarity = consultStructure.globalSimilarity.GetSimilarity(searchCase, caseBaseConnector.caseBase[i]);

			Result matchResult = new Result(caseBaseConnector.caseBase[i].id, caseBaseConnector.caseBase[i], similarity);

			matchResults.Add(matchResult);

		}

		return BubbleSort(matchResults);
	}

	public List<Result> BubbleSort(List<Result> matchResults)
	{
		int tamanho = matchResults.Count;
		int comparacoes = 0;
		int trocas = 0;

		for (int i = tamanho - 1; i >= 1; i--)
		{
			for (int j = 0; j < i; j++)
			{
				comparacoes++;
				if (matchResults[j].matchPercentage < matchResults[j + 1].matchPercentage)
				{
					Result aux = matchResults[j];
					matchResults[j] = matchResults[j + 1];
					matchResults[j + 1] = aux;
					trocas++;
				}
			}
		}

		return matchResults;
	}


}
