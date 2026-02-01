using GlobalGameJam2026.Gameplay.Overworld.PlayerController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Projectile Damage")]
public class ProjectileDamageAbility : Ability
{
    [Header("Stats")]
    public int damageAmount;
    public int range;
    public int projectileCount = 1;
    public float delayBetweenProjectiles = 0.2f;
    public Ability_FlatActionPoints cost;

    public override void ConfigureHandle(AbilityHandle abilityHandle)
    {
        // 1. Show all possible lines, stopping at any blocker (Ally or Enemy)
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
                        // If it's an enemy, it's a valid target marker. If ally, it's just a blocker.
                        int iconIndex = (tile.occupant.Team != myTeam) ? 3 : 2;
                        reticle.AddToReticle(tile, iconIndex);
                        break;
                    }
                    reticle.AddToReticle(tile, 3);
                }
            }
        };

        abilityHandle.CanPreview = () => abilityHandle.Combatant.ActionPoints >= cost.actionPointsCost;

        // 2. Preview: Only highlight if the first thing hit is an enemy
        abilityHandle.BuildPreviewReticule = (reticle) =>
        {
            var firstTarget = GetFirstTargetInDirection(abilityHandle);

            // Only show the preview reticle if the target is actually an enemy
            if (firstTarget != null && firstTarget.Team != abilityHandle.Combatant.Team)
            {
                reticle.AddToReticle(firstTarget.CurrentTile, 3);
            }
            else if (firstTarget != null)
            {
                // Optionally show the 'blocked' icon if an ally is the one in the way
                reticle.AddToReticle(firstTarget.CurrentTile, 2);
            }
        };

        abilityHandle.CastCoroutine = Cast;

        IEnumerator Cast(AbilityHandle handle, BattleTile targetTile)
        {
            var firstTarget = GetFirstTargetInDirection(handle);

            // --- Validation ---
            // Cancel if no target, or if the first target hit is on the same team
            if (firstTarget == null || firstTarget.Team == handle.Combatant.Team)
            {
                Debug.Log("Invalid Target or Ally in way. Cast canceled.");
                handle.IsCapturedControl = false;
                handle.IsCapturedGameflow = false;
                yield break;
            }

            // --- Execution ---
            handle.Combatant.ActionPoints -= cost.actionPointsCost;

            for (int i = 0; i < projectileCount; i++)
            {
                Debug.Log($"Projectile {i + 1} hit {firstTarget.name}!");
                firstTarget.TakeDamage(damageAmount);

                if (i < projectileCount - 1)
                {
                    yield return new WaitForSeconds(delayBetweenProjectiles);
                }
            }

            handle.IsCapturedControl = false;
            handle.IsCapturedGameflow = false;
        }
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
            if (tile.occupant != null)
            {
                // Returns the first occupant found (could be ally or enemy)
                return tile.occupant;
            }
        }

        return null;
    }
}