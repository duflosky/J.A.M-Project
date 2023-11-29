using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Serialization;

public class UIManager : MonoBehaviour
{
    public Gauges[] gauges;
    private Dictionary<SpaceshipManager.SystemType, Image> gaugeReferences = new();
    public Transform charactersUIParent;
    public List<CharacterUI> charactersUI;
    private List<CharacterIcon> characterIcons = new();
    public CharacterUI characterUIPrefab;
    public Transform taskNotificationParent;
    public Transform taskParent;
    public TaskUI taskUI;
    public Canvas canvas;
    public TextMeshProUGUI date;
    
    [Serializable] 
    public struct Gauges
    {
        [FormerlySerializedAs("system")] public SpaceshipManager.SystemType systemType;
        public Image gauge;
    }
    
    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        for (int i = 0; i < gauges.Length; i++)
        {
            gaugeReferences.Add(gauges[i].systemType, gauges[i].gauge);
        }

        foreach (var character in GameManager.Instance.SpaceshipManager.characters)
        {
            var ui = Instantiate(characterUIPrefab, charactersUIParent);
            ui.Initialize(character);
            charactersUI.Add(ui);
            characterIcons.Add(ui.icon);
        }
    }

    public void UpdateGauges(SpaceshipManager.SystemType systemType, float value)
    {
        gaugeReferences[systemType].fillAmount = value/20;
    }

    public void UpdateInGameDate(string newDate)
    {
        date.text = newDate;
    }

    public void RefreshCharacterIcons()
    {
        foreach (var icon in characterIcons)
        {
            icon.RefreshIcon();
        }
    }

    public void UpdateCharacterGauges()
    {
        foreach (var charUi in charactersUI)
        {
            charUi.moodGauge.fillAmount = charUi.character.GetMood() / charUi.character.GetMaxMood();
            charUi.volitionGauge.fillAmount = charUi.character.GetVolition() / charUi.character.GetMaxMood();
        }
    }
}
