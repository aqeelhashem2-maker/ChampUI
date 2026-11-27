# UiDialog

Bind Visible to parent state. The native modal uses its title and optional description as the accessible name and description. ChampUI JS focuses Close and restores focus on dismissal. Escape or Close invokes VisibleChanged(false) and OnClosed. Setting Visible in the parent does not itself invoke OnClosed.

Namespace `ChampUI.Components.Feedback`. Generic parameters: none. [Razor source](../../../src/ChampUI/Components/Feedback/UiDialog.razor).

| Parameter | C# type | Supply | Initial value |
| --- | --- | --- | --- |
| `Visible` | `bool` | optional | `false` |
| `VisibleChanged` | `EventCallback<bool>` | optional | `default (no delegate)` |
| `Title` | `string` | required | `""` |
| `Description` | `string?` | optional | `null` |
| `Size` | `UiSize` | optional | `UiSize.Medium` |
| `CloseLabel` | `string` | optional | `"Close"` |
| `Class` | `string?` | optional | `null` |
| `ChildContent` | `RenderFragment?` | optional | `null` |
| `Footer` | `RenderFragment?` | optional | `null` |
| `OnClosed` | `EventCallback` | optional | `default (no delegate)` |

Callbacks: `VisibleChanged`, `OnClosed`. Child slots: `ChildContent`, `Footer`.

## Example

```razor
<UiButton OnClick="() => open = true">Review order</UiButton>
<UiDialog @bind-Visible="open" Title="Review order" Description="Confirm the customer and total.">Order 42</UiDialog>
@code { private bool open; }
```

## Focus and dismissal

Keep the launcher focusable and keep the dialog component mounted while its Visible state changes. The browser's native modal isolates the background. The ChampUI script cycles Tab and Shift+Tab in the active dialog, handles nested dialogs, and returns focus to the opener when possible. If the opener was disabled or removed, the script attempts to focus a surviving workflow heading or container. Localize CloseLabel and keep the description short. Verify keyboard navigation and announcements in the generated application. [Dialog browser implementation](../../../src/ChampUI/wwwroot/js/champui.js)
