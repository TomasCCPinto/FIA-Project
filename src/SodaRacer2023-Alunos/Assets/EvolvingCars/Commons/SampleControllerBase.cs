using UnityEngine;
using UnityEngine.UI;
using GeneticSharp.Domain;
using System.Threading;
using System;
using System.Linq;
using UnityEngine.SceneManagement;
using System.IO;
using GeneticSharp.Domain.Chromosomes;
using System.Runtime.Serialization.Formatters.Binary;
using GeneticSharp.Runner.UnityApp.Car;

public abstract class SampleControllerBase : MonoBehaviour {

    private Thread m_gaThread;
    private Text m_generationText;
    private Text m_fitnessText;
    private Text m_previousGenerationText;
    private Text m_previousFitnessText;
    private double m_previousBestFitness;
    private double m_previousAverageFitness;
    private string folderName;
    private string newFileName = "EvolutionLog.csv";

    protected GeneticAlgorithm GA { get; private set; }
    protected bool ChromosomesCleanupEnabled { get; set; }
    protected bool ShowPreviousInfoEnabled { get; set; } = true;
    public Rect Area { get; private set; }

	private void Start()
	{
        Application.runInBackground = true;
        var sampleArea = GameObject.Find("SampleArea");
        Area = sampleArea == null
            ? Camera.main.rect
            : sampleArea.GetComponent<RectTransform>().rect;

        var generationTextGO = GameObject.Find("CurrentInfo/Background/GenerationText");

        if (generationTextGO != null)
        {
            var fitnessTextGO = GameObject.Find("CurrentInfo/Background/FitnessText");
            m_generationText = generationTextGO.GetComponent<Text>();
            m_fitnessText = fitnessTextGO.GetComponent<Text>();

            m_previousGenerationText = GameObject.Find("PreviousInfo/Background/GenerationText").GetComponent<Text>();
            m_previousFitnessText = GameObject.Find("PreviousInfo/Background/FitnessText").GetComponent<Text>();
            m_previousGenerationText.text = string.Empty;
            m_previousFitnessText.text = string.Empty;
        }

        CreateStatsFolder();

        if (m_generationText != null)
        {
            m_generationText.text = string.Empty;
            m_fitnessText.text = string.Empty;
        }

        GA = CreateGA();
        GA.GenerationRan += delegate {
            UpdateStatistics();


            if (ChromosomesCleanupEnabled)
            {
                foreach (var c in GA.Population.CurrentGeneration.Chromosomes)
                {
                    c.Fitness = null;
                }
            }
        };
        StartSample();

        m_gaThread = new Thread(() =>
        {
            try
            {
                Thread.Sleep(1000);
                GA.Start();
            }
            catch(Exception ex)
            {
                if(ex.Message.Length > 0)
                {
                    Debug.LogError($"GA thread error: {ex.Message}");
                }
                
            }
        });
        m_gaThread.Start();
	}

    void Update()
    {
        if (m_generationText != null && GA.Population.CurrentGeneration != null)
        {
            var averageFitness = GA.Population.CurrentGeneration.Chromosomes.Average(c => c.Fitness.HasValue ? c.Fitness.Value : 0);
            var bestFitness = GA.Population.CurrentGeneration.Chromosomes.Max(c => c.Fitness.HasValue ? c.Fitness.Value : 0);

            UpdateTexts(
                m_generationText,
                m_fitnessText,
                GA.GenerationsNumber,
                bestFitness, 
                averageFitness);

            if (ShowPreviousInfoEnabled && GA.GenerationsNumber > 1)
            {
                UpdateTexts(
                    m_previousGenerationText,
                    m_previousFitnessText,
                    GA.GenerationsNumber - 1,
                    m_previousBestFitness,
                    m_previousAverageFitness);
            }
        }


        UpdateSample();
    }

	private void OnDestroy()
	{
        GA.Stop();
        m_gaThread.Abort();
        if(m_generationText != null)
        {
            m_generationText.text = String.Empty;
            m_fitnessText.text = String.Empty;

            m_previousGenerationText.text = String.Empty;
            m_previousFitnessText.text = String.Empty;

        }
        
	}

	protected virtual void StartSample() 
    {
        
    }

    protected abstract GeneticAlgorithm CreateGA();

    protected virtual void UpdateSample()
    {
        
    }

    protected void SetFitnessText(string text)
    {
        if(m_fitnessText != null)
        {
            m_fitnessText.text = text;
        }
    }

    private void UpdateTexts(Text generationText, Text fitnessText, int generationsNumber, double bestFitness, double averageFitness)
    {
        generationText.text = $"Generation: {generationsNumber}";
        fitnessText.text = $"Best Fitness: {bestFitness:N2}\nPopAverage: {averageFitness:N2}";
    }

    private void UpdateStatistics()
    {
        m_previousBestFitness = GA.BestChromosome.Fitness.Value;
        m_previousAverageFitness = GA.Population.CurrentGeneration.Chromosomes.Average(c => c.Fitness.Value);
        //Debug.Log($"Generation: {GA.GenerationsNumber} - Best: ${m_previousBestFitness} - Average: ${m_previousAverageFitness}");

        var best = GA.BestChromosome as CarChromosome;
        string pathToFile = this.folderName + "/" +this.newFileName;
        string generationInfo = $"{GA.GenerationsNumber};{m_previousBestFitness};{m_previousAverageFitness};{best.MaxDistance};{best.MaxDistanceTime};{best.NumberOfWheels};{best.CarMass};{best.IsRoadComplete}" + Environment.NewLine;


        if (!File.Exists(pathToFile))
        {
            string header = "Generation;BestFitness;AverageFitnessPopulation;BestMaxDistance;BestMaxDistanceTime;BestNumberOfWheels;BestCarMass;BestIsRoadComplete" + Environment.NewLine;
            Debug.Log(header);

            File.WriteAllText(pathToFile, header);
        }
        Debug.Log(generationInfo);
        File.AppendAllText(pathToFile, generationInfo);
        DumpOverallBest();
        



    }

    private void DumpOverallBest()
    {
        //Debug.Log(GA.BestChromosome);
        string pathToFile = this.folderName + "/OverallBestGenotype.txt";
        File.WriteAllText(pathToFile, GA.BestChromosome.ToString());
        
    }

    private void CreateStatsFolder()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        string configName = "-crossProb-" + GeneticAlgorithmConfigurations.crossoverProbability.ToString() + "-elite-" + GeneticAlgorithmConfigurations.eliteSize.ToString() + "-mutProb-" + GeneticAlgorithmConfigurations.mutationProbability.ToString();
        this.folderName = "Results/" + sceneName + "-" + DateTime.Now.ToString("yyyy-dd-M-HH-mm-ss") + configName;
        System.IO.Directory.CreateDirectory(this.folderName);
    }
}
