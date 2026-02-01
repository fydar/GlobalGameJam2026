using System;
using UnityEngine;

[Serializable]
public class EnemyTeam : Team
{
    public override Vector2Int GetLogicalFieldingPosition(Map map)
    {
        return FindAvailablePosition(map, map.Width - 1, 0, -1);
    }
}
