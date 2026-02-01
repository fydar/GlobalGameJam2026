using GlobalGameJam2026.Gameplay.Overworld.PlayerController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "Abilities/Projectile Attack")]
public class ProjectileAbility : Ability
{
    public int damageAmount;
    public int range;
    public Ability_FlatActionPoints cost;

    public override void ConfigureHandle(AbilityHandle abilityHandle)
    {
        // 1. Show all 4 firing lanes
        abilityHandle.BuildReticule = (reticle) =>
        {
            var map = abilityHandle.Combatant.Battle.Map;
            var origin = abilityHandle.Combatant.CurrentTile.LogicalPosition;

            // Show full threat range in all 4 directions
            reticle.AddToReticle(map.GetLine(origin, OrthogonalDirection.Up, range, false), 3);
            reticle.AddToReticle(map.GetLine(origin, OrthogonalDirection.Down, range, false), 3);
            reticle.AddToReticle(map.GetLine(origin, OrthogonalDirection.Left, range, false), 3);
            reticle.AddToReticle(map.GetLine(origin, OrthogonalDirection.Right, range, false), 3);
        };

        abilityHandle.CanPreview = () =>
        {
            return abilityHandle.Combatant.ActionPoints >= cost.actionPointsCost;
        };

        abilityHandle.BuildPreviewReticule = (reticle) =>
        {
            if (abilityHandle.HoveredTile != null)
            {
                var origin = abilityHandle.Combatant.CurrentTile.LogicalPosition;
                var target = abilityHandle.HoveredTile.LogicalPosition;

                Vector2Int diff = target - origin;

                // 1. Validation: Orthogonal and within range
                bool isOrthogonal = (diff.x == 0 && diff.y != 0) || (diff.x != 0 && diff.y == 0);
                bool inRange = Mathf.Max(Mathf.Abs(diff.x), Mathf.Abs(diff.y)) <= range;

                if (isOrthogonal && inRange)
                {
                    var dir = OrthogonalDirection.FromVector2(new Vector2(diff.x, diff.y));
                    var fullLine = abilityHandle.Combatant.Battle.Map.GetLine(origin, dir, range, false);

                    // 2. Identify the first thing we hit in this direction
                    BattleTile firstImpactTile = null;
                    foreach (var tile in fullLine)
                    {
                        if (tile.occupant != null && tile.occupant.Team != abilityHandle.Combatant.Team)
                        {
                            firstImpactTile = tile;
                            break;
                        }
                    }

                    // 3. Logic: Is our HoveredTile an enemy, and is it blocked?
                    var hoveredOccupant = abilityHandle.HoveredTile.occupant;
                    bool isEnemyHovered = hoveredOccupant != null && hoveredOccupant.Team != abilityHandle.Combatant.Team;

                    if (isEnemyHovered)
                    {
                        if (abilityHandle.HoveredTile == firstImpactTile)
                        {
                            // Valid Target: Show as highlighted (ID 1)
                            reticle.AddToReticle(new[] { abilityHandle.HoveredTile }, 3);
                        }
                        else
                        {
                            // Blocked Target: Show as blocked (ID 2)
                            reticle.AddToReticle(new[] { abilityHandle.HoveredTile }, 2);
                        }
                    }
                }
            }
        };

        abilityHandle.CastCoroutine = Cast;

        IEnumerator Cast(AbilityHandle handle, BattleTile targetTile)
        {
            handle.IsCapturedControl = false;
            handle.IsCapturedGameflow = true;

            var originPos = handle.Combatant.CurrentTile.LogicalPosition;
            Vector2 diff = new Vector2(targetTile.LogicalPosition.x - originPos.x, targetTile.LogicalPosition.y - originPos.y);
            var dir = OrthogonalDirection.FromVector2(diff);

            var fullLine = handle.Combatant.Battle.Map.GetLine(originPos, dir, range, false);
            var finalPath = GetProjectilePath(handle.Combatant, fullLine);

            // Visual: Projectile travel
            if (finalPath.Count > 0)
            {
                // Here you would instantiate a projectile prefab and move it along the tiles
                // For now, we simulate with a loop
                foreach (var tile in finalPath)
                {
                    yield return new WaitForSeconds(0.05f); // Fast travel

                    // If this tile has an enemy, stop and hit
                    if (tile.occupant != null && tile.occupant.Team != handle.Combatant.Team)
                    {
                        // tile.occupant.TakeDamage(damageAmount);
                        Debug.Log($"Projectile hit {tile.occupant.name}!");
                        break; // Stop travel on impact
                    }
                }
            }

            handle.IsCapturedGameflow = false;
        }
    }

    /// <summary>
    /// Processes a line of tiles and truncates it at the first tile containing an enemy.
    /// </summary>
    private List<BattleTile> GetProjectilePath(Combatant shooter, BattleTile[] fullLine)
    {
        List<BattleTile> path = new List<BattleTile>();
        foreach (var tile in fullLine)
        {
            path.Add(tile);

            // Stop the path if we hit a unit that isn't on our team
            if (tile.occupant != null && tile.occupant.Team != shooter.Team)
            {
                break;
            }
        }
        return path;
    }
}