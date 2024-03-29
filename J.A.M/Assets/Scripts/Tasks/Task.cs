using System;
using System.Collections.Generic;
using SS.Enumerations;
using CharacterSystem;
using UnityEngine;

namespace Tasks
{
    public class Task
    {
        public string Name;
        public string Description;
        public SSTaskStatus TaskStatus;
        public SSTaskType TaskType;
        public Sprite Icon;
        public float TimeLeft;
        public float Duration;
        public float BaseDuration;
        public int MandatorySlots;
        public int OptionalSlots;
        public float HelpFactor;
        public RoomType Room;
        public bool IsTaskTutorial;
        public List<Tuple<ConditionSO, string>> Conditions;
        public int conditionIndex = 0;
        public List<CharacterBehaviour> leaderCharacters = new();
        public List<CharacterBehaviour> assistantCharacters = new();
        public string previewText;
        public bool IsStarted;

        public Task(string name, string description, SSTaskStatus taskStatus, SSTaskType taskType, Sprite icon,
            float timeLeft, float duration, int mandatorySlots,
            int optionalSlots, float helpFactor, RoomType room, bool isTaskTutorial, List<Tuple<ConditionSO, string>> conditions, bool isStarted = false)
        {
            Name = name;
            Description = description;
            TaskStatus = taskStatus;
            TaskType = taskType;
            Icon = icon;
            TimeLeft = timeLeft;
            Duration = duration;
            BaseDuration = duration;
            MandatorySlots = mandatorySlots;
            OptionalSlots = optionalSlots;
            HelpFactor = helpFactor;
            Room = room;
            IsTaskTutorial = isTaskTutorial;
            Conditions = conditions;
            previewText = "";
            IsStarted = isStarted;
        }
    }
}