using Microsoft.EntityFrameworkCore;
using TimeOffAccrual.Api.Data;
using TimeOffAccrual.Api.Dtos;

namespace TimeOffAccrual.Api.Services;

public enum UpdateCreditStatus { Success, NotFound, BelowTaken }

public record UpdateCreditResult(UpdateCreditStatus Status, CreditOverviewDto? Credit = null);

public class CreditService
{
    private readonly AppDbContext _db;

    public CreditService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<CreditOverviewDto>> GetOverviewAsync()
    {
        var balances = await _db.CreditBalances
            .Include(c => c.User)
            .OrderBy(c => c.User.Name)
            .ToListAsync();

        // Available is computed here in C#, never stored
        return balances.Select(c => new CreditOverviewDto
        {
            UserId = c.UserId,
            Name = c.User.Name,
            Email = c.User.Email,
            EarnedHours = c.EarnedHours,
            TakenHours = c.TakenHours,
            AvailableHours = c.EarnedHours - c.TakenHours
        }).ToList();
    }

    public async Task<UpdateCreditResult> UpdateEarnedAsync(int userId, decimal earnedHours)
    {
        var balance = await _db.CreditBalances
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (balance is null)
            return new UpdateCreditResult(UpdateCreditStatus.NotFound);

        // Earned can't drop below what was already taken, or Available would go negative
        if (earnedHours < balance.TakenHours)
            return new UpdateCreditResult(UpdateCreditStatus.BelowTaken);

        balance.EarnedHours = earnedHours;
        await _db.SaveChangesAsync();

        return new UpdateCreditResult(UpdateCreditStatus.Success, new CreditOverviewDto
        {
            UserId = balance.UserId,
            Name = balance.User.Name,
            Email = balance.User.Email,
            EarnedHours = balance.EarnedHours,
            TakenHours = balance.TakenHours,
            AvailableHours = balance.EarnedHours - balance.TakenHours
        });
    }

    public async Task<CreditOverviewDto?> GetForUserAsync(int userId)
    {
        var c = await _db.CreditBalances
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (c is null) return null;

        return new CreditOverviewDto
        {
            UserId = c.UserId,
            Name = c.User.Name,
            Email = c.User.Email,
            EarnedHours = c.EarnedHours,
            TakenHours = c.TakenHours,
            AvailableHours = c.EarnedHours - c.TakenHours
        };
    }
}