using GlobalGameJam2026.Gameplay.Overworld.PlayerController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Global Heal")]
public class SingleTargetHealAbility : Ability
{
    public int healAmount;
    public Ability_FlatActionPoints cost;

    public override void ConfigureHandle(AbilityHandle abilityHandle)
    {
        // 1. Show all allies on the map as valid targets
        abilityHandle.BuildReticule = (reticle) =>
        {
            var myTeam = abilityHandle.Combatant.Team;

            foreach (var ally in myTeam.FieldedCombatants)
            {
                // Highlight every ally tile with the standard "Valid" reticle (3)
                reticle.AddToReticle(ally.CurrentTile, 4);
            }
        };

        abilityHandle.CanPreview = () =>
        {
            return abilityHandle.Combatant.ActionPoints >= cost.actionPointsCost;
        };

        // 2. Preview: Highlight the hovered ally specifically
        abilityHandle.BuildPreviewReticule = (reticle) =>
        {
            if (abilityHandle.HoveredTile == null) return;

            var target = abilityHandle.HoveredTile.occupant;

            // Only show reticle if hovering a teammate
            if (target != null && target.Team == abilityHandle.Combatant.Team)
            {
                reticle.AddToReticle(abilityHandle.HoveredTile, 4);
            }
        };

        abilityHandle.CastCoroutine = Cast;
    }

    private IEnumerator Cast(AbilityHandle handle, BattleTile targetTile)
    {
        Combatant target = targetTile.occupant;

        // --- Validation ---
        // Ensure we actually have an ally target before spending points
        if (target == null || target.Team != handle.Combatant.Team)
        {
            Debug.Log("Heal canceled: Invalid target.");
            handle.IsCapturedControl = false;
            yield break;
        }

        // --- Execution ---
        handle.Combatant.ActionPoints -= cost.actionPointsCost;
        handle.IsCapturedGameflow = true;

        // Visual "Cast" timing
        foreach (var time in new TimedLoop(0.4f))
        {
            // Trigger a green glow or sparkle effect here
            yield return time;
        }

        // Assuming your Combatant class has a Heal method
        // target.Heal(healAmount); 
        // If not, directly modify health if it's public:
        // target.Health += healAmount; 

        Debug.Log($"Healed {target.name} for {healAmount} points!");

        handle.IsCapturedGameflow = false;
        handle.IsCapturedControl = false;
    }
}