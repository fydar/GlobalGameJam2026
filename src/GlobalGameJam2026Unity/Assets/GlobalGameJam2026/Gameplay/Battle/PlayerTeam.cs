using System;
using UnityEngine;

[Serializable]
public class PlayerTeam : Team
{
    public override Vector2Int GetLogicalFieldingPosition(Map map)
    {
        return FindAvailablePosition(map, 0, map.Width - 1, 1);
    }
}
