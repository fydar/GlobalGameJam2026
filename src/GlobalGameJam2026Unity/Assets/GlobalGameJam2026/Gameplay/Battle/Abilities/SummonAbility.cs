using GlobalGameJam2026.Gameplay.Overworld.PlayerController;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Summon")]
public class SummonAbility : Ability
{
    [Header("Settings")]
    public Ability_FlatActionPoints cost;
    public float launchHeight = 10f;
    public float airTime = 0.4f; // Time spent going up and down
    public float waitTime = 0.2f; // Time spent "off-screen"

    public override void ConfigureHandle(AbilityHandle abilityHandle)
    {
        // 1. Logic: Allies are targets (1), Free spots are ignored, Blocked spots are (2)
        abilityHandle.BuildReticule = (reticle) =>
        {
            var map = abilityHandle.Combatant.Battle.Map;
            var origin = abilityHandle.Combatant.CurrentTile.LogicalPosition;

            // Mark all adjacent tiles
            foreach (var pos in GetAdjacentPositions(origin))
            {
                var tile = map[pos.x, pos.y];
                if (tile == null) continue;

                if (tile.occupant != null)
                {
                    // If occupant is an ally, it's a valid target (1)
                    // If occupant is an enemy, it's blocked (2)
                    int index = (tile.occupant.Team == abilityHandle.Combatant.Team) ? 1 : 2;
                    reticle.AddToReticle(tile, index);
                }
                else
                {
                    // Technically free, but not a target for summoning
                    // You mentioned showing unfree spots as blocked
                }
            }
        };

        // 2. CanPreview: Only if AP is sufficient AND there is at least one free adjacent slot
        abilityHandle.CanPreview = () =>
        {
            bool hasAP = abilityHandle.Combatant.ActionPoints >= cost.actionPointsCost;
            bool hasFreeSlot = GetFreeAdjacentTiles(abilityHandle).Any();
            return hasAP && hasFreeSlot;
        };

        // 3. Preview: Highlight the targeted ally or show blocked if hovering an enemy/empty
        abilityHandle.BuildPreviewReticule = (reticle) =>
        {
            if (abilityHandle.HoveredTile == null) return;

            var tile = abilityHandle.HoveredTile;
            if (tile.occupant != null && tile.occupant.Team == abilityHandle.Combatant.Team)
            {
                // Is this tile adjacent to the healer?
                float dist = Vector2Int.Distance(tile.LogicalPosition, abilityHandle.Combatant.CurrentTile.LogicalPosition);
                if (dist <= 1.1f) // Basic adjacency check
                {
                    reticle.AddToReticle(tile, 1);
                    return;
                }
            }

            // Otherwise, show as blocked/invalid
            reticle.AddToReticle(tile, 2);
        };

        abilityHandle.CastCoroutine = Cast;
    }

    private IEnumerator Cast(AbilityHandle handle, BattleTile targetTile)
    {
        // Validate target is an ally
        if (targetTile.occupant == null || targetTile.occupant.Team != handle.Combatant.Team)
        {
            Debug.Log("Summon Canceled: Target is not an ally.");
            handle.IsCapturedControl = false;
            yield break;
        }

        // Validate we have a place to put them
        var freeTiles = GetFreeAdjacentTiles(handle);
        if (freeTiles.Count == 0)
        {
            Debug.Log("Summon Canceled: No free space around Healer.");
            handle.IsCapturedControl = false;
            yield break;
        }

        handle.IsCapturedGameflow = true;
        handle.Combatant.ActionPoints -= cost.actionPointsCost;

        Combatant ally = targetTile.occupant;
        BattleTile landingTile = freeTiles[Random.Range(0, freeTiles.Count)];

        Vector3 startPos = ally.transform.position;
        Vector3 peakPos = startPos + Vector3.up * launchHeight;
        Vector3 landPos = landingTile.transform.position;
        Vector3 landPeakPos = landPos + Vector3.up * launchHeight;

        // 1. Launch Up
        ally.CurrentTile.occupant = null; // Remove from current tile
        foreach (var time in new TimedLoop(airTime))
        {
            ally.transform.position = Vector3.Lerp(startPos, peakPos, time);
            yield return time;
        }

        // 2. Wait in the heavens
        yield return new WaitForSeconds(waitTime);

        // 3. Drop Down to new position
        foreach (var time in new TimedLoop(airTime))
        {
            ally.transform.position = Vector3.Lerp(landPeakPos, landPos, time);
            yield return time;
        }

        // 4. Finalize
        ally.CurrentTile = landingTile;
        landingTile.occupant = ally;
        ally.transform.position = landPos;

        handle.IsCapturedGameflow = false;
        handle.IsCapturedControl = false;
    }

    #region Helpers

    private List<BattleTile> GetFreeAdjacentTiles(AbilityHandle handle)
    {
        var map = handle.Combatant.Battle.Map;
        var origin = handle.Combatant.CurrentTile.LogicalPosition;
        List<BattleTile> freeTiles = new List<BattleTile>();

        foreach (var pos in GetAdjacentPositions(origin))
        {
            var tile = map[pos.x, pos.y];
            if (tile != null && tile.occupant == null)
            {
                freeTiles.Add(tile);
            }
        }
        return freeTiles;
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