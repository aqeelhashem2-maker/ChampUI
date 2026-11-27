# Employee management form

Bind typed fields and validate before accepting an edit. This session-only example records a display summary. Replace the assignment with an authorized application service call to persist changes.

```razor
<form @onsubmit="Save" @onsubmit:preventDefault>
    <UiTextField Label="Employee name" @bind-Value="name" Error="@error" />
    <UiSelect TValue="int" Label="Department" Options="departments" @bind-Value="department" />
    <UiSwitch Label="Active" @bind-Checked="active" />
    <UiButton Type="submit">Save employee</UiButton>
</form>
<p role="status">@summary</p>
@code {
    private string? name, error, summary;
    private int department = 1;
    private bool active = true;
    private readonly SelectOption<int>[] departments = [new(1, "Operations"), new(2, "Sales")];
    private void Save() {
        error = string.IsNullOrWhiteSpace(name) ? "Enter the employee name." : null;
        summary = error is null ? $"{name} · department {department} · {(active ? "active" : "inactive")}" : null;
    }
}
```
