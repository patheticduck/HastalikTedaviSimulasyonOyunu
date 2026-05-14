using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DiagnosisAI : MonoBehaviour
{
    [Header("Model JSON")]
    public TextAsset modelJson;

    [Header("Probability Calibration")]
    [SerializeField] private double softmaxTemperature = 4.0;

    private AIModelData modelData;

    private void Awake()
    {
        LoadModel();
    }

    private void LoadModel()
    {
        if (modelJson == null)
        {
            Debug.LogError("DiagnosisAI: modelJson atanmadı!");
            return;
        }

        modelData = JsonUtility.FromJson<AIModelData>(modelJson.text);

        if (modelData == null ||
            modelData.classes == null ||
            modelData.features == null ||
            modelData.class_log_prior == null ||
            modelData.feature_log_prob == null)
        {
            Debug.LogError("DiagnosisAI: model JSON okunamadı veya eksik!");
            return;
        }

        Debug.Log(
            "AI Model Loaded! Class Count: " + modelData.classes.Length +
            " | Feature Count: " + modelData.features.Length
        );
    }

    public List<DiagnosisPrediction> Predict(List<string> rawFeatures)
    {
        List<DiagnosisPrediction> emptyResults = new List<DiagnosisPrediction>();

        if (modelData == null)
        {
            Debug.LogError("DiagnosisAI: modelData null. Model yüklenmemiş.");
            return emptyResults;
        }

        if (rawFeatures == null || rawFeatures.Count == 0)
        {
            Debug.LogWarning("DiagnosisAI: Predict için feature gelmedi.");
            return emptyResults;
        }

        List<string> normalizedFeatures =
            rawFeatures
            .Where(f => !string.IsNullOrWhiteSpace(f))
            .Select(f => AIFeatureNormalizer.Normalize(f))
            .Where(f => !string.IsNullOrWhiteSpace(f))
            .Distinct()
            .ToList();

        Debug.Log("AI Raw Features: " + string.Join(", ", rawFeatures));
        Debug.Log("AI Normalized Features: " + string.Join(", ", normalizedFeatures));

        List<DiagnosisPrediction> results = new List<DiagnosisPrediction>();

        for (int classIndex = 0; classIndex < modelData.classes.Length; classIndex++)
        {
            double score = modelData.class_log_prior[classIndex];

            for (int featureIndex = 0; featureIndex < modelData.features.Length; featureIndex++)
            {
                string feature = modelData.features[featureIndex];

                if (normalizedFeatures.Contains(feature))
                    score += modelData.feature_log_prob[classIndex][featureIndex];
            }

            results.Add(new DiagnosisPrediction
            {
                disease = modelData.classes[classIndex],
                score = score
            });
        }

        if (results.Count == 0)
            return results;

        double maxScore = results.Max(r => r.score);
        double sumExp = 0;

        double temperature = Math.Max(softmaxTemperature, 0.1);

        foreach (var r in results)
        {
            r.probability = Math.Exp((r.score - maxScore) / temperature);
            sumExp += r.probability;
        }

        if (sumExp <= 0)
        {
            Debug.LogWarning("DiagnosisAI: Softmax sumExp 0 veya negatif.");
            return results;
        }

        foreach (var r in results)
        {
            r.probability /= sumExp;
        }

        return results
            .OrderByDescending(r => r.probability)
            .ToList();
    }
}

[Serializable]
public class DiagnosisPrediction
{
    public string disease;
    public double score;
    public double probability;
}

[Serializable]
public class AIModelData
{
    public string[] classes;
    public string[] features;
    public double[] class_log_prior;
    public DoubleArrayWrapper[] feature_log_prob;
}

[Serializable]
public class DoubleArrayWrapper
{
    public double[] values;

    public double this[int index]
    {
        get => values[index];
    }
}