using GlobalGameJam2026.Gameplay.Overworld.PlayerController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Sheepish Summon")]
public class SheepishAbility : Ability
{
    [Header("Summon Settings")]
    public Combatant sheepPrefab;
    public Ability_FlatActionPoints cost;

    public override void ConfigureHandle(AbilityHandle abilityHandle)
    {
        // 1. Show all adjacent empty tiles as valid summon locations (Index 1)
        abilityHandle.BuildReticule = (reticle) =>
        {
            var map = abilityHandle.Combatant.Battle.Map;
            var origin = abilityHandle.Combatant.CurrentTile.LogicalPosition;

            foreach (var pos in GetAdjacentPositions(origin))
            {
                var tile = map[pos.x, pos.y];
                // Only highlight if the tile is on map and empty
                if (tile != null && tile.occupant == null)
                {
                    reticle.AddToReticle(tile, 1);
                }
            }
        };

        abilityHandle.CanPreview = () =>
        {
            return abilityHandle.Combatant.ActionPoints >= cost.actionPointsCost;
        };

        // 2. Preview the specific tile being hovered
        abilityHandle.BuildPreviewReticule = (reticle) =>
        {
            if (abilityHandle.HoveredTile == null) return;

            var map = abilityHandle.Combatant.Battle.Map;
            var origin = abilityHandle.Combatant.CurrentTile.LogicalPosition;
            var hoverPos = abilityHandle.HoveredTile.LogicalPosition;

            // Check Manhattan distance for adjacency
            int dist = Mathf.Abs(origin.x - hoverPos.x) + Mathf.Abs(origin.y - hoverPos.y);

            if (dist == 1 && abilityHandle.HoveredTile.occupant == null)
            {
                reticle.AddToReticle(abilityHandle.HoveredTile, 1);
            }
            else
            {
                reticle.AddToReticle(abilityHandle.HoveredTile, 2); // Blocked/Invalid
            }
        };

        abilityHandle.CastCoroutine = Cast;
    }

    private IEnumerator Cast(AbilityHandle handle, BattleTile targetTile)
    {
        var origin = handle.Combatant.CurrentTile.LogicalPosition;
        var targetPos = targetTile.LogicalPosition;
        int dist = Mathf.Abs(origin.x - targetPos.x) + Mathf.Abs(origin.y - targetPos.y);

        // --- Validation ---
        if (dist != 1 || targetTile.occupant != null)
        {
            Debug.Log("Summon failed: Tile occupied or too far.");
            handle.IsCapturedControl = false;
            yield break;
        }

        // --- Execution ---
        handle.Combatant.ActionPoints -= cost.actionPointsCost;
        handle.IsCapturedGameflow = true;

        // Visual "Poof" timing
        foreach (var time in new TimedLoop(0.3f)) yield return time;

        // 1. Instantiate the prefab
        Combatant newSummon = Instantiate(sheepPrefab, targetTile.transform.position, Quaternion.identity);

        // 2. Register with the existing battle system
        var controller = handle.Combatant.Battle;
        var team = handle.Combatant.Team;

        newSummon.Battle = controller;
        newSummon.Team = team;
        newSummon.CurrentTile = targetTile;
        targetTile.occupant = newSummon;

        // 3. Add to the team's list so they can be selected/cycle
        team.FieldedCombatants.Add(newSummon);

        // 4. Initialize stats (using your RunBattle logic pattern)
        newSummon.Health = newSummon.characterClass.MaxHealth;
        newSummon.ActionPoints = 0; // Usually summons can't act immediately

        Debug.Log($"{handle.Combatant.name} summoned a {newSummon.name}!");

        handle.IsCapturedControl = false;
        handle.IsCapturedGameflow = false;
    }

    private IEnumerable<Vector2Int> GetAdjacentPositions(Vector2Int root)
    {
        yield return root + Vector2Int.up;
        yield return root + Vector2Int.down;
        yield return root + Vector2Int.left;
        yield return root + Vector2Int.right;
    }
}