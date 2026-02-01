using UnityEngine;

[CreateAssetMenu(menuName = "Class")]
public class Class : ScriptableObject
{
    [SerializeField] public string displayName;
    [SerializeField] public string characterName;
    [SerializeField] public Sprite healthBarIcon;
    [SerializeField] public Sprite characterCreatorIcon;
    public int DefaultActionPoints = 3;
    public int MaxHealth = 100;
    [SerializeField] public Ability[] abilities;
}


