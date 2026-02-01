using GlobalGameJam2026.Gameplay.Overworld.PlayerController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Ricochet")]
public class RicochetAbility : Ability
{
    [Header("Initial Shot")]
    public int initialDamage;
    public int projectileRange;
    public Ability_FlatActionPoints cost;

    [Header("Ricochet Logic")]
    public int maxJumps = 2;
    public int jumpRange = 3;
    public int damageFalloffPerJump = 5;
    public float delayBetweenJumps = 0.25f;

    public override void ConfigureHandle(AbilityHandle abilityHandle)
    {
        // 1. Standard Projectile Reticule (Orthogonal lines, stops at blockers)
        abilityHandle.BuildReticule = (reticle) =>
        {
            var map = abilityHandle.Combatant.Battle.Map;
            var origin = abilityHandle.Combatant.CurrentTile.LogicalPosition;
            var myTeam = abilityHandle.Combatant.Team;

            OrthogonalDirection[] dirs = { OrthogonalDirection.Up, OrthogonalDirection.Down, OrthogonalDirection.Left, OrthogonalDirection.Right };

            foreach (var dir in dirs)
            {
                var fullLine = map.GetLine(origin, dir, projectileRange, false);
                foreach (var tile in fullLine)
                {
                    if (tile.occupant != null)
                    {
                        int iconIndex = (tile.occupant.Team != myTeam) ? 3 : 2;
                        reticle.AddToReticle(tile, iconIndex);
                        break;
                    }
                    reticle.AddToReticle(tile, 3);
                }
            }
        };

        abilityHandle.CanPreview = () => abilityHandle.Combatant.ActionPoints >= cost.actionPointsCost;

        // 2. Preview: Trace the entire chain of bounces
        abilityHandle.BuildPreviewReticule = (reticle) =>
        {
            var chain = CalculateRicochetChain(abilityHandle);
            if (chain.Count == 0) return;

            foreach (var combatant in chain)
            {
                // Highlight every enemy in the chain with the valid reticle (3)
                reticle.AddToReticle(combatant.CurrentTile, 3);
            }
        };

        abilityHandle.CastCoroutine = Cast;
    }

    private IEnumerator Cast(AbilityHandle handle, BattleTile targetTile)
    {
        var chain = CalculateRicochetChain(handle);

        if (chain.Count == 0)
        {
            handle.IsCapturedControl = false;
            yield break;
        }

        // --- Execution ---
        handle.Combatant.ActionPoints -= cost.actionPointsCost;
        handle.IsCapturedGameflow = true;

        int currentDamage = initialDamage;

        for (int i = 0; i < chain.Count; i++)
        {
            Combatant target = chain[i];

            // Apply damage (preventing negative damage)
            int damageToApply = Mathf.Max(0, currentDamage);
            target.TakeDamage(damageToApply);
            Debug.Log($"Ricochet {i} hit {target.name} for {damageToApply}!");

            // Calculate falloff for the next hit
            currentDamage -= damageFalloffPerJump;

            // Wait before the next bounce
            if (i < chain.Count - 1)
            {
                yield return new WaitForSeconds(delayBetweenJumps);
            }
        }

        handle.IsCapturedControl = false;
        handle.IsCapturedGameflow = false;
    }

    /// <summary>
    /// Calculates the sequence of combatants hit by the ricochet.
    /// </summary>
    private List<Combatant> CalculateRicochetChain(AbilityHandle handle)
    {
        List<Combatant> chain = new List<Combatant>();
        HashSet<Combatant> hitList = new HashSet<Combatant>();

        // 1. Find Initial Target
        Combatant firstHit = GetFirstTargetInDirection(handle);
        if (firstHit == null || firstHit.Team == handle.Combatant.Team) return chain;

        chain.Add(firstHit);
        hitList.Add(firstHit);

        // 2. Calculate Bounces
        Combatant currentSource = firstHit;
        for (int i = 0; i < maxJumps; i++)
        {
            Combatant nextTarget = FindNextRicochetTarget(handle, currentSource, hitList);
            if (nextTarget == null) break;

            chain.Add(nextTarget);
            hitList.Add(nextTarget);
            currentSource = nextTarget;
        }

        return chain;
    }

    private Combatant FindNextRicochetTarget(AbilityHandle handle, Combatant source, HashSet<Combatant> alreadyHit)
    {
        var map = handle.Combatant.Battle.Map;
        var myTeam = handle.Combatant.Team;
        var sourcePos = source.CurrentTile.LogicalPosition;

        // Get all tiles within the jump range
        var potentialTiles = map.GetDiamond(sourcePos, jumpRange, false);
        Combatant bestTarget = null;
        float closestDist = float.MaxValue;

        foreach (var tile in potentialTiles)
        {
            if (tile.occupant != null &&
                tile.occupant.Team != myTeam &&
                !alreadyHit.Contains(tile.occupant))
            {
                float dist = Vector2Int.Distance(sourcePos, tile.LogicalPosition);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    bestTarget = tile.occupant;
                }
            }
        }

        return bestTarget;
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
        var line = map.GetLine(origin, dir, projectileRange, false);

        foreach (var tile in line)
        {
            if (tile.occupant != null) return tile.occupant;
        }

        return null;
    }
}