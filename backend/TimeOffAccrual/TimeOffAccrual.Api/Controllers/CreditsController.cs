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

        return result.Status switch
        {
            UpdateCreditStatus.Success => result.Credit!,
            UpdateCreditStatus.NotFound => NotFound(new { message = "No credit balance found for that user." }),
            UpdateCreditStatus.BelowTaken => BadRequest(new { message = "Earned hours cannot be less than hours already taken." }),
            UpdateCreditStatus.Conflict => Conflict(new { message = "This balance was changed by someone else. Please refresh and try again." }),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    [HttpGet("adjustments")]
    public async Task<ActionResult<List<CreditAdjustmentDto>>> GetAdjustments()
    {
        return await _credits.GetAdjustmentsAsync();
    }
}