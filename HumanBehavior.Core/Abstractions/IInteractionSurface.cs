using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HumanBehavior.Core.Models;

namespace HumanBehavior.Core.Abstractions
{
    /// <summary>
    /// Abstraction over the automation surface (ZennoPoster, Selenium, Playwright, etc.)
    /// that the simulator can use to perform low-level interactions.
    /// </summary>
    public interface IInteractionSurface
    {
        /// <summary>
        /// Returns a snapshot of currently interactive elements on the page.
        /// </summary>
        Task<IReadOnlyList<UiElement>> GetInteractiveElementsAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Performs a scroll gesture using the provided plan.
        /// </summary>
        Task ScrollAsync(ScrollPlan plan, InteractionContext context, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Performs a click (or tap) gesture on the provided element.
        /// </summary>
        Task ClickAsync(UiElement element, InteractionContext context, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Performs a hover or pointer settle on the element.
        /// </summary>
        Task HoverAsync(UiElement element, InteractionContext context, CancellationToken cancellationToken = default(CancellationToken));
    }
}
