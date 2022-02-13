using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CaseReader : MonoBehaviour
{
    public string dataBaseName = "CaseDataBaseTest.txt";
    public char splitter = ';';
    private CBRAPI cbr;
    private string[] features;

    // Start is called before the first frame update
    void Start()
    {
        cbr = new CBRAPI();
        ConvertCSVToCaseBase();
        var @Case = GetCurrentCase().ToString().Split(splitter);
        Case currentCase = CaseToCase(@Case);

        Case similiarCase = GetSimilarCase(currentCase);
    }

    private Case GetSimilarCase(Case currentCase)
    {
        // Instanciacao da estrutura do caso
        ConsultStructure consultStructure = new ConsultStructure();

        // Informando qual medida de similaridade global utilizar
        consultStructure.globalSimilarity = new EuclideanDistance(consultStructure);

        // Estruturacao de como o caso sera consultado na base de casos
        consultStructure.consultParams.Add(new ConsultParams(new List<int> { 1, 7, 8, 9 }, 1f, new Equals()));
        consultStructure.consultParams.Add(new ConsultParams(new List<int> { 2, 3 }, 1f, new MatrixSimilarity())); //criar uma fun��o
        consultStructure.consultParams.Add(new ConsultParams(new List<int> { 6 }, 1f, new LinearFunction(0, 10))); //criar fun��o


        // Realizando uma consulta na base de casos
        List<Result> results = cbr.Retrieve(currentCase, consultStructure);

        // Exibindo o resultado da consulta
        Debug.Log("Caso recuperado: " + results[0].id + ": com " + results[0].matchPercentage);

        return results[0].matchCase;
    }

    private CaseConstructor.Case GetCurrentCase()
    {
        CaseConstructor caseConstructor = GameObject.Find("CaseConstructor").GetComponent<CaseConstructor>();
        return caseConstructor.GetCase();
    }

    private Case CaseToCase(string[] values)
    {

        Case caso = new Case();

        //--------------- Estruturacao da descricao do problema do caso
        //Id
        caso.caseDescription.Add(new CaseFeature(0, features[0], typeof(int), values[0]));
        //Seed
        caso.caseDescription.Add(new CaseFeature(1, features[1], typeof(int), values[1]));
        //MaQd
        caso.caseDescription.Add(new CaseFeature(2, features[2], typeof(string), values[2]));
        //MaO
        caso.caseDescription.Add(new CaseFeature(3, features[3], typeof(string), values[3]));
        //MaQd_int
        caso.caseDescription.Add(new CaseFeature(4, features[4], typeof(string), values[4]));
        //MaO_int
        caso.caseDescription.Add(new CaseFeature(5, features[5], typeof(string), values[5]));
        //VaSec
        caso.caseDescription.Add(new CaseFeature(6, features[6], typeof(string), values[6]));
        //CaseType
        caso.caseDescription.Add(new CaseFeature(7, features[7], typeof(string), values[7]));
        //Strategy
        caso.caseDescription.Add(new CaseFeature(8, features[8], typeof(string), values[8]));
        //Result
        caso.caseDescription.Add(new CaseFeature(9, features[9], typeof(string), values[9]));

        //--------------- Estruturacao da descricao da solucao do caso
        //Planning
        caso.caseSolution.Add(new CaseFeature(0, features[10], typeof(string), values[10]));

        return caso;
    }

    void ConvertCSVToCaseBase()
    {
        using var reader = new StreamReader(dataBaseName);
        Debug.Log("Lendo " + dataBaseName);

        if (!reader.EndOfStream)
        {
            var header = reader.ReadLine();
            features = header.Split(splitter);
            Debug.Log("Cabe�alho: " + header);

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(splitter);

                Debug.Log("Lendo linha: " + line);

                Case caso = CaseToCase(values);

                // Adicionando um caso na base de casos
                cbr.AddCase(caso);
                Debug.Log("Caso carregado!");
            }
        }
    }

    private Sector[] StringToVecSector(string str)
    {
        Sector[] vector_sector;

        // Removendo o {} de fora da matriz
        string aux = str.Remove(0, 1);
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

    public int[,] StringToMatrixInt(string str)
    {
        int[,] matrix;

        // Removendo o {} de fora da matriz
        string aux = str.Remove(0, 1);
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
            line = line.Remove(0, 1);
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