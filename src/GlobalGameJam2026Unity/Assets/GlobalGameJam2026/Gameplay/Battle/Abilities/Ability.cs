using System.Collections;
using UnityEngine;

public abstract class Ability : ScriptableObject
{
    public string DisplayName = "New Ability";
    public Sprite Icon;

    public abstract void ConfigureHandle(AbilityHandle abilityHandle);
}


