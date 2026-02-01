using System.Xml;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectedUnitPanel : MonoBehaviour
{
    [SerializeField] private Animator animator;

    [SerializeField] private RectTransform spellButtonHolder;

    [SerializeField] private GameObjectPool<SpellButton> spellButtonPool;

    [SerializeField] private RectTransform actionPointsHolder;
    [SerializeField] private GameObjectPool<RectTransform> actionPointsChit;

    [SerializeField] public Image healthFill;
    [SerializeField] public Image classIcon;
    [SerializeField] public TextMeshProUGUI nameLabel;
    [SerializeField] public TextMeshProUGUI healthText;

    private Combatant combatant;

    private void Awake()
    {
        spellButtonPool.Initialise(spellButtonHolder);
    }

    public void Render(Combatant combatant)
    {
        this.combatant = combatant;
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

    public void Update()
    {
        if (combatant != null)
        {
            healthFill.fillAmount = (float)combatant.Health / combatant.characterClass.MaxHealth;
            classIcon.sprite = combatant.characterClass.characterCreatorIcon;
            nameLabel.text = combatant.characterClass.displayName;
            healthText.text = $"{combatant.Health} / {combatant.characterClass.MaxHealth}";

            actionPointsChit.ReturnAll();

            for (int i = 0; i < combatant.ActionPoints; i++)
            {
                actionPointsChit.Grab(actionPointsHolder);
            }
        }
    }

    public void Close()
    {
        animator.SetBool("Open", false);
    }
}
