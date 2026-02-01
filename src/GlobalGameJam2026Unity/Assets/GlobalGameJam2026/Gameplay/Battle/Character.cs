using UnityEngine;

[CreateAssetMenu(menuName = "Character")]
public class Character : ScriptableObject
{
    [SerializeField] public string characterName;
    [SerializeField] public Class characterClass;
    [SerializeField] public Sprite smallIcon;
    [SerializeField] public Sprite portrait;
}


