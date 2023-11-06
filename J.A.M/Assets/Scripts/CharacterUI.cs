using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterUI : MonoBehaviour, IDropHandler
{
    public CharacterBehaviour character;
    public CharacterIcon icon;
    public Image moodGauge;
    public Image volitionGauge;
    public CharacterUISlot slot;
    public void Initialize(CharacterBehaviour c)
    {
        character = c;
        icon.Initialize(character, this);
    }
    
    public void ClearCharacter()
    {
        icon = null;
    }

    public void SetCharacter(CharacterIcon icon)
    {
        this.icon = icon; 
    }
    
    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log(transform.name + " récupère");
        GameObject dropped = eventData.pointerDrag;
        icon = dropped.GetComponent<CharacterIcon>();
        icon.SetupIcon(transform, this);
    }
}
