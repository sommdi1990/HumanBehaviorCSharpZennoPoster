using System.Threading.Tasks;
using HumanBehavior.Core.Models;

namespace HumanBehavior.Core.Abstractions;

/// <summary>
/// Abstraction over the automation surface (ZennoPoster, Selenium, Playwright, etc.)
/// that the simulator can use to perform low-level interactions.
/// </summary>
public interface IInteractionSurface
{
    /// <summary>
    /// Returns a snapshot of currently interactive elements on the page.
    /// </summary>
    ValueTask<IReadOnlyList<UiElement>> GetInteractiveElementsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a scroll gesture using the provided plan.
    /// </summary>
    ValueTask ScrollAsync(ScrollPlan plan, InteractionContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a click (or tap) gesture on the provided element.
    /// </summary>
    ValueTask ClickAsync(UiElement element, InteractionContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a hover or pointer settle on the element.
    /// </summary>
    ValueTask HoverAsync(UiElement element, InteractionContext context, CancellationToken cancellationToken = default);
}

