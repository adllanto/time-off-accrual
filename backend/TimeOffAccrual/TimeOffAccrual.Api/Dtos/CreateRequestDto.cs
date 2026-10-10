using TimeOffAccrual.Api.Models;

namespace TimeOffAccrual.Api.Dtos;

public class CreateRequestDto
{
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public DurationType DurationType { get; set; }
    public decimal? Hours { get; set; } // only used for Partial; ignored for Full
}