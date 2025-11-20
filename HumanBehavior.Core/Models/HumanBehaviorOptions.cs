using System;

namespace HumanBehavior.Core.Models
{
    /// <summary>
    /// Tunable knobs for the simulator so you can match different personas.
    /// </summary>
    public sealed class HumanBehaviorOptions
    {
        public double ClickBias { get; set; } = 0.4;
        public double ScrollBias { get; set; } = 0.3;
        public double HoverBias { get; set; } = 0.2;
        public double PauseBias { get; set; } = 0.1;

        public TimeSpan MinPauseBetweenActions { get; set; } = TimeSpan.FromMilliseconds(900);
        public TimeSpan MaxPauseBetweenActions { get; set; } = TimeSpan.FromMilliseconds(2400);

        public Tuple<double, double> ScrollAmountPixels { get; set; } = new Tuple<double, double>(280, 1080);
        public TimeSpan MinScrollDuration { get; set; } = TimeSpan.FromMilliseconds(400);
        public TimeSpan MaxScrollDuration { get; set; } = TimeSpan.FromMilliseconds(1200);

        public TimeSpan MinHoverDuration { get; set; } = TimeSpan.FromMilliseconds(800);
        public TimeSpan MaxHoverDuration { get; set; } = TimeSpan.FromMilliseconds(2500);

        public double PointerNoiseInPixels { get; set; } = 8;
        public int MaxActionsPerRun { get; set; } = 25;
        public TimeSpan? MaxRunDuration { get; set; } = TimeSpan.FromSeconds(45);

        public bool AllowFallbackScrollWhenNoElements { get; set; } = true;

        internal Tuple<double, double, double, double> NormalizedWeights()
        {
            var sum = ClickBias + HoverBias + ScrollBias + PauseBias;
            if (sum <= 0)
            {
                return new Tuple<double, double, double, double>(0.4, 0.2, 0.3, 0.1);
            }

            return new Tuple<double, double, double, double>(
                ClickBias / sum,
                HoverBias / sum,
                ScrollBias / sum,
                PauseBias / sum);
        }
    }
}
