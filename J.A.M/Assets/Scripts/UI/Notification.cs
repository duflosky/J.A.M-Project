using System;
using System.Collections.Generic;
using SS.Enumerations;
using CharacterSystem;
using Managers;
using SS;
using SS.ScriptableObjects;
using Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class Notification : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [HideInInspector] public bool IsCompleted;
        [HideInInspector] public bool IsStarted;
        [HideInInspector] public bool IsCancelled;
        [HideInInspector] public List<SerializableTuple<string, string>> Dialogues;
        [HideInInspector] public Task Task;
        [HideInInspector] public List<CharacterBehaviour> LeaderCharacters = new();
        [HideInInspector] public List<CharacterBehaviour> AssistantCharacters = new();

        [SerializeField] private SpriteRenderer icon;
        [SerializeField] private TextMeshPro time;
        [SerializeField] private SpriteRenderer outlineSprite;
        [SerializeField] private SpriteRenderer timerSprite;
        [SerializeField] private SpriteRenderer timeLeftSprite;
        [SerializeField] private Animator animator;

        [SerializeField] private Sprite hoveredSprite;
        [SerializeField] private Sprite defaultSprite;
        
        private Camera camera;
        private SpaceshipManager spaceshipManager;
        private ConditionSO taskCondition;
        private OutcomeSystem.OutcomeEvent[] outcomeEvents;
        private OutcomeSystem.OutcomeEventArgs[] outcomeEventArgs;
        private SSLauncher launcher;
        private SSTaskNodeSO taskNode;
        private float timeLeft;
        private List<TaskUI.GaugesOutcome> gaugeOutcomes = new List<TaskUI.GaugesOutcome>();

        private void Start()
        {
            camera = Camera.main;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Display();
        }

        public void Initialize(Task task, SSTaskNodeSO ssTaskNode, SpaceshipManager spaceshipManager,
            SSLauncher ssLauncher,
            List<SerializableTuple<string, string>> dialogues = null)
        {
            IsCompleted = false;
            IsCancelled = false;
            Task = task;
            taskNode = ssTaskNode;
            time.text = "";
            Task.TimeLeft *= TimeTickSystem.ticksPerHour;
            timeLeft = Task.TimeLeft;
            icon.sprite = task.Icon;
            Dialogues = dialogues;
            this.spaceshipManager = spaceshipManager;
            launcher = ssLauncher;
            TimeTickSystem.ModifyTimeScale(TimeTickSystem.lastActiveTimeScale);
            timerSprite.material.SetInt("_Arc2", 360);
            timeLeftSprite.material.SetInt("_Arc1", 360);
        }

        public void InitializeCancelTask()
        {
            IsCompleted = false;
            IsCancelled = false;
            taskCondition = Task.Conditions[^1].Item1;
            Task.conditionIndex = Task.Conditions.Count - 1;
            CheckingCondition(true);
        }

        public void Display(CharacterIcon icon = null)
        {
            TimeTickSystem.ModifyTimeScale(0);
            if (IsStarted)
            {
                GameManager.Instance.UIManager.taskUI.DisplayTaskInfo(this);
            }
            else
            {
                if (icon != null)
                {
                    GameManager.Instance.UIManager.taskUI.Initialize(this, icon);
                    return;
                }
                GameManager.Instance.UIManager.taskUI.Initialize(this);
            }
        }

        public void OnStart(List<CharacterUISlot> characters, List<TaskUI.GaugesOutcome> go)
        {
            TimeTickSystem.ModifyTimeScale(TimeTickSystem.lastActiveTimeScale);
            for (var index = 0; index < characters.Count; index++)
            {
                var character = characters[index];
                if (character.isMandatory)
                {
                    if (character.icon != null)
                    {
                        Task.leaderCharacters.Add(character.icon.character);
                        LeaderCharacters.Add(character.icon.character);
                        character.icon.character.AssignTask(this, true);
                    }
                }
                else
                {
                    if (character.icon != null)
                    {
                        Task.assistantCharacters.Add(character.icon.character);
                        AssistantCharacters.Add(character.icon.character);
                        character.icon.character.AssignTask(this);
                    }
                }
            }

            //checkCondition & reference
            var validatedCondition = false;

            if (LeaderCharacters.Count == 0)
            {
                Debug.Log("No leader assigned to task");
                taskCondition = Task.Conditions[^1].Item1;
                Task.conditionIndex = Task.Conditions.Count - 1;
                validatedCondition = true;
            }
            else
            {
                for (int i = 0; i < Task.Conditions.Count; i++)
                {
                    taskCondition = Task.Conditions[i].Item1;
                    validatedCondition = RouteCondition(taskCondition.BaseCondition.target);
                    if (validatedCondition)
                    {
                        Task.conditionIndex = i;
                        break;
                    }
                }
            }
            
            foreach (var outcome in go)
            {
                gaugeOutcomes.Add(outcome);
            }
            CheckingCondition(validatedCondition);
        }

        private void CheckingCondition(bool validatedCondition)
        {
            if (validatedCondition)
            {
                //Check additional conditions
                var additionalConditionOutcomes = new List<Outcome>();
                for (uint i = 0; i < taskCondition.additionnalConditions.Length; i++)
                {
                    var cond = taskCondition.additionnalConditions[i];
                    switch (cond.BaseCondition.target)
                    {
                        case OutcomeData.OutcomeTarget.Leader:
                            if (!ConditionSystem.CheckCharacterCondition(LeaderCharacters[0],
                                    AssistantCharacters.ToArray(), cond))
                                continue;
                            break;

                        case OutcomeData.OutcomeTarget.Assistant:
                            bool condition = false;
                            for (int j = 0; j < AssistantCharacters.Count; j++)
                            {
                                if (AssistantCharacters[j] == null)
                                    if (!ConditionSystem.CheckCharacterCondition(LeaderCharacters[0],
                                            AssistantCharacters.ToArray(),
                                            cond))
                                        condition = true;
                                    else
                                    {
                                        condition = false;
                                        break;
                                    }
                            }

                            if (condition)
                                continue;
                            break;

                        case OutcomeData.OutcomeTarget.Crew:
                            if (!ConditionSystem.CheckCrewCondition(cond))
                                continue;
                            break;

                        case OutcomeData.OutcomeTarget.Ship:
                            if (!ConditionSystem.CheckSpaceshipCondition(cond))
                                continue;
                            break;

                        case OutcomeData.OutcomeTarget.Gauge:
                            if (!ConditionSystem.CheckGaugeCondition(cond))
                                continue;
                            break;

                        case OutcomeData.OutcomeTarget.None:
                            break;
                    }

                    additionalConditionOutcomes.AddRange(cond.outcomes.Outcomes);
                }

                //Generate event args
                outcomeEventArgs =
                    new OutcomeSystem.OutcomeEventArgs[taskCondition.outcomes.Outcomes.Length +
                                                       additionalConditionOutcomes.Count];

                for (int i = 0; i < taskCondition.outcomes.Outcomes.Length; i++)
                {
                    //Generate event args
                    var outcome = taskCondition.outcomes.Outcomes[i];
                    switch (outcome.OutcomeTarget)
                    {
                        case OutcomeData.OutcomeTarget.Leader:
                            outcomeEventArgs[i] = OutcomeSystem.GenerateEventArgs(outcome, LeaderCharacters[0]);
                            break;

                        case OutcomeData.OutcomeTarget.Assistant:
                            outcomeEventArgs[i] = OutcomeSystem.GenerateEventArgs(outcome, AssistantCharacters[0]);
                            break;

                        case OutcomeData.OutcomeTarget.Crew:
                            outcomeEventArgs[i] = OutcomeSystem.GenerateEventArgs(outcome,
                                GameManager.Instance.SpaceshipManager.characters);
                            break;

                        case OutcomeData.OutcomeTarget.Ship:
                            outcomeEventArgs[i] = OutcomeSystem.GenerateEventArgs(outcome);
                            break;
                        case OutcomeData.OutcomeTarget.Gauge:
                            if (outcome.OutcomeType.Equals(OutcomeData.OutcomeType.Gauge))
                                outcomeEventArgs[i] =
                                    OutcomeSystem.GenerateEventArgs(outcome, outcome.OutcomeTargetGauge);
                            else
                                outcomeEventArgs[i] = OutcomeSystem.GenerateEventArgs(outcome,
                                    outcome.OutcomeTargetGauge, LeaderCharacters[0].GetVolition());
                            break;
                    }
                }

                var numberOfBaseOutcomes = taskCondition.outcomes.Outcomes.Length;
                for (int i = 0; i < additionalConditionOutcomes.Count; i++)
                {
                    var outcome = additionalConditionOutcomes[i];
                    switch (outcome.OutcomeTarget)
                    {
                        case OutcomeData.OutcomeTarget.Leader:
                            outcomeEventArgs[numberOfBaseOutcomes + i] =
                                OutcomeSystem.GenerateEventArgs(outcome, LeaderCharacters[0]);
                            break;

                        case OutcomeData.OutcomeTarget.Assistant:
                            outcomeEventArgs[numberOfBaseOutcomes + i] =
                                OutcomeSystem.GenerateEventArgs(outcome, AssistantCharacters[0]);
                            break;

                        case OutcomeData.OutcomeTarget.Crew:
                            outcomeEventArgs[numberOfBaseOutcomes + i] = OutcomeSystem.GenerateEventArgs(outcome,
                                GameManager.Instance.SpaceshipManager.characters);
                            break;

                        case OutcomeData.OutcomeTarget.Ship:
                            outcomeEventArgs[numberOfBaseOutcomes + i] = OutcomeSystem.GenerateEventArgs(outcome);
                            break;

                        case OutcomeData.OutcomeTarget.Gauge:
                            outcomeEventArgs[numberOfBaseOutcomes + i] =
                                OutcomeSystem.GenerateEventArgs(outcome, outcome.OutcomeTargetGauge);
                            break;
                    }
                }

                //Generate events
                outcomeEvents = new OutcomeSystem.OutcomeEvent[outcomeEventArgs.Length];
                for (int i = 0; i < outcomeEventArgs.Length; i++)
                {
                    outcomeEvents[i] = OutcomeSystem.GenerateOutcomeEvent(outcomeEventArgs[i]);
                }

                if (!IsCancelled)
                {
                    Task.Duration = AssistantCharacters.Count > 0
                        ? Task.Duration / Mathf.Pow(AssistantCharacters.Count + LeaderCharacters.Count, Task.HelpFactor)
                        : Task.Duration;
                    
                    switch (Task.Room)
                    {
                        case RoomType.Common:
                            if(GameManager.Instance.SpaceshipManager.SpaceshipTraits.HasFlag(TraitsData.SpaceshipTraits.DamagedCommonRoom))
                                Task.Duration *= 2;
                            break;
                        
                        case RoomType.DockingBay:
                            if(GameManager.Instance.SpaceshipManager.SpaceshipTraits.HasFlag(TraitsData.SpaceshipTraits.DamagedDockingBay))
                                Task.Duration *= 2;
                            break;
                        
                        case RoomType.Power:
                            if(GameManager.Instance.SpaceshipManager.SpaceshipTraits.HasFlag(TraitsData.SpaceshipTraits.DamagedElectricalRoom))
                                Task.Duration *= 2;
                            break;

                        case RoomType.Trajectory:
                            if(GameManager.Instance.SpaceshipManager.SpaceshipTraits.HasFlag(TraitsData.SpaceshipTraits.DamagedBridge))
                                Task.Duration *= 2;
                            break;

                        case RoomType.MedicalBay:
                            if(GameManager.Instance.SpaceshipManager.SpaceshipTraits.HasFlag(TraitsData.SpaceshipTraits.DamagedMedicalRoom))
                                Task.Duration *= 2;
                            break;
                        
                        case RoomType.Cargo1:
                            if(GameManager.Instance.SpaceshipManager.SpaceshipTraits.HasFlag(TraitsData.SpaceshipTraits.DamagedCargoBays))
                                Task.Duration *= 2;
                            break;
                        
                        case RoomType.Cargo2:
                            if(GameManager.Instance.SpaceshipManager.SpaceshipTraits.HasFlag(TraitsData.SpaceshipTraits.DamagedCargoBays))
                                Task.Duration *= 2;
                            break;
                        
                        case RoomType.Cargo3:
                            if(GameManager.Instance.SpaceshipManager.SpaceshipTraits.HasFlag(TraitsData.SpaceshipTraits.DamagedCargoBays))
                                Task.Duration *= 2;
                            break;
                            
                        case RoomType.Military:
                            if(GameManager.Instance.SpaceshipManager.SpaceshipTraits.HasFlag(TraitsData.SpaceshipTraits.DamagedCamp))
                                Task.Duration *= 2;
                            break;
                        
                        case RoomType.AI:
                            if(GameManager.Instance.SpaceshipManager.SpaceshipTraits.HasFlag(TraitsData.SpaceshipTraits.DamagedAI))
                                Task.Duration *= 2;
                            break;
                        
                        case RoomType.BedroomCaptain:
                            if(GameManager.Instance.SpaceshipManager.SpaceshipTraits.HasFlag(TraitsData.SpaceshipTraits.DamagedBedrooms))
                                Task.Duration *= 2;
                            break;
                            
                        case RoomType.BedroomMusician:
                            break;
                        
                        case RoomType.BedroomProfessor:
                            break;
                            
                        case RoomType.Artifact:
                            if(GameManager.Instance.SpaceshipManager.SpaceshipTraits.HasFlag(TraitsData.SpaceshipTraits.DamagedArtifactRoom))
                                Task.Duration *= 2;
                            break;
                        
                        case RoomType.Kitchen:
                            if(GameManager.Instance.SpaceshipManager.SpaceshipTraits.HasFlag(TraitsData.SpaceshipTraits.DamagedCommodities))
                                Task.Duration *= 2;
                            break;
                        
                        case RoomType.Bath:
                            break;
                    }
                    
                    Task.Duration *= TimeTickSystem.ticksPerHour;
                    Task.BaseDuration = Task.Duration;
                }

                IsCancelled = false;
                IsStarted = true;
            }
            else
            {
                outcomeEventArgs = Array.Empty<OutcomeSystem.OutcomeEventArgs>();
                outcomeEvents = Array.Empty<OutcomeSystem.OutcomeEvent>();
            }
        }

        private bool RouteCondition(OutcomeData.OutcomeTarget target)
        {
            bool validateCondition = false;
            switch (target)
            {
                case OutcomeData.OutcomeTarget.Leader:
                    validateCondition =
                        ConditionSystem.CheckCharacterCondition(LeaderCharacters[0], AssistantCharacters.ToArray(),
                            taskCondition);
                    break;
                case OutcomeData.OutcomeTarget.Assistant:
                    if (AssistantCharacters.Count >= 1)
                        validateCondition =
                            ConditionSystem.CheckCharacterCondition(AssistantCharacters[0],
                                AssistantCharacters.ToArray(), taskCondition);
                    break;
                case OutcomeData.OutcomeTarget.Gauge:
                    validateCondition = ConditionSystem.CheckGaugeCondition(taskCondition);
                    break;
                case OutcomeData.OutcomeTarget.Crew:
                    validateCondition = ConditionSystem.CheckCrewCondition(taskCondition);
                    break;
                case OutcomeData.OutcomeTarget.Ship:
                    validateCondition = ConditionSystem.CheckSpaceshipCondition(taskCondition);
                    break;
                case OutcomeData.OutcomeTarget.None:
                    validateCondition = true;
                    break;
            }

            return validateCondition;
        }

        public void OnUpdate()
        {
            if (IsStarted && !IsCompleted)
            {
                if (Task.Duration > 0)
                {
                    time.text = TimeTickSystem.GetTicksAsTime((uint)Task.Duration);
                    Task.Duration -= TimeTickSystem.timePerTick;
                    timerSprite.material.SetInt("_Arc2", (int)(Task.Duration / Task.BaseDuration * 360));
                }
                else
                {
                    OnComplete();
                }
            }
            else if (Task.TaskType.Equals(SSTaskType.Timed) && IsCancelled == false)
            {
                if (Task.TimeLeft > 0)
                {
                    time.text = TimeTickSystem.GetTicksAsTime((uint)Task.TimeLeft);
                    Task.TimeLeft -= TimeTickSystem.timePerTick;
                    timeLeftSprite.material.SetInt("_Arc1", (int)(360 - Task.TimeLeft / timeLeft * 360));
                }
                else if (!IsStarted)
                {
                    GameManager.Instance.UIManager.taskUI.Initialize(this, null, false);
                    GameManager.Instance.UIManager.taskUI.StartTask();
                }
            }
        }

        private void OnComplete()
        {
            for (uint i = 0; i < outcomeEvents.Length; i++)
            {
                Debug.Log($"outcomeEvent {i} : {outcomeEvents[i]}; outcomeEventArgs {i} : {outcomeEventArgs[i]};");
                Debug.Log($"{outcomeEventArgs[i].outcomeType} / {outcomeEventArgs[i].value}");
                outcomeEvents[i].Invoke(outcomeEventArgs[i]);
            }
            IsCompleted = true;
            ResetCharacters();
            GameManager.Instance.RefreshCharacterIcons();
            var notificationContainer = transform.parent.GetComponent<NotificationContainer>();
            transform.parent = null;
            notificationContainer.DisplayNotification();
            spaceshipManager.notificationPool.AddToPool(gameObject);
            spaceshipManager.RemoveGaugeOutcomes(gaugeOutcomes);
            IsStarted = false;
        }

        public void OnCancel()
        {
            if (Task.TaskType.Equals(SSTaskType.Permanent) || Task.TaskType.Equals(SSTaskType.Compute))
            {
                if (!IsStarted || IsCancelled)
                {
                    outcomeEventArgs = Array.Empty<OutcomeSystem.OutcomeEventArgs>();
                    outcomeEvents = Array.Empty<OutcomeSystem.OutcomeEvent>();
                }

                OnComplete();
            }
            else if (Task.TaskType.Equals(SSTaskType.Timed))
            {
                launcher.IsCancelled = true;
                IsStarted = false;
                launcher.RunTimedNodeCancel(this, Task, taskNode);
                spaceshipManager.RemoveGaugeOutcomes(gaugeOutcomes);
            }
            else if (Task.TaskType.Equals(SSTaskType.Untimed))
            {
                launcher.IsCancelled = true;
                IsStarted = false;
                launcher.RunUntimedNodeCancel(this, Task, taskNode);
                spaceshipManager.RemoveGaugeOutcomes(gaugeOutcomes);
            }
        }

        private void ResetCharacters()
        {
            foreach (var character in LeaderCharacters)
            {
                character.StopTask();
            }

            foreach (var character in AssistantCharacters)
            {
                character.StopTask();
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            outlineSprite.sprite = hoveredSprite;
            animator.SetBool("Selected", true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            outlineSprite.sprite = defaultSprite;
            animator.SetBool("Selected", false);
        }
    }
}