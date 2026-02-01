using UnityEngine;

[CreateAssetMenu(menuName = "Class")]
public class Class : ScriptableObject
{
    [SerializeField] public string displayName;
    [SerializeField] public Sprite healthBarIcon;
    [SerializeField] public Sprite characterCreatorIcon;
    [SerializeField] public Ability[] abilities;
}


