using TimeOffAccrual.Api.Models;

namespace TimeOffAccrual.Api.Dtos;

public class RequestDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public DurationType DurationType { get; set; }
    public decimal Hours { get; set; }
    public RequestStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DecidedAt { get; set; }
}