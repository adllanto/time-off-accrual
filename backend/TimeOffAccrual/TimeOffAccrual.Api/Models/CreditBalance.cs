namespace TimeOffAccrual.Api.Models;

public class CreditBalance
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public decimal EarnedHours { get; set; }
    public decimal TakenHours { get; set; }

    // Computed, never stored in the database
    public decimal AvailableHours => EarnedHours - TakenHours;
}