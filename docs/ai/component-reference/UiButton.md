# UiButton

Save a form with a native submit button. The default button type is button.

Namespace `ChampUI.Components.Actions`. Generic parameters: none. [Razor source](../../../src/ChampUI/Components/Actions/UiButton.razor).

| Parameter | C# type | Supply | Initial value |
| --- | --- | --- | --- |
| `Variant` | `UiVariant` | optional | `UiVariant.Primary` |
| `Size` | `UiSize` | optional | `UiSize.Medium` |
| `Type` | `string` | optional | `"button"` |
| `Icon` | `string?` | optional | `null` |
| `Loading` | `bool` | optional | `false` |
| `Disabled` | `bool` | optional | `false` |
| `Class` | `string?` | optional | `null` |
| `OnClick` | `EventCallback<MouseEventArgs>` | optional | `default (no delegate)` |
| `ChildContent` | `RenderFragment?` | optional | `null` |
| `Attributes` | `Dictionary<string, object>?` | captures HTML attributes | `null` |

Callbacks: `OnClick`. Child slots: `ChildContent`.

## Example

```razor
<UiButton Type="submit">Save employee</UiButton>
```
