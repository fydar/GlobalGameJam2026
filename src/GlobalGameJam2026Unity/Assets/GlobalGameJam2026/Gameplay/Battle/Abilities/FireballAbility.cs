using GlobalGameJam2026.Gameplay.Overworld.PlayerController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Fireball")]
public class FireballAbility : Ability
{
    [Header("Stats")]
    public int impactDamage;
    public int splashDamage;
    public int range;
    public Ability_FlatActionPoints cost;

    public override void ConfigureHandle(AbilityHandle abilityHandle)
    {
        // 1. Show orthogonal lines stopping at any blocker
        abilityHandle.BuildReticule = (reticle) =>
        {
            var map = abilityHandle.Combatant.Battle.Map;
            var origin = abilityHandle.Combatant.CurrentTile.LogicalPosition;
            var myTeam = abilityHandle.Combatant.Team;

            OrthogonalDirection[] dirs = { OrthogonalDirection.Up, OrthogonalDirection.Down, OrthogonalDirection.Left, OrthogonalDirection.Right };

            foreach (var dir in dirs)
            {
                var fullLine = map.GetLine(origin, dir, range, false);
                foreach (var tile in fullLine)
                {
                    if (tile.occupant != null)
                    {
                        // Mark blockers (Ally/Enemy) - fireballs explode on contact
                        int iconIndex = (tile.occupant.Team != myTeam) ? 3 : 2;
                        reticle.AddToReticle(tile, iconIndex);
                        break;
                    }
                    reticle.AddToReticle(tile, 3);
                }
            }
        };

        abilityHandle.CanPreview = () => abilityHandle.Combatant.ActionPoints >= cost.actionPointsCost;

        // 2. Preview: Show the 5-tile explosion cross at the impact point
        abilityHandle.BuildPreviewReticule = (reticle) =>
        {
            var impactTarget = GetFirstTargetInDirection(abilityHandle);
            if (impactTarget == null) return;

            var map = abilityHandle.Combatant.Battle.Map;
            var explosionTiles = GetExplosionArea(map, impactTarget.CurrentTile.LogicalPosition);

            // Highlight the entire explosion zone (Index 3 for valid impact)
            foreach (var tile in explosionTiles)
            {
                reticle.AddToReticle(tile, 3);
            }
        };

        abilityHandle.CastCoroutine = Cast;

        IEnumerator Cast(AbilityHandle handle, BattleTile targetTile)
        {
            var impactTarget = GetFirstTargetInDirection(handle);

            // Validation: Must hit something to explode
            if (impactTarget == null)
            {
                Debug.Log("Fireball fizzled: No target in line.");
                handle.IsCapturedControl = false;
                yield break;
            }

            // --- Execution ---
            handle.Combatant.ActionPoints -= cost.actionPointsCost;
            handle.IsCapturedGameflow = true;

            // Visual: Projectile travel delay
            foreach (var time in new TimedLoop(0.3f)) yield return time;

            var map = handle.Combatant.Battle.Map;
            var explosionArea = GetExplosionArea(map, impactTarget.CurrentTile.LogicalPosition);

            foreach (var tile in explosionArea)
            {
                if (tile.occupant != null)
                {
                    // Deal impact damage to direct hit, splash damage to others
                    int finalDamage = (tile.occupant == impactTarget) ? impactDamage : splashDamage;

                    Debug.Log($"Fireball hit {tile.occupant.name} for {finalDamage}!");
                    tile.occupant.TakeDamage(finalDamage);
                }
            }

            handle.IsCapturedControl = false;
            handle.IsCapturedGameflow = false;
        }
    }

    /// <summary>
    /// Returns the center tile and the 4 adjacent cardinal tiles.
    /// </summary>
    private List<BattleTile> GetExplosionArea(Map map, Vector2Int center)
    {
        List<BattleTile> tiles = new List<BattleTile>();

        // Add Center
        if (map[center.x, center.y] != null) tiles.Add(map[center.x, center.y]);

        // Add Cardinals (Up, Down, Left, Right)
        Vector2Int[] offsets = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        foreach (var offset in offsets)
        {
            var pos = center + offset;
            var tile = map[pos.x, pos.y];
            if (tile != null) tiles.Add(tile);
        }

        return tiles;
    }

    private Combatant GetFirstTargetInDirection(AbilityHandle handle)
    {
        if (handle.HoveredTile == null) return null;

        var map = handle.Combatant.Battle.Map;
        var origin = handle.Combatant.CurrentTile.LogicalPosition;
        var hoverPos = handle.HoveredTile.LogicalPosition;

        Vector2Int diff = hoverPos - origin;
        bool isOrthogonal = (diff.x == 0 && diff.y != 0) || (diff.x != 0 && diff.y == 0);

        if (!isOrthogonal) return null;

        var dir = OrthogonalDirection.FromVector2(new Vector2(diff.x, diff.y));
        var line = map.GetLine(origin, dir, range, false);

        foreach (var tile in line)
        {
            if (tile.occupant != null) return tile.occupant;
        }

        return null;
    }
}