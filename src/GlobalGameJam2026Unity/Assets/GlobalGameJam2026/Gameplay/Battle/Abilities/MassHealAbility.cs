using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Mass Heal")]
public class MassHealAbility : Ability
{
    public int healAmount;
    public Ability_FlatActionPoints cost;

    public override void ConfigureHandle(AbilityHandle abilityHandle)
    {
        // 1. Show reticles under all friendly units currently on the field
        abilityHandle.BuildReticule = (reticle) =>
        {
            var allies = abilityHandle.Combatant.Team.FieldedCombatants;
            List<BattleTile> allyTiles = new List<BattleTile>();

            foreach (var ally in allies)
            {
                if (ally.CurrentTile != null)
                {
                    allyTiles.Add(ally.CurrentTile);
                }
            }

            // Using ID 1 for a "Friendly/Positive" highlight
            reticle.AddToReticle(allyTiles.ToArray(), 4);
        };

        abilityHandle.CanPreview = () =>
        {
            return abilityHandle.Combatant.ActionPoints >= cost.actionPointsCost;
        };

        // 2. Preview reticle (could highlight them more intensely or show a different color)
        abilityHandle.BuildPreviewReticule = (reticle) =>
        {
            var allies = abilityHandle.Combatant.Team.FieldedCombatants;
            List<BattleTile> allyTiles = new List<BattleTile>();

            foreach (var ally in allies)
            {
                if (ally.CurrentTile != null)
                {
                    allyTiles.Add(ally.CurrentTile);
                }
            }

            // Using ID 1 for a "Friendly/Positive" highlight
            reticle.AddToReticle(allyTiles.ToArray(), 4);
        };

        abilityHandle.CastCoroutine = Cast;

        IEnumerator Cast(AbilityHandle handle, BattleTile target)
        {
            handle.IsCapturedControl = false;
            handle.IsCapturedGameflow = true;

            // Optional: Visual effect "Charge" time
            foreach (var time in new TimedLoop(0.4f))
            {
                yield return time;
            }

            var allies = handle.Combatant.Team.FieldedCombatants;

            // Heal every ally in the team list
            foreach (var ally in allies)
            {
                // Assuming Combatant has a Heal method (standard for GGJ projects)
                // ally.Heal(healAmount); 

                // If you don't have a Heal method, you can manually adjust HP:
                // ally.CurrentHP = Mathf.Min(ally.MaxHP, ally.CurrentHP + healAmount);

                Debug.Log($"Healed {ally.name} for {healAmount}.");

                // Trigger a visual effect at the ally's position if you have one
                // Instantiate(healEffectPrefab, ally.transform.position, Quaternion.identity);
            }

            handle.IsCapturedGameflow = false;
        }
    }
}