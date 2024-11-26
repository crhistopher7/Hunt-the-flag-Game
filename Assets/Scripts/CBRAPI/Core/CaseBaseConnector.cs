using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;

/// <summary>
/// Classe resposável por fazer a conexão entre a biblioteca CBR e a base de casos.
/// </summary>
[Serializable]
public class CaseBaseConnector  {

	/// <summary>
	/// Lista de casos presentes na base de casos.
	/// </summary>
	public List<Case> caseBase;

	/// <summary>
	/// Diretório em que os arquivos que representam a base de casos são armazenados.
	/// </summary>
	private string filePath = ""; //Application.streamingAssetsPath + "/CaseBase/";

	/// <summary>
	/// Nome do arquivo que representa a base de casos.
	/// </summary>
	private string fileName = "";

	/// <summary>
	/// Construtor da classe CaseBaseConnector.
	/// </summary>
	/// <param name="fileName">Nome do arquivo que representa a base de casos.</param>
	public CaseBaseConnector(string fileName)
	{
		caseBase = new List<Case>();
		this.fileName = fileName;
	}

	/// <summary>
	/// Método que carrega todos os casos da base de casos na memória RAM.
	/// </summary>
	public void LoadCaseBase()
	{
		if (!File.Exists(Path.Combine(filePath, fileName)))
		{
			Debug.LogError("OpenCaseBase: Base de casos " + filePath + fileName + " nao existe");
			return;
		}
		else
		{
			caseBase = ConvertJSONToClass<CaseBaseConnector>(filePath, fileName).caseBase;
		}
	}

	/// <summary>
	/// Método que cria a base de casos.
	/// </summary>
	public void CreateCaseBase()
	{
		if (!File.Exists(Path.Combine(filePath, fileName)))
		{
			SaveCaseBase();
		}
	}

	/// <summary>
	/// Método que salva a base de casos da memória RAM no arquivo da base de casos.
	/// </summary>
	private void SaveCaseBase()
	{
		ConvertObjectToJSON(filePath, fileName, this);
	}

	/// <summary>
	/// Método que carrega um caso da base de casos.
	/// </summary>
	/// <param name="caseId">Identificador do caso a ser carregado.</param>
	/// <returns></returns>
	public Case LoadCase(int caseId)
	{
		return caseBase[caseId];
	}

	/// <summary>
	/// Método que adiciona um novo caso na base de casos.
	/// </summary>
	/// <param name="newCase">Novo caso a ser adicionado na base de casos.</param>
	public void AddCase(Case newCase)
	{
		caseBase.Add(newCase);
		SaveCaseBase();
	}

	/// <summary>
	/// Método que remove um caso da base de casos.
	/// </summary>
	/// <param name="caseId">Identificador do caso a ser removido da base de casos.</param>
	public void RemoveCase(int caseId)
	{
		foreach (Case c in caseBase)
		{
			if (c.id == caseId)
			{
				caseBase.Remove(c);
				SaveCaseBase();
				break;
			}
		}
	}

	/// <summary>
	/// Método que substitui um caso da base de casos por um novo caso.
	/// </summary>
	/// <param name="caseId">Identificador do caso a ser alterado na base de casos.</param>
	/// <param name="newCase">Novo caso que alterará um caso presente na base de casos.</param>
	public void EditCase(int caseId, Case newCase)
	{
		for(int i = 0; i < caseBase.Count; i++)
		{
			if (caseBase[i].id == caseId)
			{
				caseBase[i] = newCase;
				SaveCaseBase();
				break;
			}
		}
	}

	/// <summary>
	/// Método que transforma um objeto em um arquivo JSON.
	/// </summary>
	/// <param name="filePath">Diretório onde o arquivo JSON será salvo.</param>
	/// <param name="fileName">Nome do arquivo que será salvo.</param>
	/// <param name="obj">Objeto que será transformado em um arquivo JSON.</param>
	private void ConvertObjectToJSON(string filePath, string fileName, object obj)
	{
		try
		{
			string json = JsonUtility.ToJson(obj, true);

			File.WriteAllText(filePath + fileName, json.ToString());
		}
		catch (Exception)
		{
			Debug.LogError("ConvertObjectToJSON: Arquivo " + fileName + " nao pode ser criado");
		}
	}

	/// <summary>
	/// Método que transforma um arquivo JSON em uma classe.
	/// </summary>
	/// <typeparam name="T">Tipo da classe que o arquivo JSON será transformado.</typeparam>
	/// <param name="filePath">Diretório do arquivo que será lido.</param>
	/// <param name="fileName">Nome do arquivo que será lido.</param>
	/// <returns>Retorna a classe criada a partir do arquivo JSON.</returns>
	private T ConvertJSONToClass<T>(string filePath, string fileName)
	{
		T returnedCase = default(T);
		try
		{
			string json = ReadToEnd(filePath, fileName);

			returnedCase = JsonUtility.FromJson<T>(json);
		}
		catch (Exception)
		{
			Debug.LogError("ConvertJSONToClass: Nao foi possivel criar a classe");
			return default(T);
		}
		return returnedCase;

	}

	/// <summary>
	/// Método que lê um arquivo de texto até o fim.
	/// </summary>
	/// <param name="filePath">Diretório do arquivo.</param>
	/// <param name="fileName">Nome do arquivo.</param>
	/// <returns>String com os dados lidos do arquivo.</returns>
	private static string ReadToEnd(string filePath, string fileName)
	{
		string fileStr = "";
		try
		{
			using (StreamReader sr = new StreamReader(Path.Combine(filePath, fileName)))
			{
				fileStr = sr.ReadToEnd();
			}
		}
		catch (Exception)
		{
			Debug.LogError("ReadToEnd: Nao foi possivel ler o arquivo " + filePath + fileName);
			return null;
		}
		return fileStr;
	}
}
