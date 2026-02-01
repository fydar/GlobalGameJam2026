using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class Team
{
    public List<Combatant> FieldedCombatants = new();

    public abstract Vector2Int GetLogicalFieldingPosition(Map map);

    protected Vector2Int FindAvailablePosition(Map map, int startX, int endX, int stepX)
    {
        for (int x = startX; stepX > 0 ? x <= endX : x >= endX; x += stepX)
        {
            if (TryFindEmptyInColumn(map, x, out Vector2Int foundPos))
            {
                return foundPos;
            }
        }

        return new Vector2Int(-1, -1);
    }

    private bool TryFindEmptyInColumn(Map map, int x, out Vector2Int position)
    {
        int height = map.Height;
        int centerY = height / 2;

        for (int offset = 0; offset <= height / 2; offset++)
        {
            if (CheckTile(map, x, centerY + offset, out position)) return true;

            if (offset != 0)
            {
                if (CheckTile(map, x, centerY - offset, out position)) return true;
            }
        }

        position = Vector2Int.zero;
        return false;
    }

    private bool CheckTile(Map map, int x, int y, out Vector2Int position)
    {
        position = new Vector2Int(x, y);
        if (y >= 0 && y < map.Height)
        {
            var tile = map[x, y];
            if (tile != null && tile.occupant == null)
            {
                return true;
            }
        }
        return false;
    }
}
