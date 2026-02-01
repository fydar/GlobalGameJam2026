using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Move")]
public class MoveAbility : Ability
{
    public Ability_FlatActionPoints costPerTile;

    public override void ConfigureHandle(AbilityHandle abilityHandle)
    {
        // 1. Define the "Available Area" (Where the player can choose to go)
        abilityHandle.BuildReticule = (reticle) =>
        {
            var (walkable, blocking) = GetWalkableTiles(
                abilityHandle.Combatant.Battle.Map,
                abilityHandle.Combatant.CurrentTile.LogicalPosition,
                costPerTile,
                abilityHandle.Combatant.ActionPoints);

            reticle.AddToReticle(walkable, 1); // Walkable ID
            reticle.AddToReticle(blocking, 2); // Blocking ID
        };

        // 2. Define the "Path Preview" (The specific route to the hovered tile)
        abilityHandle.BuildPreviewReticule = (reticle) =>
        {
            if (abilityHandle.HoveredTile != null)
            {
                var path = FindPath(
                    abilityHandle.Combatant.Battle.Map,
                    abilityHandle.Combatant.CurrentTile.LogicalPosition,
                    abilityHandle.HoveredTile.LogicalPosition);

                if (path != null)
                {
                    List<BattleTile> validSteps = new List<BattleTile>();
                    List<BattleTile> blockedSteps = new List<BattleTile>();

                    int availableAP = abilityHandle.Combatant.ActionPoints;
                    int costPerStep = costPerTile.actionPointsCost;

                    // Determine if the destination was actually reached or just 'approximated'
                    bool targetReached = path.Count > 0 && path[path.Count - 1] == abilityHandle.HoveredTile;

                    for (int i = 0; i < path.Count; i++)
                    {
                        int costToReach = (i + 1) * costPerStep;

                        // A tile is blocked if we can't afford it OR if it's part of an incomplete path
                        if (costToReach <= availableAP)
                        {
                            validSteps.Add(path[i]);
                        }
                        else
                        {
                            blockedSteps.Add(path[i]);
                        }
                    }

                    // If the original target was unreachable, the last tile of the path 
                    // is technically a "blocked" destination even if we can walk to it.
                    if (!targetReached)
                    {
                        // Optional: Force the hovered tile itself to show as blocked 
                        // if it wasn't already in the path
                        if (!path.Contains(abilityHandle.HoveredTile))
                            blockedSteps.Add(abilityHandle.HoveredTile);
                    }

                    if (validSteps.Count > 0) reticle.AddToReticle(validSteps.ToArray(), 1);
                    if (blockedSteps.Count > 0) reticle.AddToReticle(blockedSteps.ToArray(), 2);
                }
            }
        };

        abilityHandle.CastCoroutine = Cast;

        IEnumerator Cast(AbilityHandle handle, BattleTile target)
        {
            handle.IsCapturedControl = false;
            handle.IsCapturedGameflow = true;

            var path = FindPath(
                handle.Combatant.Battle.Map,
                handle.Combatant.CurrentTile.LogicalPosition,
                target.LogicalPosition);

            // Verify path exists and isn't empty
            if (path != null && path.Count > 0)
            {
                // Clear occupancy at the start of the move
                handle.Combatant.CurrentTile.occupant = null;

                foreach (var tile in path)
                {
                    var startWorldPos = handle.Combatant.transform.position;
                    var endWorldPos = tile.transform.position;

                    foreach (var time in new TimedLoop(0.2f))
                    {
                        handle.Combatant.transform.position = Vector3.Lerp(startWorldPos, endWorldPos, time);
                        yield return time;
                    }

                    // Update logical position mid-walk
                    handle.Combatant.CurrentTile = tile;
                }

                // Finalize occupancy at destination
                handle.Combatant.CurrentTile.occupant = handle.Combatant;
            }

            handle.IsCapturedGameflow = false;
        }
    }

    #region Pathfinding Logic

    public (BattleTile[] walkable, BattleTile[] blocking) GetWalkableTiles(Map map, Vector2Int start, Ability_FlatActionPoints cost, int currentAP)
    {
        int maxRange = currentAP / cost.actionPointsCost;
        var reachable = new HashSet<BattleTile>();
        var blocking = new HashSet<BattleTile>();
        var queue = new Queue<(Vector2Int pos, int dist)>();
        var visited = new HashSet<Vector2Int> { start };

        queue.Enqueue((start, 0));

        while (queue.Count > 0)
        {
            var (currentPos, dist) = queue.Dequeue();

            BattleTile currentTile = map[currentPos.x, currentPos.y];
            if (currentTile != null) reachable.Add(currentTile);

            if (dist < maxRange)
            {
                foreach (var dir in new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
                {
                    Vector2Int next = currentPos + dir;
                    if (visited.Contains(next)) continue;

                    BattleTile nextTile = map[next.x, next.y];
                    if (nextTile == null) continue;

                    if (nextTile.occupant == null)
                    {
                        visited.Add(next);
                        queue.Enqueue((next, dist + 1));
                    }
                    else
                    {
                        blocking.Add(nextTile);
                    }
                }
            }
        }

        reachable.Remove(map[start.x, start.y]);
        return (new List<BattleTile>(reachable).ToArray(), new List<BattleTile>(blocking).ToArray());
    }

    private List<BattleTile> FindPath(Map map, Vector2Int start, Vector2Int end)
    {
        var queue = new Queue<Vector2Int>();
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();

        queue.Enqueue(start);
        cameFrom[start] = start;

        Vector2Int bestTile = start;
        float minDistance = Vector2Int.Distance(start, end);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            // Track the tile that gets us closest to the target
            float distToTarget = Vector2Int.Distance(current, end);
            if (distToTarget < minDistance)
            {
                minDistance = distToTarget;
                bestTile = current;
            }

            if (current == end) break;

            foreach (var dir in new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
            {
                Vector2Int next = current + dir;
                BattleTile nextTile = map[next.x, next.y];

                // Only traverse through valid, unoccupied tiles
                if (nextTile != null && nextTile.occupant == null && !cameFrom.ContainsKey(next))
                {
                    queue.Enqueue(next);
                    cameFrom[next] = current;
                }
            }
        }

        // Even if end was not reached, we return the path to bestTile
        List<BattleTile> path = new List<BattleTile>();
        Vector2Int curr = (cameFrom.ContainsKey(end)) ? end : bestTile;

        while (curr != start)
        {
            path.Add(map[curr.x, curr.y]);
            curr = cameFrom[curr];
        }
        path.Reverse();
        return path;
    }

    #endregion
}