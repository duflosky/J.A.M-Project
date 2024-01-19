using Managers;
using UnityEngine;

public class ShipControlPanel : MonoBehaviour
{
    [SerializeField] private Animator animator;

    public void Appear(bool state)
    {
        TimeTickSystem.ModifyTimeScale(state ? 0 : TimeTickSystem.lastActiveTimeScale);
        GameManager.Instance.taskOpened = state;
        animator.SetBool("Appear", state);
    }
}
