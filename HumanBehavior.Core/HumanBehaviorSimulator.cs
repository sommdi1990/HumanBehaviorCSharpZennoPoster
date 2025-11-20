using System;
using System.Threading;
using System.Threading.Tasks;
using HumanBehavior.Core.Abstractions;
using HumanBehavior.Core.Internal;
using HumanBehavior.Core.Models;

namespace HumanBehavior.Core
{
    /// <summary>
    /// Coordinates randomized human-like actions (scrolls, hovers, clicks, pauses).
    /// </summary>
    public sealed class HumanBehaviorSimulator
    {
        private readonly IInteractionSurface _surface;
        private readonly HumanBehaviorOptions _options;
        private readonly ActionPlanner _planner;
        private readonly Action<string> _diagnosticsSink;

        public HumanBehaviorSimulator(
            IInteractionSurface surface,
            HumanBehaviorOptions options = null,
            Action<string> diagnosticsSink = null)
        {
            _surface = surface ?? throw new ArgumentNullException(nameof(surface));
            _options = options ?? new HumanBehaviorOptions();
            _planner = new ActionPlanner(_options);
            _diagnosticsSink = diagnosticsSink;
        }

        /// <summary>
        /// Executes a single randomized action.
        /// </summary>
        public Task ExecuteOnceAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return RunAsync(1, cancellationToken);
        }

        /// <summary>
        /// Runs a bounded number of actions.
        /// </summary>
        public async Task RunAsync(int actionBudget, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (actionBudget <= 0)
            {
                return;
            }

            var budget = Math.Min(actionBudget, _options.MaxActionsPerRun);
            for (var i = 0; i < budget; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await ExecuteSingleActionAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Runs actions until the duration elapses (or <see cref="HumanBehaviorOptions.MaxRunDuration"/> is hit).
        /// </summary>
        public async Task RunAsync(TimeSpan duration, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (duration <= TimeSpan.Zero)
            {
                return;
            }

            var requestedDeadline = DateTimeOffset.UtcNow + duration;
            var safetyDeadline = _options.MaxRunDuration.HasValue
                ? DateTimeOffset.UtcNow + TimeSpan.FromTicks(Math.Min(duration.Ticks, _options.MaxRunDuration.Value.Ticks))
                : requestedDeadline;

            while (DateTimeOffset.UtcNow < safetyDeadline)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await ExecuteSingleActionAsync(cancellationToken).ConfigureAwait(false);

                if (DateTimeOffset.UtcNow >= requestedDeadline)
                {
                    break;
                }
            }
        }

        private async Task ExecuteSingleActionAsync(CancellationToken cancellationToken)
        {
            var elements = await _surface.GetInteractiveElementsAsync(cancellationToken).ConfigureAwait(false);
            var plan = _planner.CreatePlan(elements);
            await ExecutePlanAsync(plan, cancellationToken).ConfigureAwait(false);

            if (plan.Action != HumanActionType.Pause)
            {
                await DelayBetweenActionsAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task ExecutePlanAsync(BehaviorActionPlan plan, CancellationToken cancellationToken)
        {
            switch (plan.Action)
            {
                case HumanActionType.Click:
                    if (plan.Target != null)
                    {
                        await _surface.ClickAsync(plan.Target, CreateContext(plan.Target), cancellationToken).ConfigureAwait(false);
                        Trace(string.Format("Click -> {0}", plan.Target.Id));
                    }
                    break;

                case HumanActionType.Hover:
                    if (plan.Target != null)
                    {
                        await _surface.HoverAsync(plan.Target, CreateContext(plan.Target, plan.Duration), cancellationToken).ConfigureAwait(false);
                        var durationMs = plan.Duration.HasValue ? plan.Duration.Value.TotalMilliseconds : 0;
                        Trace(string.Format("Hover -> {0} ({1:0} ms)", plan.Target.Id, durationMs));
                        await DelayAsync(plan.Duration, cancellationToken).ConfigureAwait(false);
                    }
                    break;

                case HumanActionType.Scroll:
                    if (plan.ScrollPlan != null)
                    {
                        var scrollContext = CreateContext(null, plan.ScrollPlan.Duration);
                        await _surface.ScrollAsync(plan.ScrollPlan, scrollContext, cancellationToken).ConfigureAwait(false);
                        Trace(string.Format("Scroll -> {0}", plan.ScrollPlan));
                        await DelayAsync(plan.ScrollPlan.Duration, cancellationToken).ConfigureAwait(false);
                    }
                    break;

                case HumanActionType.Pause:
                    await DelayAsync(plan.Duration, cancellationToken).ConfigureAwait(false);
                    var pauseMs = plan.Duration.HasValue ? plan.Duration.Value.TotalMilliseconds : 0;
                    Trace(string.Format("Pause -> {0:0} ms", pauseMs));
                    break;
            }
        }

        private async Task DelayBetweenActionsAsync(CancellationToken cancellationToken)
        {
            var delay = Entropy.NextTimeSpan(_options.MinPauseBetweenActions, _options.MaxPauseBetweenActions);
            await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
        }

        private static async Task DelayAsync(TimeSpan? duration, CancellationToken cancellationToken)
        {
            if (!duration.HasValue || duration.Value <= TimeSpan.Zero)
            {
                return;
            }

            await Task.Delay(duration.Value, cancellationToken).ConfigureAwait(false);
        }

        private InteractionContext CreateContext(UiElement element, TimeSpan? duration = null)
        {
            var pointerTarget = element != null ? element.Center : new Point2D(Entropy.NextDouble(100, 400), Entropy.NextDouble(180, 600));
            var jittered = ApplyPointerNoise(pointerTarget);
            return new InteractionContext(Guid.NewGuid().ToString("N"), DateTimeOffset.UtcNow, jittered, _options.PointerNoiseInPixels, duration);
        }

        private Point2D ApplyPointerNoise(Point2D point)
        {
            var noise = new Point2D(
                Entropy.NextDouble(-_options.PointerNoiseInPixels, _options.PointerNoiseInPixels),
                Entropy.NextDouble(-_options.PointerNoiseInPixels, _options.PointerNoiseInPixels));

            return point + noise;
        }

        private void Trace(string message)
        {
            if (_diagnosticsSink != null)
            {
                _diagnosticsSink(string.Format("[HumanBehavior] {0}", message));
            }
        }
    }
}
