# Basic appointment list

Filter a list by local business date. In a multi-location system resolve the location timezone before calculating the day. Availability, capacity, recurrence and collision detection belong to the booking service.

```razor
<UiDatePicker Label="Day" @bind-Value="day" />
<UiDataGrid TItem="Visit" Items="@(visits.Where(v => day is null || v.Day == day))" RowKeySelector="v => v.Id">
    <Columns>
        <UiColumn TItem="Visit" Title="Customer" Value="v => v.Name" />
        <UiColumn TItem="Visit" Title="Time" Value="v => v.Time" Format="HH:mm" />
    </Columns>
</UiDataGrid>
@code {
    private sealed record Visit(int Id, DateOnly Day, TimeOnly Time, string Name);
    private DateOnly? day = new DateOnly(2026, 11, 27);
    private readonly Visit[] visits = [new(1, new(2026, 11, 27), new(9, 30), "Sara")];
}
```
