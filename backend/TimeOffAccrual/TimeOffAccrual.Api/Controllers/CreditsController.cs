using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeOffAccrual.Api.Dtos;
using TimeOffAccrual.Api.Services;

namespace TimeOffAccrual.Api.Controllers;

[ApiController]
[Route("api/credits")]
[Authorize(Roles = "Admin")]


public class CreditsController : ControllerBase
{
    private readonly ICreditService _credits;



    public CreditsController(ICreditService credits)
    {
        _credits = credits;
    }

    [HttpGet]
    public async Task<ActionResult<List<CreditOverviewDto>>> GetAll()
    {
        return await _credits.GetOverviewAsync();
    }

    [HttpPut("{userId:int}")]
    public async Task<ActionResult<CreditOverviewDto>> UpdateEarned(int userId, UpdateEarnedCreditsDto dto)
    {
        var result = await _credits.UpdateEarnedAsync(userId, dto.EarnedHours, User.GetUserId());

        if (result.Status == UpdateCreditStatus.NotFound)
            return NotFound(new { message = "No credit balance found for that user." });

        if (result.Status == UpdateCreditStatus.BelowTaken)
            return BadRequest(new { message = "Earned hours cannot be less than hours already taken." });

        if (result.Status == UpdateCreditStatus.Conflict)
    return Conflict(new { message = "This balance was changed by someone else. Please refresh and try again." });

        return result.Credit!;
    }

    [HttpGet("adjustments")]
    public async Task<ActionResult<List<CreditAdjustmentDto>>> GetAdjustments()
    {
        return await _credits.GetAdjustmentsAsync();
    }
}