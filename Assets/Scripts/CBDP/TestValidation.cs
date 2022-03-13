using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class TestValidation : MonoBehaviour
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
        Invoke(nameof(DoSimilarCase), 1f);
    }
    public void DoSimilarCase()
    {
        //var @Case = GetCurrentCase();
        var case_str = "8;7;{{,,,,,,,,,}:{A-B,,,,,,,,,}:{A-R,F-RF,,,,,,,,}:{VF-L,F-L,VF-L,,,,,,,}:{A-B,C-R,A-LB,VF-R,,,,,,}:{VF-LB,F-LB,VF-LB,F-B,VF-LB,,,,,}:{VF-LB,F-LB,VF-LB,A-B,VF-LB,A-LF,,,,}:{VF-B,F-RB,F-B,VF-R,F-B,VF-R,VF-R,,,}:{VF-LB,VF-LB,VF-LB,F-B,VF-LB,A-L,A-B,VF-L,,}:{VF-B,F-B,VF-B,VF-RB,F-B,F-R,VF-R,A-LB,VF-R,}};{{VC-F,F-R,VF-F,VF-F}:{A-B,F-RB,F-RF,F-F}:{A-R,F-R,VF-RF,VF-F}:{VF-L,A-LB,F-F,VF-LF}:{A-RB,F-R,VF-RF,F-F}:{VF-LB,VF-B,VC-F,F-L}:{VF-LB,F-B,A-LF,F-L}:{F-B,VF-RB,F-R,A-RF}:{VF-LB,VF-B,A-L,F-L}:{VF-B,VF-RB,F-R,C-R}};{{0,0,0,0,0,0,0,0,0,0}:{63,0,0,0,0,0,0,0,0,0}:{43,24,0,0,0,0,0,0,0,0}:{55,54,55,0,0,0,0,0,0,0}:{63,42,83,45,0,0,0,0,0,0}:{85,84,85,64,85,0,0,0,0,0}:{85,84,85,63,85,33,0,0,0,0}:{65,74,64,45,64,45,45,0,0,0}:{85,85,85,64,85,53,63,55,0,0}:{65,64,65,75,64,44,45,83,45,0}};{{11,44,15,15}:{63,74,24,14}:{43,44,25,15}:{55,83,14,35}:{73,44,25,14}:{85,65,11,54}:{85,64,33,54}:{64,75,44,23}:{85,65,53,54}:{65,75,44,42}};{DEFENSIVE,DEFENSIVE,DEFENSIVE,DEFENSIVE,DEFENSIVE,DEFENSIVE,DEFENSIVE,DEFENSIVE,DEFENSIVE,DEFENSIVE};DECEPTIVE;OFENSIVE;True;(case_id:5|solutionType:DECEPTIVE|actions:<Move,Agent3,,NORMAL,F-LB,8>,<Move,Agent1,,NORMAL,F-LB,8>,<Move,Agent5,,NORMAL,F-LB,8>,<Move,Agent2,,NORMAL,F-LB,8>,<Move,Agent3,,NORMAL,F-LF,20>,<Move,Agent1,,NORMAL,F-LF,20>,<Move,Agent5,,NORMAL,F-LF,20>,<Move,Agent2,,NORMAL,F-LF,20>,<Move,Agent4,,NORMAL,F-RF,22>,<Move,Agent3,,NORMAL,F-LF,29>,<Move,Agent2,,NORMAL,F-LF,32>,<Move,Agent1,,NORMAL,F-LF,34>,<Move,Agent5,,NORMAL,F-LF,36>,<Move,Agent4,,NORMAL,F-RF,44>,<Move,Agent5,,NORMAL,F-LF,54>,<Move,Agent1,,NORMAL,F-LF,54>,<Move,Agent3,,NORMAL,F-LF,54>,<Move,Agent2,,NORMAL,F-LF,54>,<Move,Agent5,,NORMAL,F-RF,60>,<Move,Agent1,,NORMAL,F-RF,60>,<Move,Agent3,,NORMAL,F-RF,60>,<Move,Agent2,,NORMAL,F-RF,60>,<Move,Agent4,,NORMAL,F-RF,64>)";

        Debug.Log("CASO ATUAL: " + case_str);
        Case currentCase = CaseToCase(case_str.Split(splitter));
        Case similiarCase = GetSimilarCase(currentCase);
        Debug.Log("CASO SIMILAR: Solução: " + similiarCase.caseSolution[0].value);
        //SendPlan(similiarCase);
    }

    private void SendPlan(Case similiarCase)
    {
        PlayerController player = GameObject.Find("PlayerController").GetComponent<PlayerController>();
        player.ReceivePlan(similiarCase.caseSolution[0].value);
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
            Debug.Log("Caso: " + result.matchCase.caseDescription[0].value + " com " + (result.matchPercentage * 100).ToString("0.00") + "% de similaridade");
            Debug.Log("Caso: " + result.matchCase.caseDescription[0].value + "Similaridade Solução: " + (PlanSimilarity(result.matchCase.caseSolution[0].value, currentCase.caseSolution[0].value) * 100).ToString("0.00") + "%");
        }


        // Não encontrou um que possui essas 3 dependencias 
        Debug.Log("Caso recuperado: " + results[0].matchCase.caseDescription[0].value + " com " + (results[0].matchPercentage * 100).ToString("0.00") + "% de similaridade");





        return results[0].matchCase;
    }

    private float PlanSimilarity(string caseSolution1, string caseSolution2)
    {
        var size = 5;

        int[] counts_a = new int[size];
        int[] counts_b = new int[size];

        var A = new CaseConstructor.Plan(caseSolution1).actions.ToList();
        var B = new CaseConstructor.Plan(caseSolution2).actions.ToList();

        for (int i = 0; i < size; i++)
        {
            counts_a[i] = 0;
            counts_b[i] = 0;
        }

        foreach (var a in A)
        {
            var i = int.Parse(a.agent.Remove(0, a.agent.Length - 1)) - 1;
            counts_a[i]++;
        }

        foreach (var b in B)
        {
            var i = int.Parse(b.agent.Remove(0, b.agent.Length - 1)) - 1;
            counts_b[i]++;
        }

        var similarity = 0f;
        
        for (int i = 0; i < size; i++)
        {
            
            float max = (counts_a[i] >= counts_b[i]) ? counts_a[i] : counts_b[i];
            float min = (counts_a[i] < counts_b[i]) ? counts_a[i] : counts_b[i];
           
            similarity += (max - min) / max;
        }

        similarity /= size;

        return 1f - similarity;
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
            //Debug.Log("Cabeçalho: " + header);

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(splitter);

                //Debug.Log("Lendo linha: " + line);

                Case caso = CaseToCase(values);

                // Adicionando um caso na base de casos
                cbr.AddCase(caso);
            }
        }
        Debug.Log("Base de casos carregada!");
    }

}
