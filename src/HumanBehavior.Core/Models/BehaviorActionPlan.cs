namespace HumanBehavior.Core.Models;

public sealed record BehaviorActionPlan(
    HumanActionType Action,
    UiElement? Target = null,
    ScrollPlan? ScrollPlan = null,
    TimeSpan? Duration = null);

