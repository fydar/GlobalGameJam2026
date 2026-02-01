using GlobalGameJam2026.Gameplay.Overworld.PlayerController;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Line Damage")]
public class LineDamageAbility : Ability
{
    public int damageAmount;
    public int lineLength;
    public Ability_FlatActionPoints cost;

    public override void ConfigureHandle(AbilityHandle abilityHandle)
    {
        // 1. Show all possible tiles that COULD be hit (all 4 directions)
        abilityHandle.BuildReticule = (reticle) =>
        {
            var map = abilityHandle.Combatant.Battle.Map;
            var origin = abilityHandle.Combatant.CurrentTile.LogicalPosition;

            // Add lines in all four directions to show potential range
            reticle.AddToReticle(map.GetLine(origin, OrthogonalDirection.Up, lineLength, false), 3);
            reticle.AddToReticle(map.GetLine(origin, OrthogonalDirection.Down, lineLength, false), 3);
            reticle.AddToReticle(map.GetLine(origin, OrthogonalDirection.Left, lineLength, false), 3);
            reticle.AddToReticle(map.GetLine(origin, OrthogonalDirection.Right, lineLength, false), 3);
        };

        abilityHandle.CanPreview = () =>
        {
            return abilityHandle.Combatant.ActionPoints >= cost.actionPointsCost;
        };

        // 2. Preview only the line currently pointed at by the mouse
        abilityHandle.BuildPreviewReticule = (reticle) =>
        {
            if (abilityHandle.HoveredTile != null)
            {
                var map = abilityHandle.Combatant.Battle.Map;
                var origin = abilityHandle.Combatant.CurrentTile.LogicalPosition;
                var target = abilityHandle.HoveredTile.LogicalPosition;

                // 1. Calculate the relative difference
                Vector2Int diff = target - origin;

                // 2. Validate: Is the tile strictly on an orthogonal axis and within range?
                bool isOrthogonal = (diff.x == 0 && diff.y != 0) || (diff.x != 0 && diff.y == 0);
                bool inRange = Mathf.Max(Mathf.Abs(diff.x), Mathf.Abs(diff.y)) <= lineLength;

                // 3. Only draw if it's a valid "Line" tile
                if (isOrthogonal && inRange)
                {
                    var dir = OrthogonalDirection.FromVector2(new Vector2(diff.x, diff.y));
                    var line = map.GetLine(origin, dir, lineLength, false);

                    reticle.AddToReticle(line, 3); // Highlight active target line
                }
            }
        };

        abilityHandle.CastCoroutine = Cast;

        IEnumerator Cast(AbilityHandle handle, BattleTile targetTile)
        {
            var map = abilityHandle.Combatant.Battle.Map;
            var origin = abilityHandle.Combatant.CurrentTile.LogicalPosition;
            var target = abilityHandle.HoveredTile.LogicalPosition;

            // 1. Calculate the relative difference
            Vector2Int diff = target - origin;

            // 2. Validate: Is the tile strictly on an orthogonal axis and within range?
            bool isOrthogonal = (diff.x == 0 && diff.y != 0) || (diff.x != 0 && diff.y == 0);
            bool inRange = Mathf.Max(Mathf.Abs(diff.x), Mathf.Abs(diff.y)) <= lineLength;

            // 3. Only draw if it's a valid "Line" tile
            if (isOrthogonal && inRange)
            {
                handle.Combatant.ActionPoints -= cost.actionPointsCost;

                var dir = OrthogonalDirection.FromVector2(new Vector2(diff.x, diff.y));
                var line = map.GetLine(origin, dir, lineLength, false);

                // Visual "Charge Up"
                foreach (var time in new TimedLoop(0.5f))
                {
                    // You could trigger a particle effect here or rotate the unit to face 'dir'
                    yield return time;
                }

                foreach (var tile in line)
                {
                    if (tile.occupant != null)
                    {
                        // Assuming your Combatant class has a TakeDamage method
                        Debug.Log($"Hit {tile.occupant.name} for {damageAmount} damage!");
                        tile.occupant.TakeDamage(damageAmount);
                    }
                }
            }

            handle.IsCapturedControl = false;
            handle.IsCapturedGameflow = false;
        }
    }
}