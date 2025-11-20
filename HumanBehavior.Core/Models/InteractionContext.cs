using System;

namespace HumanBehavior.Core.Models
{
    /// <summary>
    /// Contains telemetry for a single simulated action.
    /// </summary>
    public sealed class InteractionContext
    {
        public string ActionId { get; }
        public DateTimeOffset Timestamp { get; }
        public Point2D PointerTarget { get; }
        public double PointerNoiseInPixels { get; }
        public TimeSpan? IntendedDuration { get; }

        public InteractionContext(string actionId, DateTimeOffset timestamp, Point2D pointerTarget, double pointerNoiseInPixels, TimeSpan? intendedDuration = null)
        {
            ActionId = actionId ?? throw new ArgumentNullException(nameof(actionId));
            Timestamp = timestamp;
            PointerTarget = pointerTarget;
            PointerNoiseInPixels = pointerNoiseInPixels;
            IntendedDuration = intendedDuration;
        }
    }
}
