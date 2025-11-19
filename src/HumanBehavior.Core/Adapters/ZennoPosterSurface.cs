using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using HumanBehavior.Core.Abstractions;
using HumanBehavior.Core.Models;

namespace HumanBehavior.Core.Adapters;

/// <summary>
/// Reflection-based adapter that talks to a ZennoPoster <c>Instance</c> via dynamic dispatch.
/// Works both inside ProjectMaker (C# code block) and external DLLs loaded through ZennoPoster.
/// </summary>
/// <remarks>
/// Because this project is built on Linux (without ZennoPoster assemblies), we rely on dynamic calls.
/// At runtime ZennoPoster injects the real COM objects, so the methods this class invokes (ActiveTab, ExecuteJs, etc.)
/// map to the official API.
/// </remarks>
public sealed class ZennoPosterSurface : IInteractionSurface
{
    private readonly dynamic _instance;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    /// <param name="instance">ZennoPoster instance object that exposes <c>ActiveTab</c>.</param>
    public ZennoPosterSurface(dynamic instance)
    {
        _instance = instance ?? throw new ArgumentNullException(nameof(instance));
    }

    public ValueTask<IReadOnlyList<UiElement>> GetInteractiveElementsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var script = @"(function(){
            const selectors = 'a, button, input, textarea, select, [role=""button""], [role=""link""], [onclick], [tabindex]';
            const nodes = Array.from(document.querySelectorAll(selectors))
                .filter(el => el.offsetParent !== null && el.getBoundingClientRect)
                .slice(0, 150)
                .map(el => {
                    const rect = el.getBoundingClientRect();
                    if (!el.dataset.hbId) {
                        el.dataset.hbId = crypto.randomUUID ? crypto.randomUUID() : (Date.now().toString(16) + Math.random().toString(16).slice(2));
                    }
                    const role = el.getAttribute('role')
                        || (el.tagName ? el.tagName.toLowerCase() : 'element');
                    return {
                        id: el.dataset.hbId,
                        role,
                        x: rect.left + window.scrollX,
                        y: rect.top + window.scrollY,
                        width: rect.width,
                        height: rect.height,
                        importance: el.hasAttribute('data-important') ? 2 : 1
                    };
                });
            return JSON.stringify(nodes);
        })();";

        var result = _instance.ActiveTab?.ExecuteJs(script);
        var json = result is string jsonString
            ? jsonString
            : result?.ToString() ?? "[]";
        List<ElementSnapshot> snapshots = JsonSerializer.Deserialize<List<ElementSnapshot>>(json, _jsonOptions)
                                             ?? new List<ElementSnapshot>();

        var elements = snapshots
            .Where(s => !string.IsNullOrWhiteSpace(s.Id))
            .Select(s =>
            {
                var bounds = new BoundingBox(s.X, s.Y, s.Width, s.Height);
                return new UiElement(s.Id!, s.Role ?? "element", bounds, Importance: s.Importance);
            })
            .Cast<UiElement>()
            .ToList()
            .AsReadOnly();

        return new ValueTask<IReadOnlyList<UiElement>>(elements);
    }

    public ValueTask ScrollAsync(ScrollPlan plan, InteractionContext context, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var delta = plan.Direction == ScrollDirection.Down ? plan.Pixels : -plan.Pixels;
        var script = $@"(function(){{
            window.scrollBy({{ top: {delta.ToString(CultureInfo.InvariantCulture)}, behavior: 'smooth' }});
            return 'ok';
        }})();";

        _instance.ActiveTab?.ExecuteJs(script);
        return new ValueTask(Task.CompletedTask);
    }

    public ValueTask ClickAsync(UiElement element, InteractionContext context, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ExecutePointerScript(element, @"function(el) {
            const rect = el.getBoundingClientRect();
            const events = ['pointerover', 'pointerdown', 'mousedown', 'pointerup', 'mouseup', 'click'];
            for (const name of events) {
                el.dispatchEvent(new PointerEvent(name, { bubbles: true, cancelable: true, clientX: rect.left + rect.width/2, clientY: rect.top + rect.height/2 }));
            }
        }");
        return new ValueTask(Task.CompletedTask);
    }

    public ValueTask HoverAsync(UiElement element, InteractionContext context, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ExecutePointerScript(element, @"function(el) {
            const rect = el.getBoundingClientRect();
            el.dispatchEvent(new PointerEvent('pointerover', { bubbles: true, cancelable: true, clientX: rect.left + rect.width/2, clientY: rect.top + rect.height/2 }));
            el.dispatchEvent(new PointerEvent('mouseover', { bubbles: true, cancelable: true }));
        }");
        return new ValueTask(Task.CompletedTask);
    }

    private void ExecutePointerScript(UiElement element, string actionLambda)
    {
        var safeId = EscapeJsString(element.Id);
        var script = $@"(function(){{
            const el = document.querySelector('[data-hb-id=""{safeId}""]');
            if (!el) return 'missing';
            ({actionLambda})(el);
            return 'ok';
        }})();";

        _instance.ActiveTab?.ExecuteJs(script);
    }

    private static string EscapeJsString(string value) =>
        value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("'", "\\'");

    private sealed record ElementSnapshot
    {
        public string? Id { get; init; }
        public string? Role { get; init; }
        public double X { get; init; }
        public double Y { get; init; }
        public double Width { get; init; }
        public double Height { get; init; }
        public double Importance { get; init; } = 1;
    }
}

