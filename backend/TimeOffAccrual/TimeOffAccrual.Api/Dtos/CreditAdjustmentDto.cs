namespace TimeOffAccrual.Api.Dtos;

public class CreditAdjustmentDto
{
    public int Id { get; set; }
    public string AgentName { get; set; } = string.Empty;
    public string AdminName { get; set; } = string.Empty;
    public decimal PreviousEarnedHours { get; set; }
    public decimal NewEarnedHours { get; set; }
    public DateTime CreatedAt { get; set; }
}