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
    public float airTime = 0.4f;
    public float waitTime = 0.2f;

    public override void ConfigureHandle(AbilityHandle abilityHandle)
    {
        // 1. Logic: Allies get Reticle 1, Blocked adjacent spots get Reticle 2
        abilityHandle.BuildReticule = (reticle) =>
        {
            var map = abilityHandle.Combatant.Battle.Map;
            var origin = abilityHandle.Combatant.CurrentTile.LogicalPosition;
            var myTeam = abilityHandle.Combatant.Team;

            // Highlight blocked adjacent tiles first
            foreach (var pos in GetAdjacentPositions(origin))
            {
                var tile = map[pos.x, pos.y];
                if (tile != null && tile.occupant != null && tile.occupant.Team != abilityHandle.Combatant.Team)
                {
                    reticle.AddToReticle(tile, 2); // Show blocked icon on occupied neighbors
                }
            }

            // Highlight all summonable allies on the field with Reticle 1
            foreach (var ally in myTeam.FieldedCombatants)
            {
                if (ally != abilityHandle.Combatant)
                {
                    reticle.AddToReticle(ally.CurrentTile, 1);
                }
            }
        };

        abilityHandle.CanPreview = () =>
        {
            bool hasAP = abilityHandle.Combatant.ActionPoints >= cost.actionPointsCost;
            bool hasFreeSlot = GetFreeAdjacentTiles(abilityHandle).Any();
            return hasAP && hasFreeSlot;
        };

        // 2. Preview: Specific focus on the hovered ally or blocked status
        abilityHandle.BuildPreviewReticule = (reticle) =>
        {
            if (abilityHandle.HoveredTile == null) return;

            var tile = abilityHandle.HoveredTile;
            var myTeam = abilityHandle.Combatant.Team;

            if (tile.occupant != null)
            {
                // If it's an ally, highlight them as the selected target
                if (tile.occupant.Team == myTeam && tile.occupant != abilityHandle.Combatant)
                {
                    reticle.AddToReticle(tile, 1);
                }
                else
                {
                    // If it's an enemy or self, show blocked
                    reticle.AddToReticle(tile, 2);
                }
            }
        };

        abilityHandle.CastCoroutine = Cast;
    }

    private IEnumerator Cast(AbilityHandle handle, BattleTile targetTile)
    {
        // --- Validation ---
        if (targetTile.occupant == null || targetTile.occupant.Team != handle.Combatant.Team)
        {
            Debug.Log("Summon Canceled: Target is not an ally.");
            handle.IsCapturedControl = false;
            yield break;
        }

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

        // --- Execution ---
        // 1. Launch Up
        ally.CurrentTile.occupant = null;
        foreach (var time in new TimedLoop(airTime))
        {
            ally.transform.position = Vector3.Lerp(startPos, peakPos, time);
            yield return time;
        }

        yield return new WaitForSeconds(waitTime);

        // 2. Drop Down
        foreach (var time in new TimedLoop(airTime))
        {
            ally.transform.position = Vector3.Lerp(landPeakPos, landPos, time);
            yield return time;
        }

        // 3. Finalize
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