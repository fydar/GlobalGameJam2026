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
        // 1. Show the 4 adjacent tiles (Up, Down, Left, Right)
        abilityHandle.BuildReticule = (reticle) =>
        {
            var map = abilityHandle.Combatant.Battle.Map;
            var origin = abilityHandle.Combatant.CurrentTile.LogicalPosition;

            // GetDiamond with distance 1 on a square grid gives exactly the 4 orthogonal neighbors
            var adjacentTiles = map.GetDiamond(origin, 1, false);

            // Using ID 2 (red/threat) for the attackable tiles
            reticle.AddToReticle(adjacentTiles, 2);
        };

        // 2. Preview the specific tile being targeted
        abilityHandle.BuildPreviewReticule = (reticle) =>
        {
            if (abilityHandle.HoveredTile != null)
            {
                var origin = abilityHandle.Combatant.CurrentTile.LogicalPosition;
                var target = abilityHandle.HoveredTile.LogicalPosition;

                // Ensure the hovered tile is actually adjacent before showing preview
                int distance = Mathf.Abs(origin.x - target.x) + Mathf.Abs(origin.y - target.y);
                if (distance == 1)
                {
                    reticle.AddToReticle(new[] { abilityHandle.HoveredTile }, 1);
                }
            }
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
