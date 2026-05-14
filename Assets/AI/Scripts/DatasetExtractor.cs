using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class DatasetExtractor : EditorWindow
{
    [MenuItem("AI/Generate Training Dataset")]
    public static void GenerateDataset()
    {
        string[] guids = AssetDatabase.FindAssets("t:PatientCaseData");

        List<TrainingEntry> dataset = new List<TrainingEntry>();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            PatientCaseData patient =
                AssetDatabase.LoadAssetAtPath<PatientCaseData>(path);

            if (patient == null)
                continue;

            HashSet<string> features = new HashSet<string>();

            foreach (var node in patient.allNodes)
            {
                foreach (var choice in node.choices)
                {
                    AddNormalizedFeatures(features, choice.gainedTags);
                    AddNormalizedFeatures(features, choice.discoveredSymptoms);
                    AddNormalizedFeatures(features, choice.performedTests);
                }
            }

            TrainingEntry entry = new TrainingEntry
            {
                disease = patient.trueDiseaseId.Trim(),
                features = features.OrderBy(f => f).ToList()
            };

            dataset.Add(entry);
        }

        string json =
            JsonUtility.ToJson(
                new TrainingDatasetWrapper(dataset),
                true);

        string outputPath =
            "Assets/AI/Generated/training_dataset.json";

        File.WriteAllText(outputPath, json);

        AssetDatabase.Refresh();

        Debug.Log($"Clean normalized dataset generated! Total entries: {dataset.Count}");
    }

    private static void AddNormalizedFeatures(HashSet<string> features, List<string> rawValues)
    {
        foreach (string rawValue in rawValues)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
                continue;

            string normalized =
                AIFeatureNormalizer.Normalize(rawValue.Trim());

            if (IsValidFeature(normalized))
                features.Add(normalized);
        }
    }

    private static bool IsValidFeature(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        value = value.Trim();

        // Türkçe açıklama / notebook metni gibi duranları ele
        if (value.Contains(" "))
            return false;

        // Çok kısa anlamsız değerleri ele
        if (value.Length < 3)
            return false;

        return true;
    }
}

[System.Serializable]
public class TrainingEntry
{
    public string disease;
    public List<string> features;
}

[System.Serializable]
public class TrainingDatasetWrapper
{
    public List<TrainingEntry> entries;

    public TrainingDatasetWrapper(List<TrainingEntry> entries)
    {
        this.entries = entries;
    }
}