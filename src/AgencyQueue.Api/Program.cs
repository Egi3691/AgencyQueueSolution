using AgencyQueue.Data;
using AgencyQueue.Business;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Queue") ?? "Data Source=queue.db"));
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated(); // Demo only; use EF migrations for production.
}
app.UseSwagger();
app.UseSwaggerUI();
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapPost("/api/appointments", async (CreateAppointmentRequest request, IBookingService service, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(request.CustomerName) || request.CustomerName.Trim().Length < 2 ||
        string.IsNullOrWhiteSpace(request.Phone) || request.Phone.Trim().Length < 5)
        return Results.BadRequest(new ApiError("A valid customer name and phone number are required."));
    if (request.PreferredDate == default)
        return Results.BadRequest(new ApiError("Preferred date is required."));
    try
    {
        var booked = await service.BookAsync(request, ct);
        return Results.Created($"/api/appointments/{booked.Id}", booked);
    }
    catch (ArgumentException ex) { return Results.BadRequest(new ApiError(ex.Message)); }
    catch (InvalidOperationException ex) { return Results.Conflict(new ApiError(ex.Message)); }
}).WithTags("Appointments");

app.MapGet("/api/appointments/{id:int}", async (int id, AppDbContext db, CancellationToken ct) =>
    await db.Appointments.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct) is { } a
        ? Results.Ok(a) : Results.NotFound()).WithTags("Appointments");

app.MapGet("/api/queue", async (DateOnly date, AppDbContext db, CancellationToken ct) =>
    Results.Ok(await db.Appointments.AsNoTracking().Where(x => x.AppointmentDate == date)
        .OrderBy(x => x.QueueNumber).ToListAsync(ct))).WithTags("Appointments");

app.MapGet("/api/settings", async (AppDbContext db, CancellationToken ct) =>
    Results.Ok(await db.Settings.AsNoTracking().SingleAsync(ct))).WithTags("Administration");

app.MapPut("/api/settings", async (UpdateSettingsRequest request, AppDbContext db, CancellationToken ct) =>
{
    if (request.DailyCapacity < 1 || request.DailyCapacity > 10000)
        return Results.BadRequest(new ApiError("Daily capacity must be between 1 and 10,000."));
    var settings = await db.Settings.SingleAsync(ct);
    settings.DailyCapacity = request.DailyCapacity;
    await db.SaveChangesAsync(ct);
    return Results.Ok(settings);
}).WithTags("Administration");

app.MapGet("/api/closed-days", async (AppDbContext db, CancellationToken ct) =>
    Results.Ok(await db.ClosedDays.AsNoTracking().OrderBy(x => x.Date).ToListAsync(ct)))
    .WithTags("Administration");

app.MapPost("/api/closed-days", async (AddClosedDayRequest request, AppDbContext db, CancellationToken ct) =>
{
    if (request.Date == default) return Results.BadRequest(new ApiError("Date is required."));
    if (await db.ClosedDays.AnyAsync(x => x.Date == request.Date, ct))
        return Results.Conflict(new ApiError("Date is already closed."));
    if (await db.Appointments.AnyAsync(x => x.AppointmentDate == request.Date, ct))
        return Results.Conflict(new ApiError("Cannot close a date that already has appointments."));
    var closed = new ClosedDay { Date = request.Date, Reason = request.Reason?.Trim() ?? "" };
    db.ClosedDays.Add(closed);
    await db.SaveChangesAsync(ct);
    return Results.Created($"/api/closed-days/{closed.Id}", closed);
}).WithTags("Administration");

app.MapDelete("/api/closed-days/{id:int}", async (int id, AppDbContext db, CancellationToken ct) =>
{
    var closed = await db.ClosedDays.FindAsync([id], ct);
    if (closed is null) return Results.NotFound();
    db.ClosedDays.Remove(closed);
    await db.SaveChangesAsync(ct);
    return Results.NoContent();
}).WithTags("Administration");

app.Run();
public partial class Program { }
