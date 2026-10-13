using TimeOffAccrual.Api.Models;

namespace TimeOffAccrual.Api.Services;

public interface ITokenService
{
    string CreateToken(User user);
}