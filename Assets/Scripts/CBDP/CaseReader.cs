using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CaseReader : MonoBehaviour
{
    private CBRAPI cbr;
    private string[] features;

    // Start is called before the first frame update
    void Start()
    {
        cbr = new CBRAPI();
        ConvertCSVToCaseBase();
        Invoke(nameof(DoSimilarCase), 1f);
    }
    public void DoSimilarCase()
    {
        var @Case = GetCurrentCase();
        var case_str = @Case.ToString();

        Debug.Log("CASO ATUAL: " + case_str);
        Case currentCase = CaseToCase(case_str.Split(Config.SPLITTER));
        Case similiarCase = GetSimilarCase(currentCase);
        Debug.Log("CASO SIMILAR: Solução: " + similiarCase.caseSolution[0].value);
        SendPlan(similiarCase);
    }

    private void SendPlan(Case similiarCase)
    {
        SimulationController simulationController = GameObject.Find("SimulationController").GetComponent<SimulationController>();
        simulationController.ReceivePlan(similiarCase.caseSolution[0].value);
    }

    private Case GetSimilarCase(Case currentCase)
    {
        // Instanciacao da estrutura do caso
        ConsultStructure consultStructure = new ConsultStructure();

        // Informando qual medida de similaridade global utilizar
        consultStructure.globalSimilarity = new EuclideanDistance(consultStructure);

        // Estruturacao de como o caso sera consultado na base de casos
        consultStructure.consultParams.Add(new ConsultParams(new List<int> { 1 }, 0.2f, new Equals()));            //Seed
        //consultStructure.consultParams.Add(new ConsultParams(new List<int> { 7 }, 1f, new Equals()));            //Tipo do caso
        //consultStructure.consultParams.Add(new ConsultParams(new List<int> { 8 }, 1f, new Equals()));            //Estratégia 
        //consultStructure.consultParams.Add(new ConsultParams(new List<int> { 9 }, 1f, new Equals()));            //Resultado
        consultStructure.consultParams.Add(new ConsultParams(new List<int> { 2 }, 1f, new MatrixSimilarity()));  //Matriz de agentes
        consultStructure.consultParams.Add(new ConsultParams(new List<int> { 3 }, 0.4f, new MatrixSimilarity()));  //Matriz de objetivos
        consultStructure.consultParams.Add(new ConsultParams(new List<int> { 6 }, 0.2f, new SectorSimilarity()));  //Vetor de setor dos agentes

        // Realizando uma consulta na base de casos (lista já ordenada por maior score)
        List<Result> results = cbr.Retrieve(currentCase, consultStructure);

        // Entre os resltados encontrar o melhor que é (Enganoso, ofensivo e Funcionou)
        foreach (Result result in results)
        {
            //DECEPTIVE;OFENSIVE;True
            if (result.matchCase.caseDescription[7].value.Equals("DECEPTIVE") && 
                result.matchCase.caseDescription[8].value.Equals("OFENSIVE") &&
                result.matchCase.caseDescription[9].value.Equals("True"))
            {
                // Exibindo o resultado da consulta
                Debug.Log("Caso recuperado: " + result.matchCase.caseDescription[0].value + " com " + (result.matchPercentage * 100).ToString("0.00") + "% de similaridade");

                //return result.matchCase;
            }
        }


        // Não encontrou um que possui essas 3 dependencias 
        Debug.Log("Caso recuperado: " + results[0].matchCase.caseDescription[0].value + " com " + (results[0].matchPercentage * 100).ToString("0.00") + "% de similaridade");

        return results[0].matchCase;
    }

    private CaseCBDP GetCurrentCase()
    {
        CBDP caseConstructor = GameObject.Find("CaseConstructor").GetComponent<CBDP>();
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
        using var reader = new StreamReader(Config.DATA_BASE);
        Debug.Log("Lendo " + Config.DATA_BASE);

        if (!reader.EndOfStream)
        {
            var header = reader.ReadLine();
            features = header.Split(Config.SPLITTER);
            //Debug.Log("Cabeçalho: " + header);

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(Config.SPLITTER);

                //Debug.Log("Lendo linha: " + line);

                Case caso = CaseToCase(values);

                // Adicionando um caso na base de casos
                cbr.AddCase(caso);
            }
        }
        Debug.Log("Base de casos carregada!");
    }

 
}
