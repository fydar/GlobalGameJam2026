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

        // 2. Preview the trajectory, stopping at the first enemy
        abilityHandle.BuildPreviewReticule = (reticle) =>
        {
            if (abilityHandle.HoveredTile != null)
            {
                var origin = abilityHandle.Combatant.CurrentTile.LogicalPosition;
                var target = abilityHandle.HoveredTile.LogicalPosition;

                Vector2 diff = new Vector2(target.x - origin.x, target.y - origin.y);
                if (diff.magnitude > 0)
                {
                    var dir = OrthogonalDirection.FromVector2(diff);
                    var fullLine = abilityHandle.Combatant.Battle.Map.GetLine(origin, dir, range, false);

                    // Filter the line to stop at the first enemy
                    var finalLine = GetProjectilePath(abilityHandle.Combatant, fullLine);
                    reticle.AddToReticle(finalLine.ToArray(), 1);
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