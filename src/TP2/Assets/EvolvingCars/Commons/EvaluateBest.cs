using System;
using System.Collections;
using System.Collections.Generic;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Runner.UnityApp.Car;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class EvaluateBest : MonoBehaviour
{
    private static CarSampleConfig s_config;

    private Text m_generationText;
    private Text m_fitnessText;
    private Text m_previousGenerationText;
    private Text m_previousFitnessText;
    private double m_previousBestFitness;
    private double m_previousAverageFitness;
    private string folderName;
    private string newFileName = "EvolutionLog.csv";
    private Vector3 m_lastPosition;
    private PrefabPool m_evaluationPool;
    public UnityEngine.Object EvaluationPrefab;
    public CarSampleConfig Config;
    protected CarChromosome bestCarChromosome;
    protected bool IsBestRunning = false;
    public string FilePath = "";


    protected bool ChromosomesCleanupEnabled { get; set; }
    protected bool ShowPreviousInfoEnabled { get; set; } = true;
    public Rect Area { get; private set; }

    public static void SetConfig(CarSampleConfig config)
    {
        s_config = config;
    }

    private void Awake()
    {
        if (s_config != null)
        {
            Config = s_config;

        }
    }

    private void Start()
    {
        FilePath = EditorUtility.OpenFilePanel("Open Best Individual", "", "txt");
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

        

        if (m_generationText != null)
        {
            m_generationText.text = string.Empty;
            m_fitnessText.text = string.Empty;
        }

        //int[] genotype = new int[] { 1,1,1,1,0,1,0,0,0,0,1,0,1,1,0,0,0,0,0,0,1,0,1,0,0,0,1,0,1,1,0,0,0,0,0,1,1,1,0,0,0,1,1,0,0,1,0,1,1,1,1,0,0,1,0,1,0,0,0,0,1,0,0,1,0,0,1,1,1,0,1,1,0,0,1,0,0,1,0,1,0,0,0,0,0,1,0,1,0,1,1,0,1,0,0,0,0,0,0,0,0,0,1,1,0,1,0,1,0,1,0,0,0,1,0,1,1,0,1,1,0,1,0,1,0,0,0,1,1,0,1,0,0,0,0,0,0,1,1,1,0,1,1,1,0,1,1,1,1,0,1,0,0,0,1,1,0,1,0,0,0,1,0,1,1,1,1,0,0,0,1,0,0,1,1,1,1,0,1,0,1,0,0,1,0,0,1,1,0,0,1,0,0,0,1,1,0,0,0,0,0,1,0,0,1,0,0,0,0,0,0,0,1,1,1,1 };
        //int[] genotype = new int[] { 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 1, 0, 0, 0, 1, 0, 1, 0, 0, 1, 1, 0, 1, 1, 1, 0, 0, 0, 0, 1, 0, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 1, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 1, 0, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 0, 0, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 0, 0, 0, 1, 0, 1, 0, 0, 1, 0, 1, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 1, 0, 0, 0, 0, 0, 1, 1, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 1, 0, 0, 1, 0 };
        double[] genotype = loadGenotype();
        bestCarChromosome = new CarChromosome(Config);
        for(int i = 0; i < genotype.Length; i++)
        {
            bestCarChromosome.ReplaceGene(i, new Gene(genotype[i]));
        }
        
        StartSample();

      
    }

    protected void StartSample()
    {
        ChromosomesCleanupEnabled = false;
        m_lastPosition = Vector3.zero;
        m_evaluationPool = new PrefabPool(EvaluationPrefab);
    }

    protected double[] loadGenotype()
    {
        string lines = System.IO.File.ReadAllLines(FilePath)[0];
        string[] genotypeValues = lines.Split(';');
        double[] genotype = new double[genotypeValues.Length];
        
        for(int i = 0; i < genotypeValues.Length; i++)
        {
            genotype[i] = double.Parse(genotypeValues[i]);
        }
        return genotype;

    }

    protected void UpdateSample()
    {

           
        var c = bestCarChromosome;


        if (!IsBestRunning)
        {


            var evaluation = m_evaluationPool.Get(m_lastPosition);
            evaluation.name = c.ID;
            c.Evaluated = false;

            var road = evaluation.GetComponentInChildren<RoadController>();
            road.Build(Config);

            var car = evaluation.GetComponentInChildren<CarController>();
            car.transform.position = m_lastPosition;
            car.SetChromosome(c, Config);
            IsBestRunning = true;

            try
            {
                Camera m_camera = car.m_cam.Camera;
                m_camera.pixelRect = new Rect(0, 0, Screen.width, Screen.height);
                m_camera.depth = 0;
                m_camera.fieldOfView = 80;
            }
            catch (Exception ex)
            {
                Debug.Log("No Camera Available Yet!");
            }
           

            //BoxCollider2D m_collider = GetComponent<BoxCollider2D>();

           
            //StartCoroutine(car.CheckTimeout());
            //m_collider.size = new Vector2(m_camera.pixelWidth, m_camera.pixelHeight);
        }
    }


    void Update()
    {
       


        UpdateSample();
    }

    private void OnDestroy()
    {
        if (m_generationText != null)
        {
            m_generationText.text = String.Empty;
            m_fitnessText.text = String.Empty;

            m_previousGenerationText.text = String.Empty;
            m_previousFitnessText.text = String.Empty;

        }

    }
    protected void SetFitnessText(string text)
    {
        if (m_fitnessText != null)
        {
            m_fitnessText.text = text;
        }
    }

    private void UpdateTexts(Text generationText, Text fitnessText, int generationsNumber, double bestFitness, double averageFitness)
    {
        generationText.text = $"Generation: {generationsNumber}";
        fitnessText.text = $"Best Fitness: {bestFitness:N2}\nPopAverage: {averageFitness:N2}";
    }
}
