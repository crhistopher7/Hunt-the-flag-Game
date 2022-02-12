using System;

/// <summary>
/// Classe utilizada como função de similaridade local na qual compara se dois atributos são iguais.
/// </summary>
public class MatrixSimilarity : AbstractLocalSimilarity
{

	/// <summary>
	/// Construtor da classe MatrixSimilarity.
	/// </summary>
	public MatrixSimilarity()
	{

	}

	/// <summary>
	/// Método que retorna o valor de similaridade entre duas strings.
	/// </summary>
	/// <param name="consultParams">Parâmetros da consulta.</param>
	/// <param name="searchCase">Caso utilizado como consulta.</param>
	/// <param name="retriveCase">Caso recuperado da base de casos.</param>
	/// <returns>Valor de similaridade entre duas strings[,].</returns>
	public override float GetSimilarity(ConsultParams consultParams, Case searchCase, Case retrieveCase)
	{
		string value1 = searchCase.caseDescription[consultParams.indexes[0]].value;
		string value2 = retrieveCase.caseDescription[consultParams.indexes[0]].value;

        var A = StringToMatrix(value1);
        var B = StringToMatrix(value2);

		if (value1 == value2)
			return 1f;
		else
			return 0f;
	}

    private string[,] StringToMatrix(string str)
    {
        string[,] matrix;

        // Removendo o {} de fora da matriz
        string aux = str.Remove(0, 1);
        aux = aux.Remove(aux.Length - 1, 1);

        // Separando em linhas
        var lines = aux.Split(':');

        // Pegando a quantidade de valores que tem na matrix
        var columnsSize = CountOccurrences(lines[0], ',');

        // Iniciando a matriz
        matrix = new string[lines.Length, columnsSize];

        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            // Exemplo: { VF - RB,A - LB,,,,,,,,,}

            var line = lines[i];

            // Removendo o {}
            line = line.Remove(0, 1);
            line = line.Remove(line.Length - 1, 1);

            var values = line.Split(',');

            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                matrix[i, j] = values[j];
            }
        }

        return matrix;
    }

    private int CountOccurrences(string str, char delimiter)
    {
        int count = 0;
        foreach (char c in str)
            if (c == delimiter) count++;

        return count;
    }
}