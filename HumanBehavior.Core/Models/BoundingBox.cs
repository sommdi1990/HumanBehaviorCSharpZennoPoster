namespace HumanBehavior.Core.Models
{
    /// <summary>
    /// Represents the rectangular area of a UI element in pixels.
    /// </summary>
    public struct BoundingBox
    {
        public double X { get; }
        public double Y { get; }
        public double Width { get; }
        public double Height { get; }

        public BoundingBox(double x, double y, double width, double height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public double Right => X + Width;
        public double Bottom => Y + Height;
        public Point2D Center => new Point2D(X + Width / 2.0, Y + Height / 2.0);

        public bool Contains(Point2D point)
        {
            return point.X >= X &&
                   point.Y >= Y &&
                   point.X <= Right &&
                   point.Y <= Bottom;
        }
    }
}
