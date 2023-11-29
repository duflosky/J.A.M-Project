using TMPro;
using System.Collections.Generic;
using UnityEngine;

public class TaskUI : MonoBehaviour
{
    [Header("Task")] 
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI timeLeftText;
    [SerializeField] private TextMeshProUGUI durationText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI previewOutcomeText;
    [SerializeField] private Transform leaderSlotsParent;
    [SerializeField] private Transform assistantSlotsParent;
    [SerializeField] private CharacterUISlot[] inactiveSlots;
    [SerializeField] private WarningUI warningUI;

    [Header("Dialogues")] 
    [SerializeField] private GameObject dialogueContainer;

    [Header("Values")] 
    [SerializeField] private float timeLeft;
    [SerializeField] private float duration;

    private TaskNotification taskNotification;
    private List<CharacterUISlot> characterSlots = new();
    private bool taskStarted;

    /*
     * NOTES :
     *      fix : Notif icon grabs raycast
     *      fix : opening menu with notif icon doesnt show assigned characters
     *      add : refreshDisplay to update values after assigning characters
     *      fix : remove characterIcon from task if menu is closed without starting task
     */
    public void Initialize(TaskNotification tn)
    {
        taskNotification = tn;
        warningUI.gameObject.SetActive(false);
        titleText.text = taskNotification.Task.Name;
        timeLeft = taskNotification.Task.TimeLeft;
        duration = taskNotification.Task.Duration;
        descriptionText.text = taskNotification.Task.Description;
        taskStarted = false;
        taskNotification = tn;
        for (int i = 0; i < taskNotification.Task.MandatorySlots; i++)
        {
            var slot = inactiveSlots[i];
            slot.isMandatory = true;
            slot.transform.SetParent(leaderSlotsParent);
            slot.gameObject.SetActive(true);
            characterSlots.Add(slot);
        }

        for (int i = 3; i < taskNotification.Task.OptionalSlots + 3; i++)
        {
            var slot = inactiveSlots[i];
            slot.isMandatory = false;
            slot.transform.SetParent(assistantSlotsParent);
            slot.gameObject.SetActive(true);
            characterSlots.Add(slot);
        }

        timeLeftText.SetText(timeLeft.ToString());

        TimeTickSystem.OnTick += UpdateTask;
        gameObject.SetActive(true);
    }

    // TODO : Player can StartTask() without assigning characters
    public void UpdateTask(object sender, TimeTickSystem.OnTickEventArgs e)
    {
        if (!taskStarted)
        {
            if (CanStartTask())
            {
                if (characterSlots[0] == null) return;
                if (taskNotification.Task.IsPermanent)
                {
                    previewOutcomeText.text = "+ " + (int)characterSlots[0].icon.character.GetVolition() + " " +
                                              taskNotification.Task.PreviewOutcome;
                }
                else
                {
                    previewOutcomeText.text = characterSlots[0].icon.character.GetCharacterData().firstName + " " +
                                              taskNotification.Task.PreviewOutcome;
                }

                var assistantCharacters = 0;
                foreach (var slot in characterSlots)
                {
                    if (!slot.isMandatory && slot.icon != null) assistantCharacters++;
                }

                duration = assistantCharacters > 0
                    ? taskNotification.Task.Duration /
                      (Mathf.Pow(assistantCharacters + 1, taskNotification.Task.HelpFactor))
                    : taskNotification.Task.Duration;
                durationText.text = duration.ToString("F2") + " hours";
            }
            else
            {
                previewOutcomeText.text = null;
            }
        }
    }

    public void StartTask()
    {
        if (CanStartTask())
        {
            if (!CharactersWorking())
            {
                taskNotification.OnStart(characterSlots);
                taskStarted = true;
                CloseTask();
            }
        }
    }

    public void CloseTask()
    {
        foreach (var slot in characterSlots)
        {
            if (slot.icon != null) slot.icon.ResetTransform();
            slot.ClearCharacter();
            slot.gameObject.SetActive(false);
        }

        TimeTickSystem.OnTick -= UpdateTask;
        previewOutcomeText.text = null;
        characterSlots.Clear();
        GameManager.Instance.RefreshCharacterIcons();
        gameObject.SetActive(false);
    }

    public void CloseNotification()
    {
        if (taskNotification.Task.IsPermanent)
        {
            taskNotification.OnCancel();
        }
    }

    private bool CanStartTask()
    {
        foreach (var slot in characterSlots)
        {
            if (slot.isMandatory && slot.icon == null)
            {
                return false;
            }
        }

        return true;
    }

    private bool CharactersWorking()
    {
        foreach (var character in characterSlots)
        {
            if (character.icon != null && character.icon.character.IsWorking())
            {
                warningUI.gameObject.SetActive(true);
                warningUI.Init(character.icon.character);
                return true;
            }
        }

        return false;
    }
}