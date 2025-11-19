namespace HumanBehavior.Core.Models
{
    /// <summary>
    /// Represents a double-precision 2D coordinate.
    /// </summary>
    public struct Point2D
    {
        public double X { get; }
        public double Y { get; }

        public Point2D(double x, double y)
        {
            X = x;
            Y = y;
        }

        public static Point2D operator +(Point2D left, Point2D right)
        {
            return new Point2D(left.X + right.X, left.Y + right.Y);
        }

        public static Point2D operator -(Point2D left, Point2D right)
        {
            return new Point2D(left.X - right.X, left.Y - right.Y);
        }

        public override string ToString()
        {
            return string.Format("({0:0.##}, {1:0.##})", X, Y);
        }
    }
}
