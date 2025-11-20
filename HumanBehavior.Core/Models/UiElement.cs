using System;
using System.Collections.Generic;

namespace HumanBehavior.Core.Models
{
    /// <summary>
    /// Lightweight snapshot of an interactive element that can be targeted by the simulator.
    /// </summary>
    public sealed class UiElement
    {
        public string Id { get; }
        public string Role { get; }
        public BoundingBox Bounds { get; }
        public bool IsVisible { get; }
        public double Importance { get; }
        public IReadOnlyDictionary<string, string> Metadata { get; }

        public UiElement(string id, string role, BoundingBox bounds, bool isVisible = true, double importance = 1, IReadOnlyDictionary<string, string> metadata = null)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Role = role ?? throw new ArgumentNullException(nameof(role));
            Bounds = bounds;
            IsVisible = isVisible;
            Importance = importance;
            Metadata = metadata;
        }

        public Point2D Center => Bounds.Center;

        public bool IsClickable =>
            IsVisible &&
            (Role.Equals("button", StringComparison.OrdinalIgnoreCase) ||
             Role.Equals("link", StringComparison.OrdinalIgnoreCase) ||
             Role.Equals("input", StringComparison.OrdinalIgnoreCase) ||
             Role.Equals("card", StringComparison.OrdinalIgnoreCase));

        public double Weight => Math.Max(0.05, Importance);
    }
}
