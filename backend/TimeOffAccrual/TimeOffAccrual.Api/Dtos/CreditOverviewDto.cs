namespace TimeOffAccrual.Api.Dtos;

public class CreditOverviewDto
{
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal EarnedHours { get; set; }
    public decimal TakenHours { get; set; }
    public decimal AvailableHours { get; set; }
}