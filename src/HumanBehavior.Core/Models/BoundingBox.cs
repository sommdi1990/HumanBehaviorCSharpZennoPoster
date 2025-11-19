namespace HumanBehavior.Core.Models;

/// <summary>
/// Represents the rectangular area of a UI element in pixels.
/// </summary>
public readonly record struct BoundingBox(double X, double Y, double Width, double Height)
{
    public double Right => X + Width;
    public double Bottom => Y + Height;
    public Point2D Center => new(X + Width / 2d, Y + Height / 2d);

    public bool Contains(Point2D point) =>
        point.X >= X &&
        point.Y >= Y &&
        point.X <= Right &&
        point.Y <= Bottom;
}

