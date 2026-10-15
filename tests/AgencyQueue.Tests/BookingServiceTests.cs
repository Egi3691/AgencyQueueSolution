using AgencyQueue.Api;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AgencyQueue.Tests;

public class BookingServiceTests
{
    private static async Task<(SqliteConnection Connection, AppDbContext Db, BookingService Service)> SetupAsync(int capacity = 2)
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        await conn.OpenAsync();
        var opts = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(conn).Options;
        var db = new AppDbContext(opts);
        await db.Database.EnsureCreatedAsync();
        (await db.Settings.SingleAsync()).DailyCapacity = capacity;
        await db.SaveChangesAsync();
        return (conn, db, new BookingService(db, TimeProvider.System));
    }

    [Fact]
    public async Task BookingCreatesSequentialDailyTokens()
    {
        var (conn, db, svc) = await SetupAsync();
        await using (conn) await using (db)
        {
            var day = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2));
            var a = await svc.BookAsync(new("Alice", "123456789", day), default);
            var b = await svc.BookAsync(new("Bob", "123456780", day), default);
            Assert.Equal(1, a.QueueNumber);
            Assert.Equal(2, b.QueueNumber);
            Assert.Equal($"{day:yyyyMMdd}-001", a.Token);
            Assert.Equal($"{day:yyyyMMdd}-002", b.Token);
        }
    }

    [Fact]
    public async Task FullDayRollsOverToFollowingDay()
    {
        var (conn, db, svc) = await SetupAsync(capacity: 1);
        await using (conn) await using (db)
        {
            var day = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2));
            await svc.BookAsync(new("Alice", "123456789", day), default);
            var b = await svc.BookAsync(new("Bob", "123456780", day), default);
            Assert.Equal(day.AddDays(1), b.AppointmentDate);
            Assert.Equal(1, b.QueueNumber);
        }
    }

    [Fact]
    public async Task ClosedDayIsSkipped()
    {
        var (conn, db, svc) = await SetupAsync();
        await using (conn) await using (db)
        {
            var day = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2));
            db.ClosedDays.Add(new ClosedDay { Date = day, Reason = "Holiday" });
            await db.SaveChangesAsync();
            var booking = await svc.BookAsync(new("Alice", "123456789", day), default);
            Assert.Equal(day.AddDays(1), booking.AppointmentDate);
        }
    }

    [Fact]
    public async Task PastDateIsRejected()
    {
        var (conn, db, svc) = await SetupAsync();
        await using (conn) await using (db)
        {
            var day = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2));
            await Assert.ThrowsAsync<ArgumentException>(() =>
                svc.BookAsync(new("Alice", "123456789", day), default));
        }
    }
}
