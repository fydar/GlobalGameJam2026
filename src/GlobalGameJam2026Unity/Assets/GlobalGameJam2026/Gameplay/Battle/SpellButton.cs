using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SpellButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] public Button button;
    [SerializeField] private TextMeshProUGUI abilityLabel;
    [SerializeField] private Image abilityIcon;
    [SerializeField] private TextMeshProUGUI abilityActionPoints;

    public AbilityHandle abilityHandle;

    private void Update()
    {
        if (abilityHandle != null)
        {
            abilityHandle.IsButtonHovered = EventSystem.current.currentSelectedGameObject == button.gameObject;

            button.interactable = abilityHandle.CanPreview();
        }
    }

    public void Render(AbilityHandle abilityHandle)
    {
        if (abilityHandle != null)
        {
            abilityHandle.IsUnitSelected = false;
            abilityHandle.IsButtonHovered = false;
        }
        this.abilityHandle = abilityHandle;

        abilityHandle.IsUnitSelected = true;
        abilityLabel.text = abilityHandle.Ability.DisplayName;
        abilityIcon.sprite = abilityHandle.Ability.Icon;
    }

    public void UISubmit()
    {
        abilityHandle.IsClicked = true;
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


