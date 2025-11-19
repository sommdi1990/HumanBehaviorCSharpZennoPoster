namespace HumanBehavior.Core.Models;

/// <summary>
/// Represents a double-precision 2D coordinate.
/// </summary>
public readonly record struct Point2D(double X, double Y)
{
    public static Point2D operator +(Point2D left, Point2D right) => new(left.X + right.X, left.Y + right.Y);

    public static Point2D operator -(Point2D left, Point2D right) => new(left.X - right.X, left.Y - right.Y);

    public override string ToString() => $"({X:0.##}, {Y:0.##})";
}

