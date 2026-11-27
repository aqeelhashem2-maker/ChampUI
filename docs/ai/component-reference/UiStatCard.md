# UiStatCard

Value and Delta are display strings. Format them for the chosen currency and culture before rendering.

Namespace `ChampUI.Components.Layout`. Generic parameters: none. [Razor source](../../../src/ChampUI/Components/Layout/UiStatCard.razor).

| Parameter | C# type | Supply | Initial value |
| --- | --- | --- | --- |
| `Label` | `string` | required | `""` |
| `Value` | `string` | required | `""` |
| `Icon` | `string?` | optional | `null` |
| `Delta` | `string?` | optional | `null` |
| `Trend` | `UiTrend` | optional | `UiTrend.Neutral` |
| `Hint` | `string?` | optional | `null` |
| `Spark` | `IReadOnlyList<double>?` | optional | `null` |
| `Interactive` | `bool` | optional | `false` |
| `Class` | `string?` | optional | `null` |

Callbacks: none. Child slots: none.

## Example

```razor
<UiStatCard Label="Revenue" Value="$2,400" Delta="+8%" Trend="UiTrend.Up" />
```
