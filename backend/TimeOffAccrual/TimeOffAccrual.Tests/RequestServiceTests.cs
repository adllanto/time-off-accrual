using TimeOffAccrual.Api.Dtos;
using TimeOffAccrual.Api.Models;
using TimeOffAccrual.Api.Services;

namespace TimeOffAccrual.Tests;

public class RequestServiceTests
{
    private static DateOnly Future(int daysAhead) =>
        DateOnly.FromDateTime(DateTime.UtcNow).AddDays(daysAhead);

    [Fact]
    public async Task Full_day_request_over_balance_is_rejected()
    {
        // Arrange: agent has only 8 hours available
        using var t = new TestDb();
        var agentId = t.AddAgent("a@test.com", earned: 100);
        var service = new RequestService(t.Db);

        // Act: ask for 2 full days (16 hours)
        var result = await service.CreateAsync(agentId, new CreateRequestDto
        {
            StartDate = Future(5),
            EndDate = Future(6),
            DurationType = DurationType.Full
        });

        // Assert
        Assert.Equal(CreateRequestStatus.InsufficientBalance, result.Status);
    }
}