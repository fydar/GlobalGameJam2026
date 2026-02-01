using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    [SerializeField] public Combatant combatant;
    [Space]
    [SerializeField] public Image healthFill;
    [SerializeField] public Image classIcon;

    private void Update()
    {
       healthFill.fillAmount = (float)combatant.Health / combatant.characterClass.MaxHealth;
        classIcon.sprite = combatant.characterClass.healthBarIcon;
    }
}


