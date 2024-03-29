using System;
using CharacterSystem;
using Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class CharacterIcon : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler,
        IPointerEnterHandler, IPointerExitHandler
    {
        public CharacterUI baseParentScript;

        [SerializeField] private Image image;
        [SerializeField] private Image characterIcon;
        [SerializeField] private Image currentTaskImage;
        [SerializeField] private TextMeshProUGUI characterName;
        [NonSerialized] public CharacterBehaviour character;
        [SerializeField] private AudioClip hoverSound;


        private Animator animator;
        private Transform parentAfterDrag;
        private CharacterUI parentScript;
        private float clickTime;
        private uint clicked;
        private float clickDelay = 0.5f;

        public void Initialize(CharacterBehaviour c, CharacterUI script)
        {
            baseParentScript = script;
            baseParentScript.defaultIcon = this;
            character = c;
            characterIcon.sprite = character.GetCharacterData().characterIcon;
            parentScript = script;
            characterName.text = character.GetCharacterData().firstName;
            animator = GetComponent<Animator>();
        }

        public void ResetTransform()
        {
            transform.SetParent(baseParentScript.transform);
            transform.position = baseParentScript.transform.position;
            transform.localScale = baseParentScript.transform.localScale;
            baseParentScript.icon = this;
            parentScript = baseParentScript;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            parentAfterDrag = transform.parent;
            parentScript.ClearCharacter();
            image.raycastTarget = false;
            transform.SetParent(GameManager.Instance.UIManager.canvas.transform);
            if(!GameManager.Instance.taskOpened) GameManager.Instance.SpaceshipManager.DisplayRooms(true);
            animator.SetBool("Selected", true);
        }

        public void OnDrag(PointerEventData eventData)
        {
            //Check si character.IsWorking(), si oui, return ou ResetTransform()
            transform.position = Input.mousePosition;
            GameManager.Instance.UIManager.characterInfoUI.SetupCharacterInfo(character.GetCharacterData());
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            SetupIconValues();
            GameManager.Instance.UIManager.characterInfoUI.ClearCharacterInfo();
            if(!GameManager.Instance.taskOpened) GameManager.Instance.SpaceshipManager.DisplayRooms(false);
        }

        public void SetupIconValues()
        {
            transform.SetParent(parentAfterDrag);
            transform.localPosition = Vector3.zero;
            animator.SetBool("Selected", false);
            parentScript.icon = this;
            image.raycastTarget = true;
        }

        private void AssignTask(Task t)
        {
            currentTaskImage.sprite = t.Icon;
            currentTaskImage.enabled = true;
        }

        public void RefreshIcon()
        {
            if (character.IsWorking())
            {
                AssignTask(character.GetTask());
            }
            else
            {
                StopTask();
            }
        }

        private void StopTask()
        {
            currentTaskImage.enabled = false;
        }

        public void SetupIcon(Transform parent, CharacterUI script)
        {
            parentAfterDrag = parent;
            transform.localPosition = Vector3.zero;
            parentScript = script;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!GameManager.Instance.taskOpened) return;
            clicked++;
            if (clicked == 1) clickTime = Time.time;
            if (clicked > 1 && Time.time - clickTime < clickDelay)
            {
                clicked = 0;
                clickTime = 0;
                GameManager.Instance.UIManager.taskUI.SetLeader(this);
            }
            else if (clicked >= 2 && Time.time - clickTime > clickDelay) clicked = 0;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            GameManager.Instance.UIManager.characterInfoUI.SetupCharacterInfo(character.GetCharacterData());
            SoundManager.Instance.PlaySound(hoverSound);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            GameManager.Instance.UIManager.characterInfoUI.ClearCharacterInfo();
        }
    }
}