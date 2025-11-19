namespace HumanBehavior.Core.Models;

/// <summary>
/// Contains telemetry for a single simulated action.
/// </summary>
public sealed record InteractionContext(
    string ActionId,
    DateTimeOffset Timestamp,
    Point2D PointerTarget,
    double PointerNoiseInPixels,
    TimeSpan? IntendedDuration = null);

