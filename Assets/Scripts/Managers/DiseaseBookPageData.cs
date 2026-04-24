using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DiseaseBookPage", menuName = "Game/Disease Book Page")]
public class DiseaseBookPageData : ScriptableObject
{
    [Header("Kimlik")]
    public string diseaseId;
    public string diseaseName;

    [Header("Sol Sayfa")]
    [TextArea(3, 8)] public string generalInfo;
    [TextArea(3, 8)] public string symptomsInfo;
    [TextArea(3, 8)] public string diagnosisTips;

    [Header("Sağ Sayfa")]
    public Sprite diseaseImage;
    [TextArea(3, 8)] public string extraInfo;

    [Header("Vurgulanacak Kelimeler")]
    public List<string> highlightKeywords = new List<string>();
}