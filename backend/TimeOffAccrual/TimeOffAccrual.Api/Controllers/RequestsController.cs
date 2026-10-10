using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeOffAccrual.Api.Dtos;
using TimeOffAccrual.Api.Models;
using TimeOffAccrual.Api.Services;

namespace TimeOffAccrual.Api.Controllers;

[ApiController]
[Route("api/requests")]
[Authorize]
public class RequestsController : ControllerBase
{
    private readonly RequestService _requests;

    public RequestsController(RequestService requests)
    {
        _requests = requests;
    }

    [HttpPost]
    [Authorize(Roles = "Agent")]
    public async Task<ActionResult<RequestDto>> Create(CreateRequestDto dto)
    {
        var userId = User.GetUserId();
        var result = await _requests.CreateAsync(userId, dto);

        return result.Status switch
        {
            CreateRequestStatus.Success => StatusCode(StatusCodes.Status201Created, result.Request),
            CreateRequestStatus.Invalid => BadRequest(new { message = result.Message }),
            CreateRequestStatus.InsufficientBalance => BadRequest(new { message = result.Message }),
            CreateRequestStatus.Overlap => Conflict(new { message = result.Message }),
            CreateRequestStatus.NoBalance => BadRequest(new { message = result.Message }),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    [HttpGet("mine")]
    [Authorize(Roles = "Agent")]
    public async Task<ActionResult<List<RequestDto>>> GetMine()
    {
        return await _requests.GetForUserAsync(User.GetUserId());
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<RequestDto>>> GetByStatus([FromQuery] RequestStatus status = RequestStatus.Pending)
    {
        return await _requests.GetByStatusAsync(status);
    }

    [HttpPost("{id:int}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<RequestDto>> Approve(int id)
    {
        var result = await _requests.ApproveAsync(id, User.GetUserId());
        return ToActionResult(result);
    }

    [HttpPost("{id:int}/deny")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<RequestDto>> Deny(int id)
    {
        var result = await _requests.DenyAsync(id, User.GetUserId());
        return ToActionResult(result);
    }

    private ActionResult<RequestDto> ToActionResult(DecisionResult result) => result.Status switch
    {
        DecisionStatus.Success => result.Request!,
        DecisionStatus.NotFound => NotFound(new { message = result.Message }),
        DecisionStatus.NotPending => Conflict(new { message = result.Message }),
        DecisionStatus.InsufficientBalance => Conflict(new { message = result.Message }),
        DecisionStatus.Conflict => Conflict(new { message = result.Message }),
        _ => StatusCode(StatusCodes.Status500InternalServerError)
    };
}