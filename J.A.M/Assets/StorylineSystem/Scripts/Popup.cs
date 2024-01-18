﻿using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SS
{
    public class Popup : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI popupText;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button passTutorialButton;
        
        [HideInInspector] public bool continueButtonPressed;
        [HideInInspector] public bool passTutorialPressed;

        private void OnEnable()
        {
            continueButton.onClick.AddListener(() => gameObject.SetActive(false));
            passTutorialButton.onClick.AddListener(() => gameObject.SetActive(false));
            continueButton.onClick.AddListener(() => continueButtonPressed = true);
            passTutorialButton.onClick.AddListener(() => passTutorialPressed = true);
        }
        
        private void OnDisable()
        {
            continueButton.onClick.RemoveAllListeners();
            passTutorialButton.onClick.RemoveAllListeners();
        }

        public void Initialize(string text)
        {
            popupText.text = text;
            gameObject.SetActive(true);
            continueButtonPressed = false;
            passTutorialPressed = false;
        }
    }
}