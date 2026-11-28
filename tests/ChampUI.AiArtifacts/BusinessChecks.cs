using SalonDashboard.Services;

var store = new SalonStore();
var date = store.Today.AddDays(30).AddHours(9);
var failures = new List<string>();
void Check(bool condition, string name)
{
    Console.WriteLine($"{(condition ? "PASS" : "FAIL")} {name}");
    if (!condition) failures.Add(name);
}

Check(!store.AddBooking(0, 1, 1, date), "missing customer rejected");
Check(!store.AddBooking(1, 0, 1, date), "missing employee rejected");
Check(!store.AddBooking(1, 1, 0, date), "missing service rejected");
Check(!store.AddBooking(1, 1, 1, store.Today.AddDays(-1)), "past date rejected");
var before = store.Revenue(date.Date, date.Date.AddDays(1));
Check(store.AddBooking(1, 1, 1, date), "valid appointment accepted");
var booking = store.Bookings.Last();
Check(store.Revenue(date.Date, date.Date.AddDays(1)) == before, "scheduled appointment excluded from revenue");
Check(!store.AddBooking(1, 2, 1, date), "customer conflict rejected");
Check(!store.AddBooking(2, 1, 1, date), "employee conflict rejected");
store.SetStatus(booking.Id, BookingStatus.Completed);
Check(store.Revenue(date.Date, date.Date.AddDays(1)) == before + booking.Price, "completed service recognized on appointment date");
store.SetStatus(booking.Id, BookingStatus.Completed);
Check(store.Revenue(date.Date, date.Date.AddDays(1)) == before + booking.Price, "repeated completion does not duplicate revenue");
Check(store.Revenue(store.Today, store.Today.AddDays(1)) == store.Bookings.Where(x => x.Status == BookingStatus.Completed && x.Start.Date == store.Today).Sum(x => x.Price), "future completion excluded from today's revenue");
return failures.Count == 0 ? 0 : 1;
