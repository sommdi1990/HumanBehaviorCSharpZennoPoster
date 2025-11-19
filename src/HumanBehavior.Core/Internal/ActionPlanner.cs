using HumanBehavior.Core.Models;

namespace HumanBehavior.Core.Internal;

internal sealed class ActionPlanner
{
    private readonly HumanBehaviorOptions _options;

    public ActionPlanner(HumanBehaviorOptions options)
    {
        _options = options;
    }

    public BehaviorActionPlan CreatePlan(IReadOnlyList<UiElement> elements)
    {
        if (elements.Count == 0 && !_options.AllowFallbackScrollWhenNoElements)
        {
            return BuildPausePlan();
        }

        if (elements.Count == 0)
        {
            return BuildScrollPlan();
        }

        var weights = _options.NormalizedWeights();
        var roll = Entropy.NextDouble();

        if (roll < weights.Click)
        {
            return BuildClickPlan(elements);
        }

        if (roll < weights.Click + weights.Hover)
        {
            return BuildHoverPlan(elements);
        }

        if (roll < weights.Click + weights.Hover + weights.Scroll)
        {
            return BuildScrollPlan();
        }

        return BuildPausePlan();
    }

    private BehaviorActionPlan BuildClickPlan(IReadOnlyList<UiElement> elements)
    {
        var element = SelectElement(elements, preferClickable: true);
        return new BehaviorActionPlan(HumanActionType.Click, element);
    }

    private BehaviorActionPlan BuildHoverPlan(IReadOnlyList<UiElement> elements)
    {
        var element = SelectElement(elements, preferClickable: false);
        var duration = Entropy.NextTimeSpan(_options.MinHoverDuration, _options.MaxHoverDuration);
        return new BehaviorActionPlan(HumanActionType.Hover, element, Duration: duration);
    }

    private BehaviorActionPlan BuildScrollPlan()
    {
        var pixels = Entropy.NextDouble(_options.ScrollAmountPixels.Min, _options.ScrollAmountPixels.Max);
        var direction = Entropy.NextDouble() > 0.5 ? ScrollDirection.Down : ScrollDirection.Up;
        var duration = Entropy.NextTimeSpan(_options.MinScrollDuration, _options.MaxScrollDuration);
        return new BehaviorActionPlan(HumanActionType.Scroll, ScrollPlan: new ScrollPlan(direction, pixels, duration));
    }

    private BehaviorActionPlan BuildPausePlan()
    {
        var duration = Entropy.NextTimeSpan(_options.MinPauseBetweenActions, _options.MaxPauseBetweenActions);
        return new BehaviorActionPlan(HumanActionType.Pause, Duration: duration);
    }

    private static UiElement SelectElement(IReadOnlyList<UiElement> elements, bool preferClickable)
    {
        if (elements.Count == 1)
        {
            return elements[0];
        }

        var filtered = preferClickable
            ? elements.Where(e => e.IsClickable).ToArray()
            : elements.ToArray();

        if (filtered.Length == 0)
        {
            filtered = elements.ToArray();
        }

        var totalWeight = filtered.Sum(e => e.Weight);
        var roll = Entropy.NextDouble(0, totalWeight);
        double cumulative = 0;

        foreach (var element in filtered)
        {
            cumulative += element.Weight;
            if (roll <= cumulative)
            {
                return element;
            }
        }

        return filtered[filtered.Length - 1];
    }
}

