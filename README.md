# HumanBehavior (ZennoPoster Helper)

Simulation helpers to bring more human-like randomness (scrolls, clicks, hovers and idle pauses) into ZennoPoster automations. The resulting DLL can be referenced directly from ProjectMaker scripts or any other .NET host.

## Quick start

```bash
# From the workspace root
/home/soroush/.dotnet/dotnet build
```

The build produces `HumanBehavior.dll` for both `netstandard2.0` and `net8.0` under `src/HumanBehavior.Core/bin/Debug`.

## Project layout

| Path | Description |
| --- | --- |
| `src/HumanBehavior.Core` | Multi-targeted class library (netstandard2.0 + net8.0). |
| `Abstractions/IInteractionSurface.cs` | Interface that bridges the simulator with ZennoPoster (or any automation runtime). |
| `HumanBehaviorSimulator.cs` | Orchestrates randomized action plans. |
| `Models/*` | Lightweight DTOs describing UI elements, scroll plans and tuning knobs. |
| `Internal/*` | Randomness helpers and the action planner. |

## Usage

1. Implement `IInteractionSurface` using ZennoPoster APIs (buttons discovery, scrolling, clicking). ZennoPoster already exposes browser automation primitives, so the adapter simply forwards simulator requests to ProjectMaker blocks. [ZennoPoster automates anything a user can do in a browser, which makes it ideal for this type of adapter](https://zennolab.com/en/products/zennoposter/).
2. Instantiate the simulator (now that `ZennoPosterSurface` exists):

```csharp
var surface = new ZennoPosterSurface(instance); // your implementation
var options = new HumanBehaviorOptions
{
    ClickBias = 0.45,
    ScrollBias = 0.25,
    HoverBias = 0.2,
    PauseBias = 0.1,
};

var simulator = new HumanBehaviorSimulator(surface, options, Console.WriteLine);
await simulator.RunAsync(actionBudget: 15, cancellationToken);
```

3. Ship the compiled `HumanBehavior.dll` alongside your ZennoPoster project and load it through the C# code block inside ProjectMaker or via the `RunDll` action.

## Customizing behavior

- `HumanBehaviorOptions` lets you tweak weights, jitter, and durations to mimic different personas (slow reader, speedy scroller, etc.).
- Implementations can feed richer metadata into each `UiElement` (roles, labels, confidence scores) so the planner favors meaningful targets.
- Use the diagnostics sink to pipe traces into ZennoPoster logs for debugging sequences.

## Testing locally (Linux)

The repo bundles a local .NET 8 SDK under `~/.dotnet`. If you need a clean rebuild:

```bash
rm -rf /home/soroush/CSharp/HumanBehavior/src/HumanBehavior.Core/bin \
       /home/soroush/CSharp/HumanBehavior/src/HumanBehavior.Core/obj
/home/soroush/.dotnet/dotnet build
```

To produce a release DLL:

```bash
/home/soroush/.dotnet/dotnet build --configuration Release
```

