using UnityEngine;
using UnityEngine.EventSystems;

public class SelectedUnitPanel : MonoBehaviour
{
    [SerializeField] private Animator animator;

    [SerializeField] private RectTransform spellButtonHolder;

    [SerializeField] private GameObjectPool<SpellButton> spellButtonPool;

    private void Awake()
    {
        spellButtonPool.Initialise(spellButtonHolder);
    }

    public void Render(Combatant combatant)
    {
        spellButtonPool.ReturnAll();

        gameObject.SetActive(true);
        animator.SetTrigger("ChangeUnit");
        animator.SetBool("Open", true);

        for (int i = 0; i < combatant.activeAbilities.Count; i++)
        {
            AbilityHandle activeAbility = combatant.activeAbilities[i];
            var spellButton = spellButtonPool.Grab(spellButtonHolder);
            spellButton.gameObject.SetActive(true);
            spellButton.Render(activeAbility);

            if (i == 0)
            {
                EventSystem.current.SetSelectedGameObject(spellButton.button.gameObject);
            }
        }
    }

    public void Close()
    {
        animator.SetBool("Open", false);
    }
}
