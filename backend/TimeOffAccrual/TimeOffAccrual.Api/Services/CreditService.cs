using Microsoft.EntityFrameworkCore;
using TimeOffAccrual.Api.Data;
using TimeOffAccrual.Api.Dtos;
using TimeOffAccrual.Api.Models;

namespace TimeOffAccrual.Api.Services;

public enum UpdateCreditStatus { Success, NotFound, BelowTaken, Conflict }

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

    public async Task<UpdateCreditResult> UpdateEarnedAsync(int userId, decimal earnedHours, int adminId)
    {
        var balance = await _db.CreditBalances
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (balance is null)
            return new UpdateCreditResult(UpdateCreditStatus.NotFound);

        if (earnedHours < balance.TakenHours)
            return new UpdateCreditResult(UpdateCreditStatus.BelowTaken);

        // Only record a real change
        if (earnedHours != balance.EarnedHours)
        {
            _db.CreditAdjustments.Add(new CreditAdjustment
            {
                AgentUserId = userId,
                AdminUserId = adminId,
                PreviousEarnedHours = balance.EarnedHours,
                NewEarnedHours = earnedHours
            });
            balance.EarnedHours = earnedHours;
            balance.ConcurrencyToken = Guid.NewGuid(); // new version stamp

            try
            {
                await _db.SaveChangesAsync(); // balance change and audit row save together
            }
            catch (DbUpdateConcurrencyException)
            {
                return new UpdateCreditResult(UpdateCreditStatus.Conflict);
            }
        }

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

    public async Task<List<CreditAdjustmentDto>> GetAdjustmentsAsync()
    {
        var rows = await _db.CreditAdjustments
            .Include(a => a.AgentUser)
            .Include(a => a.AdminUser)
            .ToListAsync();

        // Sorted in C# because SQLite can't reliably order DateTime values
        return rows.OrderByDescending(a => a.CreatedAt).Select(a => new CreditAdjustmentDto
        {
            Id = a.Id,
            AgentName = a.AgentUser.Name,
            AdminName = a.AdminUser.Name,
            PreviousEarnedHours = a.PreviousEarnedHours,
            NewEarnedHours = a.NewEarnedHours,
            CreatedAt = a.CreatedAt
        }).ToList();
    }
}