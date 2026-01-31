using System;
using UnityEngine;

namespace GlobalGameJam2026.Gameplay.Overworld.PlayerController
{
    public readonly struct OrthogonalDirection : IEquatable<OrthogonalDirection>
    {
        public static readonly OrthogonalDirection Up = new(0, 1);
        public static readonly OrthogonalDirection Down = new(0, -1);
        public static readonly OrthogonalDirection Left = new(-1, 0);
        public static readonly OrthogonalDirection Right = new(1, 0);

        public readonly int X;
        public readonly int Y;

        private OrthogonalDirection(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Converts a Vector2 to the nearest OrthogonalDirection.
        /// Prioritizes Left/Right over Up/Down at 45 degree angles.
        /// </summary>
        public static OrthogonalDirection FromVector2(Vector2 vector, float buffer = 0.0001f)
        {
            float absX = Mathf.Abs(vector.x);
            float absY = Mathf.Abs(vector.y);

            if (absX + buffer >= absY)
            {
                return vector.x >= 0 ? Right : Left;
            }
            else
            {
                return vector.y >= 0 ? Up : Down;
            }
        }

        /// <summary>
        /// Converts this direction back into a normalized Vector2.
        /// </summary>
        public Vector2 ToVector2()
        {
            return new Vector2(X, Y);
        }

        public override string ToString()
        {
            return $"(X:{X}, Y:{Y})";
        }

        public override bool Equals(object obj)
        {
            return obj is OrthogonalDirection direction && Equals(direction);
        }

        public bool Equals(OrthogonalDirection other)
        {
            return X == other.X &&
                   Y == other.Y;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        public static bool operator ==(OrthogonalDirection left, OrthogonalDirection right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(OrthogonalDirection left, OrthogonalDirection right)
        {
            return !(left == right);
        }
    }
}
