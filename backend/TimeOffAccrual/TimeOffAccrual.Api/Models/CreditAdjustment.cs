namespace TimeOffAccrual.Api.Models;

public class CreditAdjustment
{
    public int Id { get; set; }

    public int AgentUserId { get; set; }
    public User AgentUser { get; set; } = null!;

    public int AdminUserId { get; set; }
    public User AdminUser { get; set; } = null!;

    public decimal PreviousEarnedHours { get; set; }
    public decimal NewEarnedHours { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}