# UiToastHost

Observes the scoped ToastService and renders a polite live region. UiAppShell includes one host. Use this standalone pattern outside the shell.

Namespace `ChampUI.Components.Feedback`. Generic parameters: none. [Razor source](../../../src/ChampUI/Components/Feedback/UiToastHost.razor).

This component declares no public parameters.

Callbacks: none. Child slots: none.

## Example

```razor
@inject ToastService Toasts
<UiToastHost />
<UiButton OnClick="Notify">Confirm save</UiButton>
@code { private void Notify() => Toasts.Success("Employee saved"); }
```

Show a toast after the save succeeds. Use ToastService.Show with an explicit TimeSpan when the four-second default is too short. Expiry and Dismiss(Guid) remove the message from the scoped collection. Store actionable validation errors beside the input so recovery remains possible after dismissal.
