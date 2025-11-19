namespace HumanBehavior.Core.Models;

/// <summary>
/// Lightweight snapshot of an interactive element that can be targeted by the simulator.
/// </summary>
public sealed record UiElement(
    string Id,
    string Role,
    BoundingBox Bounds,
    bool IsVisible = true,
    double Importance = 1,
    IReadOnlyDictionary<string, string>? Metadata = null)
{
    public Point2D Center => Bounds.Center;

    public bool IsClickable =>
        IsVisible &&
        (Role.Equals("button", StringComparison.OrdinalIgnoreCase) ||
         Role.Equals("link", StringComparison.OrdinalIgnoreCase) ||
         Role.Equals("input", StringComparison.OrdinalIgnoreCase) ||
         Role.Equals("card", StringComparison.OrdinalIgnoreCase));

    public double Weight => Math.Max(0.05, Importance);
}

