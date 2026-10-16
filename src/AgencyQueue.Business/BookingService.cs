using AgencyQueue.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace AgencyQueue.Business;

public interface IBookingService
{
    Task<Appointment> BookAsync(CreateAppointmentRequest request, CancellationToken ct);
}

public sealed class BookingService(AppDbContext db, TimeProvider clock) : IBookingService
{
    public async Task<Appointment> BookAsync(CreateAppointmentRequest request, CancellationToken ct)
    {
        // SQLite has one writer at a time. Retry transient contention and unique-index races.
        for (var retry = 0; retry < 5; retry++)
        {
            try
            {
                db.ChangeTracker.Clear();
                return await BookOnceAsync(request, ct);
            }
            catch (DbUpdateException ex) when (IsRetryable(ex) && retry < 4)
            {
                db.ChangeTracker.Clear();
                await Task.Delay(40 * (retry + 1), ct);
            }
            catch (SqliteException ex) when (IsRetryable(ex) && retry < 4)
            {
                db.ChangeTracker.Clear();
                await Task.Delay(40 * (retry + 1), ct);
            }
        }
        throw new InvalidOperationException("Could not reserve a queue token. Please retry.");
    }

    private async Task<Appointment> BookOnceAsync(CreateAppointmentRequest request, CancellationToken ct)
    {
        var capacity = (await db.Settings.AsNoTracking().SingleAsync(ct)).DailyCapacity;
        var closed = (await db.ClosedDays.AsNoTracking().Select(x => x.Date).ToListAsync(ct)).ToHashSet();
        var date = request.PreferredDate;
        var todayUtc = DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime);
        if (date < todayUtc) throw new ArgumentException("Preferred date cannot be in the past (UTC).");

        // Choose the earliest open day with remaining capacity, within a one-year horizon.
        for (int day = 0; day < 366; day++, date = date.AddDays(1))
        {
            if (closed.Contains(date)) continue;
            var maxQueue = await db.Appointments.Where(x => x.AppointmentDate == date)
                .Select(x => (int?)x.QueueNumber).MaxAsync(ct) ?? 0;
            if (maxQueue >= capacity) continue;

            var appointment = new Appointment
            {
                CustomerName = request.CustomerName.Trim(),
                Phone = request.Phone.Trim(),
                AppointmentDate = date,
                QueueNumber = maxQueue + 1,
                Token = $"{date:yyyyMMdd}-{maxQueue + 1:D3}",
                CreatedAtUtc = clock.GetUtcNow().UtcDateTime
            };
            db.Appointments.Add(appointment);
            await db.SaveChangesAsync(ct);
            return appointment;
        }
        throw new InvalidOperationException("No available appointment date in the next 366 days.");
    }

    private static bool IsRetryable(DbUpdateException e) =>
        e.InnerException is SqliteException se && IsRetryable(se);
    private static bool IsRetryable(SqliteException e) => e.SqliteErrorCode is 5 or 6 or 19;
}
