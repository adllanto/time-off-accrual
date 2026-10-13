using TimeOffAccrual.Api.Dtos;
using TimeOffAccrual.Api.Models;

namespace TimeOffAccrual.Api.Services;

public interface IRequestService
{
    Task<CreateRequestResult> CreateAsync(int userId, CreateRequestDto dto);
    Task<DecisionResult> CancelAsync(int requestId, int userId);
    Task<List<RequestDto>> GetForUserAsync(int userId);
    Task<List<RequestDto>> GetByStatusAsync(RequestStatus status);
    Task<DecisionResult> ApproveAsync(int requestId, int adminId);
    Task<DecisionResult> DenyAsync(int requestId, int adminId);
}