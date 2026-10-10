using TimeOffAccrual.Api.Models;

namespace TimeOffAccrual.Api.Dtos;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public Role Role { get; set; }
}