using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Rally")]
public class RallyAbility : Ability
{
    public Ability_FlatActionPoints totalCost; // Fixed cost for the jump
    public float jumpHeight = 2f;             // How high the "giant leap" arc goes

    public override void ConfigureHandle(AbilityHandle abilityHandle)
    {
        // 1. Define the "Available Area" (Tiles adjacent to allies)
        abilityHandle.BuildReticule = (reticle) =>
        {
            if (abilityHandle.Combatant.ActionPoints < totalCost.actionPointsCost)
            {
                return; // Can't afford to jump at all
            }

            var validJumpTiles = GetRallyTargets(abilityHandle);
            reticle.AddToReticle(validJumpTiles.ToArray(), 1); // Highlight jumpable tiles
        };

        abilityHandle.CanPreview = () =>
        {
            return abilityHandle.Combatant.ActionPoints >= totalCost.actionPointsCost;
        };

        // 2. Define the "Preview" (Just the single target tile)
        abilityHandle.BuildPreviewReticule = (reticle) =>
        {
            if (abilityHandle.HoveredTile != null)
            {
                // Verify the hovered tile is actually a valid rally target
                var validJumpTiles = GetRallyTargets(abilityHandle);
                if (validJumpTiles.Contains(abilityHandle.HoveredTile))
                {
                    reticle.AddToReticle(abilityHandle.HoveredTile, 1);
                }
                else
                {
                    reticle.AddToReticle(abilityHandle.HoveredTile, 2); // Blocked/Invalid
                }
            }
        };

        abilityHandle.CastCoroutine = Cast;
    }

    private IEnumerator Cast(AbilityHandle handle, BattleTile target)
    {
        var validJumpTiles = GetRallyTargets(handle);
        if (!validJumpTiles.Contains(target))
        {
            yield break; // Invalid target
        }

        handle.IsCapturedGameflow = true;

        // --- Execution ---
        handle.Combatant.ActionPoints -= totalCost.actionPointsCost;


        var combatant = handle.Combatant;
        var startPos = combatant.transform.position;
        var endPos = target.transform.position;

        // 1. Clear initial occupancy
        combatant.CurrentTile.occupant = null;

        // 2. The "Giant Leap" (Parabolic Lerp)
        foreach (var time in new TimedLoop(0.6f)) // Slightly slower for a "heavy" jump
        {
            // Standard horizontal movement
            Vector3 currentPos = Vector3.Lerp(startPos, endPos, time);

            // Add vertical "Arc"
            // Formula: Height * (1 - (2t - 1)^2) creates a parabola peaking at t=0.5
            float arc = jumpHeight * (1.0f - Mathf.Pow(2.0f * time - 1.0f, 2.0f));
            currentPos.y += arc;

            combatant.transform.position = currentPos;
            yield return time;
        }

        // 3. Finalize Logic
        combatant.ActionPoints -= totalCost.actionPointsCost;
        combatant.CurrentTile = target;
        target.occupant = combatant;
        combatant.transform.position = endPos; // Snap to final spot

        handle.IsCapturedGameflow = false;
        handle.IsCapturedControl = false;
    }

    #region Rally Logic

    private List<BattleTile> GetRallyTargets(AbilityHandle handle)
    {
        List<BattleTile> targets = new List<BattleTile>();
        var map = handle.Combatant.Battle.Map;
        var allCombatants = handle.Combatant.Team.FieldedCombatants;

        foreach (var other in allCombatants)
        {
            // Only check allies (and ignore self)
            if (other != handle.Combatant && other.Team == handle.Combatant.Team)
            {
                var neighbors = GetAdjacentPositions(other.CurrentTile.LogicalPosition);

                foreach (var pos in neighbors)
                {
                    var tile = map[pos.x, pos.y];
                    // If tile exists and is NOT occupied, it's a valid rally point
                    if (tile != null && tile.occupant == null)
                    {
                        if (!targets.Contains(tile))
                            targets.Add(tile);
                    }
                }
            }
        }
        return targets;
    }

    private IEnumerable<Vector2Int> GetAdjacentPositions(Vector2Int root)
    {
        yield return root + Vector2Int.up;
        yield return root + Vector2Int.down;
        yield return root + Vector2Int.left;
        yield return root + Vector2Int.right;
    }

    #endregion
}