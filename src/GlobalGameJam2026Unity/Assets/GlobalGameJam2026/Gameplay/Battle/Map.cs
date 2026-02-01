using GlobalGameJam2026.Gameplay.Overworld.PlayerController;
using System.Collections.Generic;
using UnityEngine;

public class Map
{
    public BattleTile[,] tiles { get; set; }

    public int Width => tiles.GetLength(0);
    public int Height => tiles.GetLength(1);

    public Map(int width, int height)
    {
        tiles = new BattleTile[width, height];
    }

    public BattleTile this[int x, int y]
    {
        get
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                return null;

            return tiles[x, y];
        }
        set
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
                tiles[x, y] = value;
        }
    }

    public BattleTile[] GetLine(Vector2Int origin, OrthogonalDirection direction, int length, bool includeOrigin)
    {
        List<BattleTile> results = new List<BattleTile>();
        int start = includeOrigin ? 0 : 1;

        for (int i = start; i <= length; i++)
        {
            int targetX = origin.x + (direction.X * i);
            int targetY = origin.y + (direction.Y * i);

            BattleTile tile = this[targetX, targetY];
            if (tile != null) results.Add(tile);
        }

        return results.ToArray();
    }

    public BattleTile[] GetCircle(Vector2Int origin, float radius, bool includeOrigin)
    {
        List<BattleTile> results = new List<BattleTile>();
        int ceilRadius = Mathf.CeilToInt(radius);

        for (int x = -ceilRadius; x <= ceilRadius; x++)
        {
            for (int y = -ceilRadius; y <= ceilRadius; y++)
            {
                if (!includeOrigin && x == 0 && y == 0) continue;

                if (x * x + y * y <= radius * radius)
                {
                    BattleTile tile = this[origin.x + x, origin.y + y];
                    if (tile != null) results.Add(tile);
                }
            }
        }
        return results.ToArray();
    }

    public BattleTile[] GetDiamond(Vector2Int origin, int distance, bool includeOrigin)
    {
        List<BattleTile> results = new List<BattleTile>();

        for (int x = -distance; x <= distance; x++)
        {
            for (int y = -distance; y <= distance; y++)
            {
                if (!includeOrigin && x == 0 && y == 0) continue;

                // Manhattan distance check
                if (Mathf.Abs(x) + Mathf.Abs(y) <= distance)
                {
                    BattleTile tile = this[origin.x + x, origin.y + y];
                    if (tile != null) results.Add(tile);
                }
            }
        }
        return results.ToArray();
    }
}
