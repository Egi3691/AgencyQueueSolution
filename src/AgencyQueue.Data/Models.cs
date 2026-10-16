using System.ComponentModel.DataAnnotations;

namespace AgencyQueue.Data;

public sealed class Appointment
{
    public int Id { get; set; }
    [MaxLength(120)] public string CustomerName { get; set; } = "";
    [MaxLength(30)] public string Phone { get; set; } = "";
    public DateOnly AppointmentDate { get; set; }
    public int QueueNumber { get; set; }
    [MaxLength(30)] public string Token { get; set; } = "";
    public DateTime CreatedAtUtc { get; set; }
}

public sealed class ClosedDay
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    [MaxLength(200)] public string Reason { get; set; } = "";
}

public sealed class AgencySettings
{
    public int Id { get; set; } = 1;
    public int DailyCapacity { get; set; } = 20;
}

public sealed record CreateAppointmentRequest(
    [Required, MinLength(2), MaxLength(120)] string CustomerName,
    [Required, MinLength(5), MaxLength(30)] string Phone,
    DateOnly PreferredDate);

public sealed record UpdateSettingsRequest([Range(1, 10000)] int DailyCapacity);
public sealed record AddClosedDayRequest(DateOnly Date, [MaxLength(200)] string Reason);
public sealed record ApiError(string Error);
