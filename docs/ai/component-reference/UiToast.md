# UiToast

A single status message. Standalone rendering requires the caller to handle dismissal and lifetime.

Namespace `ChampUI.Components.Feedback`. Generic parameters: none. [Razor source](../../../src/ChampUI/Components/Feedback/UiToast.razor).

| Parameter | C# type | Supply | Initial value |
| --- | --- | --- | --- |
| `Message` | `ToastMessage` | required | `default!` |
| `DismissLabel` | `string` | optional | `"Dismiss"` |
| `OnDismiss` | `EventCallback<MouseEventArgs>` | optional | `default (no delegate)` |

Callbacks: `OnDismiss`. Child slots: none.

## Example

```razor
<UiToast Message="@(new ToastMessage { Text = "Employee saved", Kind = ToastKind.Success })" />
```
