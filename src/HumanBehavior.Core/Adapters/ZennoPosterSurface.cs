using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HumanBehavior.Core.Abstractions;
using HumanBehavior.Core.Models;
using Newtonsoft.Json;

namespace HumanBehavior.Core.Adapters
{
    /// <summary>
    /// Reflection-based adapter that talks to a ZennoPoster Instance via dynamic dispatch.
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
        private readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        /// <param name="instance">ZennoPoster instance object that exposes ActiveTab.</param>
        public ZennoPosterSurface(dynamic instance)
        {
            _instance = instance ?? throw new ArgumentNullException(nameof(instance));
        }

        public Task<IReadOnlyList<UiElement>> GetInteractiveElementsAsync(CancellationToken cancellationToken = default(CancellationToken))
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

            object result = null;
            try
            {
                if (_instance != null && _instance.ActiveTab != null)
                {
                    result = _instance.ActiveTab.ExecuteJs(script);
                }
            }
            catch
            {
                // Ignore dynamic call errors
            }

            var json = result as string ?? (result != null ? result.ToString() : "[]");
            var snapshots = JsonConvert.DeserializeObject<List<ElementSnapshot>>(json, _jsonSettings) ?? new List<ElementSnapshot>();

            var elements = snapshots
                .Where(s => !string.IsNullOrWhiteSpace(s.Id))
                .Select(s =>
                {
                    var bounds = new BoundingBox(s.X, s.Y, s.Width, s.Height);
                    return new UiElement(s.Id, s.Role ?? "element", bounds, importance: s.Importance);
                })
                .Cast<UiElement>()
                .ToList()
                .AsReadOnly();

            return Task.FromResult<IReadOnlyList<UiElement>>(elements);
        }

        public Task ScrollAsync(ScrollPlan plan, InteractionContext context, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var delta = plan.Direction == ScrollDirection.Down ? plan.Pixels : -plan.Pixels;
            var script = string.Format(@"(function(){{
                window.scrollBy({{ top: {0}, behavior: 'smooth' }});
                return 'ok';
            }})();", delta.ToString(CultureInfo.InvariantCulture));

            try
            {
                if (_instance != null && _instance.ActiveTab != null)
                {
                    _instance.ActiveTab.ExecuteJs(script);
                }
            }
            catch
            {
                // Ignore dynamic call errors
            }

            return Task.CompletedTask;
        }

        public Task ClickAsync(UiElement element, InteractionContext context, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ExecutePointerScript(element, @"function(el) {
                const rect = el.getBoundingClientRect();
                const events = ['pointerover', 'pointerdown', 'mousedown', 'pointerup', 'mouseup', 'click'];
                for (const name of events) {
                    el.dispatchEvent(new PointerEvent(name, { bubbles: true, cancelable: true, clientX: rect.left + rect.width/2, clientY: rect.top + rect.height/2 }));
                }
            }");
            return Task.CompletedTask;
        }

        public Task HoverAsync(UiElement element, InteractionContext context, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ExecutePointerScript(element, @"function(el) {
                const rect = el.getBoundingClientRect();
                el.dispatchEvent(new PointerEvent('pointerover', { bubbles: true, cancelable: true, clientX: rect.left + rect.width/2, clientY: rect.top + rect.height/2 }));
                el.dispatchEvent(new PointerEvent('mouseover', { bubbles: true, cancelable: true }));
            }");
            return Task.CompletedTask;
        }

        private void ExecutePointerScript(UiElement element, string actionLambda)
        {
            var safeId = EscapeJsString(element.Id);
            var script = string.Format(@"(function(){{
                const el = document.querySelector('[data-hb-id=""{0}""]');
                if (!el) return 'missing';
                ({1})(el);
                return 'ok';
            }})();", safeId, actionLambda);

            try
            {
                if (_instance != null && _instance.ActiveTab != null)
                {
                    _instance.ActiveTab.ExecuteJs(script);
                }
            }
            catch
            {
                // Ignore dynamic call errors
            }
        }

        private static string EscapeJsString(string value)
        {
            return value
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("'", "\\'");
        }

        private sealed class ElementSnapshot
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("role")]
            public string Role { get; set; }

            [JsonProperty("x")]
            public double X { get; set; }

            [JsonProperty("y")]
            public double Y { get; set; }

            [JsonProperty("width")]
            public double Width { get; set; }

            [JsonProperty("height")]
            public double Height { get; set; }

            [JsonProperty("importance")]
            public double Importance { get; set; } = 1;
        }
    }
}
