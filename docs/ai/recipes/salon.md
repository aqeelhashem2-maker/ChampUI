# Salon services management

Maintain a session-local service catalog. Require a name and a positive duration. The [inline editing recipe](../grid.md) shows how to edit existing records. Apply the same rules on the server before storing a service.

```razor
<UiTextField Label="Service" @bind-Value="name" />
<UiNumberField Label="Minutes" Min="1" Step="5" @bind-Value="minutes" />
<UiButton Disabled="@(string.IsNullOrWhiteSpace(name) || !(minutes > 0))" OnClick="Add">Add service</UiButton>
<UiDataGrid TItem="Service" Items="services" RowKeySelector="row => row.Id">
    <Columns>
        <UiColumn TItem="Service" Title="Name" Value="row => row.Name" />
        <UiColumn TItem="Service" Title="Minutes" Value="row => row.Minutes" />
    </Columns>
</UiDataGrid>
@code {
    private sealed record Service(Guid Id, string Name, double Minutes);
    private List<Service> services = [];
    private string? name;
    private double? minutes = 30;
    private void Add() {
        if (string.IsNullOrWhiteSpace(name) || minutes is not > 0) return;
        services = [.. services, new(Guid.NewGuid(), name.Trim(), minutes.Value)];
        name = null;
    }
}
```
