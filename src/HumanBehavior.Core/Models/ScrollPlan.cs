namespace HumanBehavior.Core.Models;

public enum ScrollDirection
{
    Up,
    Down
}

/// <summary>
/// Encapsulates a scroll amount and direction, plus optional pauses.
/// </summary>
public sealed record ScrollPlan(ScrollDirection Direction, double Pixels, TimeSpan Duration)
{
    public override string ToString() => $"{Direction} {Pixels:0}px";
}

