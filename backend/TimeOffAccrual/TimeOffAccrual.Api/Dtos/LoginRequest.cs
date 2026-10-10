using System.ComponentModel.DataAnnotations;

namespace TimeOffAccrual.Api.Dtos;

public class LoginRequest
{
    [Required, EmailAddress, StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Password { get; set; } = string.Empty;
}