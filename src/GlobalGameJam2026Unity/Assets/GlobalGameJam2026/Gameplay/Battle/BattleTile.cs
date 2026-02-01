using UnityEngine;
using UnityEngine.UI;

public class BattleTile : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Button button;

    internal BattleController battleController;
    internal Combatant occupant;

    public Button Button => button;

    public Vector2Int LogicalPosition { get; set; }

    public bool SelectedCombatant
    {
        get => animator.GetBool("SelectedCombatant");
        set => animator.SetBool("SelectedCombatant", value);
    }

    public void OnSubmit()
    {
        battleController.OnTileSubmitted(this);
    }

    internal void SetReticuleState(int mode)
    {
        animator.SetInteger("Reticule", mode);
    }
}


