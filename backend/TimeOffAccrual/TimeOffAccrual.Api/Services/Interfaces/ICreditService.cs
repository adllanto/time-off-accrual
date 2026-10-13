using TimeOffAccrual.Api.Dtos;

namespace TimeOffAccrual.Api.Services;

public interface ICreditService
{
    Task<List<CreditOverviewDto>> GetOverviewAsync();
    Task<CreditOverviewDto?> GetForUserAsync(int userId);
    Task<UpdateCreditResult> UpdateEarnedAsync(int userId, decimal earnedHours, int adminId);
    Task<List<CreditAdjustmentDto>> GetAdjustmentsAsync();
}