using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BattleTile : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Animator animator;
    [SerializeField] private Button button;

    internal BattleController battleController;
    internal Combatant occupant;

    public Button Button => button;

    public Vector2Int LogicalPosition { get; set; }

    private void Update()
    {
        animator.SetBool("SelectedCombatant", battleController.SelectedCombatant != null && battleController.SelectedCombatant == occupant);
    }

    public void OnSubmit()
    {
        battleController.OnTileSubmitted(this);
    }

    internal void SetReticuleState(int mode)
    {
        animator.SetInteger("Reticule", mode);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        EventSystem.current.SetSelectedGameObject(button.gameObject);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (EventSystem.current.currentSelectedGameObject == button.gameObject)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}


