using GlobalGameJam2026.Gameplay.Overworld.PlayerController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Melee Attack")]
public class MeleeAttackAbility : Ability
{
    public int damageAmount;
    public Ability_FlatActionPoints cost;

    public override void ConfigureHandle(AbilityHandle abilityHandle)
    {
        // 1. Show threat range (filtering out allies)
        abilityHandle.BuildReticule = (reticle) =>
        {
            var map = abilityHandle.Combatant.Battle.Map;
            var origin = abilityHandle.Combatant.CurrentTile.LogicalPosition;
            var adjacentTiles = map.GetDiamond(origin, 1, false);

            List<BattleTile> validTargets = new List<BattleTile>();

            foreach (var tile in adjacentTiles)
            {
                // Logic: Add if empty OR if it's an enemy. (Exclude if occupant is on our team)
                if (tile.occupant == null || tile.occupant.Team != abilityHandle.Combatant.Team)
                {
                    validTargets.Add(tile);
                }
            }

            reticle.AddToReticle(validTargets.ToArray(), 2);
        };

        abilityHandle.CanPreview = () =>
        {
            return abilityHandle.Combatant.ActionPoints >= cost.actionPointsCost;
        };

        // 2. Preview only the specific tile being hovered
        abilityHandle.BuildPreviewReticule = (reticle) =>
        {
            var map = abilityHandle.Combatant.Battle.Map;
            var origin = abilityHandle.Combatant.CurrentTile.LogicalPosition;
            var adjacentTiles = map.GetDiamond(origin, 1, false);

            List<BattleTile> validTargets = new List<BattleTile>();

            foreach (var tile in adjacentTiles)
            {
                // Logic: Add if empty OR if it's an enemy. (Exclude if occupant is on our team)
                if (tile.occupant == null || tile.occupant.Team != abilityHandle.Combatant.Team)
                {
                    validTargets.Add(tile);
                }
            }

            reticle.AddToReticle(validTargets.ToArray(), 2);
        };

        abilityHandle.CastCoroutine = Cast;

        IEnumerator Cast(AbilityHandle handle, BattleTile target)
        {
            handle.IsCapturedControl = false;
            handle.IsCapturedGameflow = true;

            // Face the target (optional visual polish)
            Vector3 direction = (target.transform.position - handle.Combatant.transform.position).normalized;
            if (direction != Vector3.zero)
            {
                handle.Combatant.transform.forward = new Vector3(direction.x, 0, direction.z);
            }

            // Quick "Anticipation" pause
            foreach (var time in new TimedLoop(0.15f)) yield return time;

            // Apply damage if there is an occupant
            if (target.occupant != null)
            {
                //target.occupant.TakeDamage(damageAmount);
                Debug.Log($"Melee hit on {target.occupant.name} for {damageAmount}!");
            }
            else
            {
                Debug.Log("Whiffed! No occupant on target tile.");
            }

            // "Follow-through" pause
            foreach (var time in new TimedLoop(0.1f)) yield return time;

            handle.IsCapturedGameflow = false;
        }
    }
}
