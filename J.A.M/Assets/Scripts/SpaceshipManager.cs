using System;
using System.Collections.Generic;
using UnityEditor.VersionControl;
using UnityEngine;

public class SpaceshipManager : MonoBehaviour
{
    public Room[] rooms;
    public ShipSystem[] shipSystems;
    [SerializeField] private float gaugesThreshold;
    public CharacterBehaviour[] characters;

    [Serializable]
    public struct Room
    {
        public string name;
        public Transform transform;
        public Transform doorPosition;
        public GameObject[] roomObjects;
    }

    [Serializable]
    public class ShipSystem
    {
        public System systemName;
        public GameObject systemObject;
        public float decreaseSpeed;
        [Range(0, 100)]
        public float gaugeValue;
        public TaskDataScriptable task;
    }

    public enum System
    {
        Power = 1,
        Oxygen = 2,
        Food = 3,
    }
    private void InitializeSystems()
    {
        foreach (var system in shipSystems)
        {
            system.gaugeValue = 100;
        }
    }

    private void Start()
    {
        InitializeSystems();
    }

    private void FixedUpdate()
    {
        UpdateSystems();
    }

    private void UpdateSystems()
    {
        foreach (var system in shipSystems)
        {
            system.gaugeValue -= system.decreaseSpeed;
            GameManager.Instance.UIManager.UpdateGauges(system.systemName, system.gaugeValue);
            if ((int)system.gaugeValue == gaugesThreshold)
            {
                SpawnTask(system); 
            }
        }
    }
    
    private void SpawnTask(ShipSystem system)
    {
        GameManager.Instance.UIManager.SpawnTaskUI(system.task);
    }
}
