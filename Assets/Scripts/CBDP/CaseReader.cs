using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CaseReader : MonoBehaviour
{
    public string dataBaseName = "CaseDataBase.txt";
    public char splitter = ';';
    private CBRAPI cbr;

    // Start is called before the first frame update
    void Start()
    {
        cbr = new CBRAPI();
        ConvertCSVToCAseBase();

    }

    void ConvertCSVToCAseBase()
    {
        using var reader = new StreamReader(dataBaseName);
        if (!reader.EndOfStream)
        {
            var header = reader.ReadLine();
            var features = header.Split(splitter);


            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(splitter);

                Case caso = new Case();

                //--------------- Estruturacao da descricao do problema do caso
                //Id
                caso.caseDescription.Add(new CaseFeature(0, features[0], typeof(int), values[0]));
                //Seed
                caso.caseDescription.Add(new CaseFeature(1, features[1], typeof(int), values[1]));
                //MaQd
                caso.caseDescription.Add(new CaseFeature(2, features[2], typeof(string[,]), StringToMatrix(values[2])));
                //MaO
                caso.caseDescription.Add(new CaseFeature(3, features[3], typeof(string[,]), StringToMatrix(values[3])));
                //MaQd_int
                caso.caseDescription.Add(new CaseFeature(4, features[4], typeof(int[,]), StringToMatrixInt(values[4])));
                //MaO_int
                caso.caseDescription.Add(new CaseFeature(5, features[5], typeof(int[,]), StringToMatrixInt(values[5])));
                //VaSec
                caso.caseDescription.Add(new CaseFeature(6, features[6], typeof(Sector[]), StringToVecSector(values[6])));
                //CaseType
                caso.caseDescription.Add(new CaseFeature(7, features[7], typeof(string), values[7]));
                //Strategy
                caso.caseDescription.Add(new CaseFeature(8, features[8], typeof(string), values[8]));
                //Result
                caso.caseDescription.Add(new CaseFeature(9, features[9], typeof(string), values[9]));

                //--------------- Estruturacao da descricao da solucao do caso
                //Planning
                caso.caseSolution.Add(new CaseFeature(0, features[10], typeof(string), values[10]));

                // Adicionando um caso na base de casos
                cbr.AddCase(caso);
            }
        }
    }

    private Sector[] StringToVecSector(string str)
    {
        Sector[] vector_sector;

        // Removendo o {} de fora da matriz
        string aux = str.Remove(1);
        aux = aux.Remove(aux.Length - 1, 1);

        // Separando em valores
        var values = aux.Split(',');

        vector_sector = new Sector[values.Length];

        for (int i = 0; i < vector_sector.Length; i++)
        {
            Enum.TryParse(values[i], out Sector sector);
            vector_sector[i] = sector;
        }

        return vector_sector;
    }

    public string[,] StringToMatrix(string str)
    {
        string[,] matrix;

        // Removendo o {} de fora da matriz
        string aux = str.Remove(1);
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
            line = line.Remove(1);
            line = line.Remove(line.Length - 1, 1);

            var values = line.Split(',');

            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                matrix[i, j] = values[j];
            }
        }

        return matrix;
    }

    public int[,] StringToMatrixInt(string str)
    {
        int[,] matrix;

        // Removendo o {} de fora da matriz
        string aux = str.Remove(1);
        aux = aux.Remove(aux.Length - 1, 1);

        // Separando em linhas
        var lines = aux.Split(':');

        // Pegando a quantidade de valores que tem na matrix
        var columnsSize = CountOccurrences(lines[0], ',');

        // Iniciando a matriz
        matrix = new int[lines.Length, columnsSize];

        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            // Exemplo: { VF - RB,A - LB,,,,,,,,,}

            var line = lines[i];

            // Removendo o {}
            line = line.Remove(1);
            line = line.Remove(line.Length - 1, 1);

            var values = line.Split(',');

            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                matrix[i, j] = int.Parse(values[j]);
            }
        }

        return matrix;
    }

    int CountOccurrences(string str, char delimiter)
    {
        int count = 0;
        foreach (char c in str)
            if (c == delimiter) count++;

        return count;
    }
}
