# Customer list and details

Select Details with the keyboard or pointer. Keep the launcher in the grid while the dialog is open. Load authorized details by stable ID in a real directory.

```razor
<UiDataGrid TItem="Customer" Items="customers" RowKeySelector="item => item.Id">
    <Columns>
        <UiColumn TItem="Customer" Title="Customer" Value="item => item.Name" />
        <UiColumn TItem="Customer" Title="Actions" Sortable="false">
            <Template Context="item"><UiButton OnClick="() => Show(item)">Details</UiButton></Template>
        </UiColumn>
    </Columns>
</UiDataGrid>
<UiDialog Title="Customer details" @bind-Visible="open">@current?.Name · @current?.Email</UiDialog>
@code {
    private sealed record Customer(int Id, string Name, string Email);
    private readonly Customer[] customers = [new(1, "Sara", "sara@example.com")];
    private Customer? current;
    private bool open;
    private void Show(Customer item) { current = item; open = true; }
}
```
