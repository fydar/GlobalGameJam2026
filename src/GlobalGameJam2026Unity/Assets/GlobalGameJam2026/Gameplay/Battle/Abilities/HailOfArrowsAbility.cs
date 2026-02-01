using GlobalGameJam2026.Gameplay.Overworld.PlayerController;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Hail of Arrows")]
public class HailOfArrowsAbility : Ability
{
    public int damageAmount;
    public int castRange;
    public Ability_FlatActionPoints cost;

    public override void ConfigureHandle(AbilityHandle abilityHandle)
    {
        // 1. Show all possible tiles that can be targeted (the range)
        abilityHandle.BuildReticule = (reticle) =>
        {
            var map = abilityHandle.Combatant.Battle.Map;
            var origin = abilityHandle.Combatant.CurrentTile.LogicalPosition;

            // GetDiamond with range represents the total area the user can click
            var castableTiles = map.GetDiamond(origin, castRange, false);
            reticle.AddToReticle(castableTiles.ToArray(), 1);
        };

        abilityHandle.CanPreview = () =>
        {
            return abilityHandle.Combatant.ActionPoints >= cost.actionPointsCost;
        };

        // 2. Preview the 3x3 square around the hovered tile
        abilityHandle.BuildPreviewReticule = (reticle) =>
        {
            if (abilityHandle.HoveredTile != null)
            {
                var map = abilityHandle.Combatant.Battle.Map;
                var origin = abilityHandle.Combatant.CurrentTile.LogicalPosition;
                var targetPos = abilityHandle.HoveredTile.LogicalPosition;

                // Check if the center of the hail is within range
                if (Vector2Int.Distance(origin, targetPos) <= castRange + 0.1f)
                {
                    var aoeTiles = Get3x3Area(map, targetPos);
                    reticle.AddToReticle(aoeTiles.ToArray(), 3);
                }
            }
        };

        abilityHandle.CastCoroutine = Cast;

        IEnumerator Cast(AbilityHandle handle, BattleTile targetTile)
        {
            var map = handle.Combatant.Battle.Map;
            var origin = handle.Combatant.CurrentTile.LogicalPosition;
            var targetPos = targetTile.LogicalPosition;

            // Validation: Ensure the click was actually within cast range
            if (Vector2Int.Distance(origin, targetPos) > castRange + 0.1f)
            {
                handle.IsCapturedControl = false;
                yield break;
            }

            // --- Execution ---
            handle.Combatant.ActionPoints -= cost.actionPointsCost;
            handle.IsCapturedGameflow = true;

            // Visual "Volley" timing
            foreach (var time in new TimedLoop(0.6f)) yield return time;

            var affectedTiles = Get3x3Area(map, targetPos);
            foreach (var tile in affectedTiles)
            {
                if (tile.occupant != null)
                {
                    Debug.Log($"Hail of Arrows hit {tile.occupant.name} for {damageAmount}!");
                    tile.occupant.TakeDamage(damageAmount);
                }
            }

            handle.IsCapturedControl = false;
            handle.IsCapturedGameflow = false;
        }
    }

    /// <summary>
    /// Helper to collect a 3x3 grid of tiles around a center point.
    /// </summary>
    private List<BattleTile> Get3x3Area(Map map, Vector2Int center)
    {
        List<BattleTile> area = new List<BattleTile>();
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector2Int currentPos = new Vector2Int(center.x + x, center.y + y);
                var tile = map[currentPos.x, currentPos.y];
                if (tile != null)
                {
                    area.Add(tile);
                }
            }
        }
        return area;
    }
}