using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class TestValidation : MonoBehaviour
{
    private CBRAPI cbr;
    private string[] features;
   

    private Case GetSimilarCase(Case currentCase)
    {
        // Instanciacao da estrutura do caso
        ConsultStructure consultStructure = new ConsultStructure();

        // Informando qual medida de similaridade global utilizar
        consultStructure.globalSimilarity = new EuclideanDistance(consultStructure);

        // Estruturacao de como o caso sera consultado na base de casos
        consultStructure.consultParams.Add(new ConsultParams(new List<int> { 1 }, 0.2f, new Equals()));            //Seed
        //consultStructure.consultParams.Add(new ConsultParams(new List<int> { 7 }, 1f, new Equals()));            //Tipo do caso
        //consultStructure.consultParams.Add(new ConsultParams(new List<int> { 8 }, 1f, new Equals()));            //Estrat?gia 
        //consultStructure.consultParams.Add(new ConsultParams(new List<int> { 9 }, 1f, new Equals()));            //Resultado
        consultStructure.consultParams.Add(new ConsultParams(new List<int> { 2 }, 1f, new MatrixSimilarity()));    //Matriz de agentes
        consultStructure.consultParams.Add(new ConsultParams(new List<int> { 3 }, 0.4f, new MatrixSimilarity()));  //Matriz de objetivos
        consultStructure.consultParams.Add(new ConsultParams(new List<int> { 6 }, 0.2f, new SectorSimilarity()));  //Vetor de setor dos agentes

        // Realizando uma consulta na base de casos (lista j? ordenada por maior score)
        List<Result> results = cbr.Retrieve(currentCase, consultStructure);

        // Entre os resltados encontrar o melhor que ? (Enganoso, ofensivo e Funcionou)
        foreach (Result result in results)
        {
            Debug.Log("Caso: " + result.matchCase.caseDescription[0].value + " com " + (result.matchPercentage * 100).ToString("0.00") + "% de similaridade");
            Debug.Log("Caso: " + result.matchCase.caseDescription[0].value + "Similaridade Solu??o: " + (PlanSimilarity(result.matchCase, currentCase) * 100).ToString("0.00") + "%");
        }


        // N?o encontrou um que possui essas 3 dependencias 
        Debug.Log("Caso recuperado: " + results[0].matchCase.caseDescription[0].value + " com " + (results[0].matchPercentage * 100).ToString("0.00") + "% de similaridade");
        return results[0].matchCase;
    }

    private float PlanSimilarity(Case case1, Case case2)
    {
        PlanSimilarity ps = new PlanSimilarity();
        return ps.GetSimilarity(null, case1, case2);
           
    }

}
