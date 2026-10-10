namespace TimeOffAccrual.Api.Models;

public class TimeOffRequest
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

    public DurationType DurationType { get; set; }
    public decimal Hours { get; set; }

    public RequestStatus Status { get; set; } = RequestStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DecidedAt { get; set; }
    public int? DecidedByUserId { get; set; }

    public Guid ConcurrencyToken { get; set; } = Guid.NewGuid();
}