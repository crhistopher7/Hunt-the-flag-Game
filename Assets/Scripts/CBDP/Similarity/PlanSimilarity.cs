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

        //Deceptive level
        var decpLevel_A = A.solutionType;
        var decpLevel_B = B.solutionType;
        float decpLevel_similarity = EqualsSimilarity(decpLevel_A, decpLevel_B);

        //Contar a quantidade de ações
        int actions_count_A = A.actions.Count;
        int actions_count_B = B.actions.Count;
        float actions_count_similarity = EqualsSimilarity(actions_count_A, actions_count_B);

        //Contar a quantidade de agentes envolvidos
        int agents_count_A = A.CountAgentsInPlan();
        int agents_count_B = B.CountAgentsInPlan();
        float agents_count_similarity = EqualsSimilarity(agents_count_A, agents_count_B);

        //Contar a quatidade de agentes envolvidos e a quantidade de ações que cada agente participa
        int[] counts_a = A.CountActionsOfAgentsInPlan(); // 5 agentes fixo                                                                       ex [3,0,5,7,2]
        int[] counts_b = B.CountActionsOfAgentsInPlan();
        float counts_similarity = Hamming(counts_a, counts_b);

        //Contar a quantidade de ações de cada tipo de engano
        int[] count_deceptions_A = A.CountDeceptionActionsOfPlan();//[NORMAL, DECEPTIVE_1, DECEPTIVE_2, DECEPTIVE_3, DECEPTIVE_4]                ex [3,0,5,7,2]
        int[] count_deceptions_B = B.CountDeceptionActionsOfPlan();
        float counts_deceptions_similarity = Hamming(count_deceptions_A, count_deceptions_B);

        //Contar a Densidade de engano das ações do Plano
        int[] count_density_A = A.CountDeceptionDensityOfPlan(); //[HIGHLY_DECEPTIVE, PARTIALLY_DECEPTIVE, LITTLE_DECEPTIVE, NOT_DECEPTIVE]      ex [3,0,5,7]
        int[] count_density_B = B.CountDeceptionDensityOfPlan();
        float counts_density_similarity = Hamming(count_density_A, count_density_B);

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
        float costs_similarity = Cosine(costs_A, costs_B);

        double similarity = 0f;

        similarity += Math.Pow(decpLevel_similarity, 2) + Math.Pow(actions_count_similarity, 2) + Math.Pow(agents_count_similarity, 2) + Math.Pow(counts_similarity, 2) + Math.Pow(counts_deceptions_similarity, 2) +
            Math.Pow(counts_density_similarity, 2) + Math.Pow(JaccardSimilarity_real, 2) + Math.Pow(JaccardSimilarity_deceptive, 2) + Math.Pow(costs_similarity, 2);

        

        return (float)Math.Sqrt(similarity / 9);
    }

    private float Hamming(int[] A, int[] B)
    {
        int count = 0;
        for (int i = 0; i < A.Length; i++)
            if (A[i] != B[i])
                count++;

        float total_size = A.Length;
        float distance = count / total_size;

        return 1f - distance;
    }

    private float EqualsSimilarity(System.Object A, System.Object B)
    {
        if (A.ToString() == B.ToString())
            return 1f;
        return 0f;
    }

    private float Cosine(float[] A, float[] B)
    {
        // Cos(A, B) = A.B / || A || * || B ||

        double divd = 0;
        double divsA = 0;
        double divsB = 0;

        int size = Math.Max(A.Length, B.Length);

            // Cos(A, B) = A.B / || A || * || B ||
        for (int i = 0; i < A.Length; i++)
        {
            
            divd += A[i] * B[i]; //Dividendo
            divsA += Math.Pow(A[i], 2); //Divisor parte ||A||
            divsB += Math.Pow(B[i], 2); //Divisor parte ||B||
        }


        float similarity = (float)((float) divd / (Math.Sqrt(divsA) * Math.Sqrt(divsB)));
        similarity += 1;
        similarity /= 2; //normalize to 0 1

        return similarity;
    }
}