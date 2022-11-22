using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Classe utilizada como função de similaridade local na qual retorna a similaridade de duas matrizes.
/// </summary>
public class PlanSimilarity : AbstractLocalSimilarity
{
    /// <summary>
    /// Construtor da classe MatrixSimilarity.
    /// </summary>
    public PlanSimilarity()
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
        var A = new Plan(searchCase.caseSolution[0].value);
        var B = new Plan(retrieveCase.caseSolution[0].value);


        //Contar a quantidade de ações
        int actions_count_A = A.actions.Count;
        int actions_count_B = B.actions.Count;

        //Contar a quantidade de agentes envolvidos
        int agents_count_A = A.CountAgentsInPlan();
        int agents_count_B = B.CountAgentsInPlan();

        //Contar a quatidade de agentes envolvidos e a quantidade de ações que cada agente participa
        int[] counts_a = A.CountActionsOfAgentsInPlan(); // 5 agentes fixo                                                                       ex [3,0,5,7,2]
        int[] counts_b = B.CountActionsOfAgentsInPlan();

        //Contar a Densidade de engano das ações do Plano
        int[] count_deceptions_A = A.CountDeceptionActionsOfPlan();//[NORMAL, DECEPTIVE_1, DECEPTIVE_2, DECEPTIVE_3, DECEPTIVE_4]                ex [3,0,5,7,2]
        int[] count_deceptions_B = B.CountDeceptionActionsOfPlan();

        //Contar a Densidade de engano das ações do Plano
        int[] count_density_A = A.CountDeceptionDensityOfPlan(); //[HIGHLY_DECEPTIVE, PARTIALLY_DECEPTIVE, LITTLE_DECEPTIVE, NOT_DECEPTIVE]      ex [3,0,5,7]
        int[] count_density_B = B.CountDeceptionDensityOfPlan();

        //Calcular a similaridade entre os movimentos das ações de cada plano //Usando jaccard
        var vectorA = A.GetRealObjectives();
        var vectorB = B.GetRealObjectives();
        var intersection = CBDPUtils.Intersection(vectorA, vectorB);
        float JaccardSimilarity_real = intersection.Count / (vectorA.Count + vectorB.Count - intersection.Count); // |A ∩ B| / |A| + |B| - |A ∩ B|

        vectorA = A.GetDeceptiveObjectives();
        vectorB = B.GetDeceptiveObjectives();
        intersection = CBDPUtils.Intersection(vectorA, vectorB);
        float JaccardSimilarity_deceptive = intersection.Count / (vectorA.Count + vectorB.Count - intersection.Count); // |A ∩ B| / |A| + |B| - |A ∩ B|

        //Comparar o custo do caminho das ações
        float[] costs_A = A.GetCostOfActions(); // array de floats [0,1,3,...,n] onde n = A.actions.Count
        float[] costs_B = B.GetCostOfActions();


        var similarity = 0f;

        return 1f - similarity;
    }
}