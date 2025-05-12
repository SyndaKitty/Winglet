using System.Numerics;

public struct Vector2Int : IEquatable<Vector2Int>
{
    public int X;
    public int Y;

    public float Length => MathF.Sqrt((float)X * X + Y * Y);

    public Vector2Int(int x, int y)
    {
        X = x;
        Y = y;
    }

    public Vector2Int(Vector2 v)
    {
        X = (int)v.X;
        Y = (int)v.Y;
    }

    public void Set(int x, int y)
    {
        X = x;
        Y = y;
    }

    public override bool Equals(object other)
    {
        if (other is Vector2Int v)
        {
            return Equals(v);
        }    
        return false;
    }

    public bool Equals(Vector2Int other)
    {
        return X == other.X && Y == other.Y;
    }

    public override int GetHashCode()
    {
        const int p1 = 73856093;
        const int p2 = 83492791;
        return (X * p1) ^ (Y * p2);
    }

    public override string ToString()
    {
        return $"({X}, {Y})";
    }

    public static readonly Vector2Int Zero = new Vector2Int(0, 0);
    public static readonly Vector2Int One = new Vector2Int(1, 1);
    public static readonly Vector2Int Up = new Vector2Int(0, 1);
    public static readonly Vector2Int Down = new Vector2Int(0, -1);
    public static readonly Vector2Int Left = new Vector2Int(-1, 0);
    public static readonly Vector2Int Right = new Vector2Int(1, 0);

    public static float Distance(Vector2Int a, Vector2Int b)
    {
        float dx = a.X - b.X;
        float dy = a.Y - b.Y;

        return (float)Math.Sqrt(dx * dx + dy * dy);
    }

    public static implicit operator Vector2(Vector2Int v)
    {
        return new Vector2(v.X, v.Y);
    }

    public static Vector2Int operator -(Vector2Int v)
    {
        return new Vector2Int(-v.X, -v.Y);
    }

    public static Vector2Int operator +(Vector2Int a, Vector2Int b)
    {
        return new Vector2Int(a.X + b.X, a.Y + b.Y);
    }

    public static Vector2Int operator -(Vector2Int a, Vector2Int b)
    {
        return new Vector2Int(a.X - b.X, a.Y - b.Y);
    }

    public static Vector2Int operator *(int a, Vector2Int b)
    {
        return new Vector2Int(a * b.X, a * b.Y);
    }

    public static Vector2Int operator *(Vector2Int a, int b)
    {
        return new Vector2Int(a.X * b, a.Y * b);
    }

    public static Vector2Int operator /(Vector2Int a, int b)
    {
        return new Vector2Int(a.X / b, a.Y / b);
    }

    public static bool operator ==(Vector2Int a, Vector2Int b)
    {
        return a.X == b.X && a.Y == b.Y;
    }

    public static bool operator !=(Vector2Int a, Vector2Int b)
    {
        return !(a == b);
    }
}