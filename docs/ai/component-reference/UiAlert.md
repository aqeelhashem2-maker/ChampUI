# UiAlert

Dismissible enables the close control. Supply a translated DismissLabel when localizing.

Namespace `ChampUI.Components.Feedback`. Generic parameters: none. [Razor source](../../../src/ChampUI/Components/Feedback/UiAlert.razor).

| Parameter | C# type | Supply | Initial value |
| --- | --- | --- | --- |
| `Variant` | `UiVariant` | optional | `UiVariant.Info` |
| `Title` | `string?` | optional | `null` |
| `Dismissible` | `bool` | optional | `false` |
| `DismissLabel` | `string` | optional | `"Dismiss"` |
| `Class` | `string?` | optional | `null` |
| `OnDismiss` | `EventCallback` | optional | `default (no delegate)` |
| `ChildContent` | `RenderFragment?` | optional | `null` |

Callbacks: `OnDismiss`. Child slots: `ChildContent`.

## Example

```razor
<UiAlert Variant="UiVariant.Warning" Title="Schedule changed">Review your appointments.</UiAlert>
```
