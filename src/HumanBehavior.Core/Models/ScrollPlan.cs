using System;

namespace HumanBehavior.Core.Models
{
    public enum ScrollDirection
    {
        Up,
        Down
    }

    /// <summary>
    /// Encapsulates a scroll amount and direction, plus optional pauses.
    /// </summary>
    public sealed class ScrollPlan
    {
        public ScrollDirection Direction { get; }
        public double Pixels { get; }
        public TimeSpan Duration { get; }

        public ScrollPlan(ScrollDirection direction, double pixels, TimeSpan duration)
        {
            Direction = direction;
            Pixels = pixels;
            Duration = duration;
        }

        public override string ToString()
        {
            return string.Format("{0} {1:0}px", Direction, Pixels);
        }
    }
}
