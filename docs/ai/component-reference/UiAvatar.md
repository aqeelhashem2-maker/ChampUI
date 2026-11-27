# UiAvatar

Supply a name for the initials fallback and an optional image URL.

Namespace `ChampUI.Components.Display`. Generic parameters: none. [Razor source](../../../src/ChampUI/Components/Display/UiAvatar.razor).

| Parameter | C# type | Supply | Initial value |
| --- | --- | --- | --- |
| `Name` | `string` | required | `""` |
| `ImageUrl` | `string?` | optional | `null` |
| `Size` | `UiSize` | optional | `UiSize.Medium` |
| `Status` | `string?` | optional | `null` |
| `Class` | `string?` | optional | `null` |

Callbacks: none. Child slots: none.

## Example

```razor
<UiAvatar Name="Sara Ali" />
```
