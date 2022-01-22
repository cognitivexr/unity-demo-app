using System.Collections.Generic;
using CognitiveXR.CogStream;
using Microsoft.MixedReality.Toolkit.UI;
using TMPro;
using UnityEngine;

public class EngineSelectionMenu : MonoBehaviour
{
    [SerializeField] private List<PressableButtonHoloLens2> EngineSelectButtons;
    public delegate void EngineSelected(int engineIdx);
    public static EngineSelected OnEngineSelected;
    
    public void DisplayEngines(List<Engine> engines)
    {
        for (int i = 0; i < EngineSelectButtons.Count; i++)
        {
            EngineSelectButtons[i].gameObject.SetActive(i< engines.Count);

            if (i < engines.Count)
            {
                var textMeshProGo = EngineSelectButtons[i].transform.Find("IconAndText/TextMeshPro");
                textMeshProGo.GetComponent<TMP_Text>().text = engines[i].name;
            }
        }
    }

    public void LaunchEngine(int engineIdx)
    {
        Debug.Log($"engine selected with index {engineIdx}");
        OnEngineSelected(engineIdx);
        gameObject.SetActive(false);
    }
}
