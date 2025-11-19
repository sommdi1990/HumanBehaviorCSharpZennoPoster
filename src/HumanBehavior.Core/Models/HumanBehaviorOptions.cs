namespace HumanBehavior.Core.Models;

/// <summary>
/// Tunable knobs for the simulator so you can match different personas.
/// </summary>
public sealed class HumanBehaviorOptions
{
    public double ClickBias { get; init; } = 0.4;
    public double ScrollBias { get; init; } = 0.3;
    public double HoverBias { get; init; } = 0.2;
    public double PauseBias { get; init; } = 0.1;

    public TimeSpan MinPauseBetweenActions { get; init; } = TimeSpan.FromMilliseconds(900);
    public TimeSpan MaxPauseBetweenActions { get; init; } = TimeSpan.FromMilliseconds(2400);

    public (double Min, double Max) ScrollAmountPixels { get; init; } = (280, 1080);
    public TimeSpan MinScrollDuration { get; init; } = TimeSpan.FromMilliseconds(400);
    public TimeSpan MaxScrollDuration { get; init; } = TimeSpan.FromMilliseconds(1200);

    public TimeSpan MinHoverDuration { get; init; } = TimeSpan.FromMilliseconds(800);
    public TimeSpan MaxHoverDuration { get; init; } = TimeSpan.FromMilliseconds(2500);

    public double PointerNoiseInPixels { get; init; } = 8;
    public int MaxActionsPerRun { get; init; } = 25;
    public TimeSpan? MaxRunDuration { get; init; } = TimeSpan.FromSeconds(45);

    public bool AllowFallbackScrollWhenNoElements { get; init; } = true;

    internal (double Click, double Hover, double Scroll, double Pause) NormalizedWeights()
    {
        var sum = ClickBias + HoverBias + ScrollBias + PauseBias;
        if (sum <= 0)
        {
            return (0.4, 0.2, 0.3, 0.1);
        }

        return (ClickBias / sum, HoverBias / sum, ScrollBias / sum, PauseBias / sum);
    }
}

