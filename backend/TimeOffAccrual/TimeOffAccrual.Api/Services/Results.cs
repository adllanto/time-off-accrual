using TimeOffAccrual.Api.Dtos;

namespace TimeOffAccrual.Api.Services;

public enum CreateRequestStatus { Success, Invalid, InsufficientBalance, Overlap, NoBalance }
public record CreateRequestResult(CreateRequestStatus Status, string? Message = null, RequestDto? Request = null);

public enum DecisionStatus { Success, NotFound, NotPending, InsufficientBalance, Conflict }
public record DecisionResult(DecisionStatus Status, string? Message = null, RequestDto? Request = null);

public enum UpdateCreditStatus { Success, NotFound, BelowTaken, Conflict }
public record UpdateCreditResult(UpdateCreditStatus Status, CreditOverviewDto? Credit = null);
