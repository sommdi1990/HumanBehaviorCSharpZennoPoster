using System;

namespace HumanBehavior.Core.Models
{
    public sealed class BehaviorActionPlan
    {
        public HumanActionType Action { get; }
        public UiElement Target { get; }
        public ScrollPlan ScrollPlan { get; }
        public TimeSpan? Duration { get; }

        public BehaviorActionPlan(HumanActionType action, UiElement target = null, ScrollPlan scrollPlan = null, TimeSpan? duration = null)
        {
            Action = action;
            Target = target;
            ScrollPlan = scrollPlan;
            Duration = duration;
        }
    }
}
