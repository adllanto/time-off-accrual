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
    private readonly CreditService _credits;



    public CreditsController(CreditService credits)
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
        var result = await _credits.UpdateEarnedAsync(userId, dto.EarnedHours);

        if (result.Status == UpdateCreditStatus.NotFound)
            return NotFound(new { message = "No credit balance found for that user." });

        if (result.Status == UpdateCreditStatus.BelowTaken)
            return BadRequest(new { message = "Earned hours cannot be less than hours already taken." });

        return result.Credit!;
    }
}