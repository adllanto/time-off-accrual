using Microsoft.EntityFrameworkCore;
using TimeOffAccrual.Api.Data;
using TimeOffAccrual.Api.Dtos;
using TimeOffAccrual.Api.Models;

namespace TimeOffAccrual.Api.Services;

public enum CreateRequestStatus { Success, Invalid, InsufficientBalance, Overlap, NoBalance }

public record CreateRequestResult(CreateRequestStatus Status, string? Message = null, RequestDto? Request = null);

public enum DecisionStatus { Success, NotFound, NotPending, InsufficientBalance, Conflict }

public record DecisionResult(DecisionStatus Status, string? Message = null, RequestDto? Request = null);

public class RequestService
{
    private const decimal HoursPerDay = 8m;
    private const int MaxDays = 365;

    private readonly AppDbContext _db;

    public RequestService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<CreateRequestResult> CreateAsync(int userId, CreateRequestDto dto)
    {
        // 1. Date rules
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        if (dto.EndDate < dto.StartDate)
            return Fail(CreateRequestStatus.Invalid, "End date cannot be before start date.");

        if (dto.StartDate < today)
            return Fail(CreateRequestStatus.Invalid, "Start date cannot be in the past.");

        var days = dto.EndDate.DayNumber - dto.StartDate.DayNumber + 1;
        if (days > MaxDays)
            return Fail(CreateRequestStatus.Invalid, $"A request cannot span more than {MaxDays} days.");

        // 2. Work out the hours on the server
        decimal hours;
        if (dto.DurationType == DurationType.Full)
        {
            hours = HoursPerDay * days; // client-supplied Hours is ignored
        }
        else
        {
            if (dto.StartDate != dto.EndDate)
                return Fail(CreateRequestStatus.Invalid, "Partial day requests must be a single date.");

            if (dto.Hours is null or <= 0 or > HoursPerDay)
                return Fail(CreateRequestStatus.Invalid, "Partial hours must be greater than 0 and at most 8.");

            hours = Math.Round(dto.Hours.Value, 2);
        }

        // 3. Balance check (done in C# because SQLite stores decimals as text)
        var balance = await _db.CreditBalances.FirstOrDefaultAsync(c => c.UserId == userId);
        if (balance is null)
            return Fail(CreateRequestStatus.NoBalance, "No credit balance found for this user.");

        if (hours > balance.EarnedHours - balance.TakenHours)
            return Fail(CreateRequestStatus.InsufficientBalance, "Requested hours exceed your available balance.");

        // 4. Overlap check: Pending or Approved requests for the same user
        var overlaps = await _db.TimeOffRequests.AnyAsync(r =>
            r.UserId == userId &&
            (r.Status == RequestStatus.Pending || r.Status == RequestStatus.Approved) &&
            r.StartDate <= dto.EndDate &&
            r.EndDate >= dto.StartDate);

        if (overlaps)
            return Fail(CreateRequestStatus.Overlap, "You already have a request that overlaps these dates.");

        // 5. Save
        var user = await _db.Users.FindAsync(userId);

        var request = new TimeOffRequest
        {
            UserId = userId,
            User = user!,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            DurationType = dto.DurationType,
            Hours = hours,
            Status = RequestStatus.Pending
        };

        _db.TimeOffRequests.Add(request);
        await _db.SaveChangesAsync();

        return new CreateRequestResult(CreateRequestStatus.Success, Request: ToDto(request));
    }

    public async Task<List<RequestDto>> GetForUserAsync(int userId)
    {
        var requests = await _db.TimeOffRequests
            .Include(r => r.User)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return requests.Select(ToDto).ToList();
    }

    private static CreateRequestResult Fail(CreateRequestStatus status, string message)
        => new(status, message);

    public static RequestDto ToDto(TimeOffRequest r) => new()
    {
        Id = r.Id,
        UserId = r.UserId,
        UserName = r.User.Name,
        StartDate = r.StartDate,
        EndDate = r.EndDate,
        DurationType = r.DurationType,
        Hours = r.Hours,
        Status = r.Status,
        CreatedAt = r.CreatedAt,
        DecidedAt = r.DecidedAt
    };

    public async Task<List<RequestDto>> GetByStatusAsync(RequestStatus status)
    {
        var requests = await _db.TimeOffRequests
            .Include(r => r.User)
            .Where(r => r.Status == status)
            .ToListAsync();

        // Sort in C# (SQLite can't reliably order DateTime values)
        return requests.OrderBy(r => r.CreatedAt).Select(ToDto).ToList();
    }

    public async Task<DecisionResult> ApproveAsync(int requestId, int adminId)
    {
        // Transaction: the status change and the balance change succeed or fail together
        await using var tx = await _db.Database.BeginTransactionAsync();

        var request = await _db.TimeOffRequests
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (request is null)
            return new DecisionResult(DecisionStatus.NotFound, "Request not found.");

        if (request.Status != RequestStatus.Pending)
            return new DecisionResult(DecisionStatus.NotPending, "This request has already been decided.");

        var balance = await _db.CreditBalances.FirstOrDefaultAsync(c => c.UserId == request.UserId);
        if (balance is null || request.Hours > balance.EarnedHours - balance.TakenHours)
            return new DecisionResult(DecisionStatus.InsufficientBalance,
                "The agent no longer has enough available balance for this request.");

        request.Status = RequestStatus.Approved;
        request.DecidedAt = DateTime.UtcNow;
        request.DecidedByUserId = adminId;
        request.ConcurrencyToken = Guid.NewGuid(); // changing it makes a concurrent save fail

        balance.TakenHours += request.Hours;

        try
        {
            await _db.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            // Someone else changed this request at the same time
            return new DecisionResult(DecisionStatus.Conflict,
                "This request was changed by someone else. Please refresh.");
        }

        return new DecisionResult(DecisionStatus.Success, Request: ToDto(request));
    }

    public async Task<DecisionResult> DenyAsync(int requestId, int adminId)
    {
        var request = await _db.TimeOffRequests
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (request is null)
            return new DecisionResult(DecisionStatus.NotFound, "Request not found.");

        if (request.Status != RequestStatus.Pending)
            return new DecisionResult(DecisionStatus.NotPending, "This request has already been decided.");

        request.Status = RequestStatus.Denied;
        request.DecidedAt = DateTime.UtcNow;
        request.DecidedByUserId = adminId;
        request.ConcurrencyToken = Guid.NewGuid();
        // No balance change on denial

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            return new DecisionResult(DecisionStatus.Conflict,
                "This request was changed by someone else. Please refresh.");
        }

        return new DecisionResult(DecisionStatus.Success, Request: ToDto(request));
    }
}