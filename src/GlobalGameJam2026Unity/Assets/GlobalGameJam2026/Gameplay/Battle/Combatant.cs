using System.Collections.Generic;
using UnityEngine;

public class Combatant : MonoBehaviour
{
    [SerializeField] public Class characterClass;

    public BattleTile CurrentTile;

    public List<AbilityHandle> activeAbilities = new();

    public int ActionPoints { get; internal set; } = 5;
    public BattleController Battle { get; internal set; }
    public Team Team { get; set; }

    public void Start()
    {
        for (int i = 0; i < characterClass.abilities.Length; i++)
        {
            var ability = characterClass.abilities[i];
            var handle = new AbilityHandle(this, ability);
            ability.ConfigureHandle(handle);
            activeAbilities.Add(handle);
        }
    }
}


