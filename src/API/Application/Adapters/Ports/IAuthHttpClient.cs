using System.Threading.Tasks;
using Common;
using Domain.Boundaries.Auth;

namespace API.Application.Adapters.Ports;

public interface IAuthHttpClient
{
    Task<Result<AccessTokenResponse>> Login(AuthInput input);
    Task<Result<object>> CreateUser(UserInput input);
    Task<Result<object>> ChangePassword(ChangePasswordInput input);
}