using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectedAbilityPanel : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private TextMeshProUGUI abilityLabel;
    [SerializeField] private Image abilityIcon;

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    public void Render(AbilityHandle abilityHandle)
    {
        gameObject.SetActive(true);
        animator.SetBool("Open", true);

        abilityLabel.text = abilityHandle.Ability.DisplayName;
    }

    public void Close()
    {
        animator.SetBool("Open", false);
    }
}
