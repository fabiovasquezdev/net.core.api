using System;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Options;
using API.Application.Adapters.Ports;
using API.Application.Adapters.HttpClient.Configuration;
using Common;
using Domain.Boundaries.Auth;

namespace API.Application.Adapters.HttpClients;

public class AuthHttpClient : IAuthHttpClient
{
    private readonly KeycloakSettings _authSettings;

    public AuthHttpClient(IOptions<KeycloakSettings> authSettings)
    {
        _authSettings = authSettings.Value;
    }

    public async Task<Result<AccessTokenResponse>> Login(AuthInput input)
    {
        try
        {
            var response = await _authSettings.base_url
                .AppendPathSegment($"realms/{_authSettings.realms}/protocol/openid-connect/token")
                .WithHeader("Content-Type", _authSettings.content_type)
                .AllowAnyHttpStatus()
                .PostUrlEncodedAsync(new
                {
                    _authSettings.grant_type,
                    _authSettings.client_id,
                    _authSettings.client_secret,
                    username = input.Username,
                    password = input.Password
                });

            if (response.ResponseMessage.IsSuccessStatusCode)
            {
                var result = await response.GetJsonAsync<AccessTokenResponse>();
                return result;
            }

            return await response.GetStringAsync();
        }
        catch (Exception)
        {
            return ErrorCodes.UNAUTHORIZED;
        }
    }

    public async Task<Result<object>> CreateUser(UserInput input)
    {
        try
        {
            var auth = await AdminLogin();
            if (!auth)
            {
                return ErrorCodes.UNAUTHORIZED;
            }

            var response = await _authSettings.base_url
                .AppendPathSegment($"admin/realms/{_authSettings.realms}/users")
                .WithOAuthBearerToken(auth.Value.AccessToken)
                .AllowAnyHttpStatus()
                .PostJsonAsync(input);

            if (response.ResponseMessage.IsSuccessStatusCode)
            {
                return true;
            }

            return await response.GetStringAsync();
        }
        catch (Exception)
        {
            return ErrorCodes.UNAUTHORIZED;
        }
    }

    public async Task<Result<object>> ChangePassword(ChangePasswordInput input)
    {
        try
        {
            var user = await Login(new AuthInput(input.Username, input.CurrentPassword));
            if (!user)
                return ErrorCodes.UNAUTHORIZED;

            var auth = await AdminLogin();
            if (!auth)
                return ErrorCodes.UNAUTHORIZED;

            var response = await _authSettings.base_url
                .AppendPathSegment($"admin/realms/{_authSettings.realms}/users/{user.Value.Sub}/reset-password")
                .WithOAuthBearerToken(auth.Value.AccessToken)
                .AllowAnyHttpStatus()
                .PutJsonAsync(new
                {
                    type = "password",
                    value = input.NewPassword,
                    temporary = false
                });

            if (response.ResponseMessage.IsSuccessStatusCode)
            {
                return true;
            }

            return await response.GetStringAsync();
        }
        catch (Exception)
        {
            return ErrorCodes.UNAUTHORIZED;
        }
    }

    private async Task<Result<AccessTokenResponse>> AdminLogin()
    {
        try
        {
            var response = await _authSettings.base_url
                .AppendPathSegment($"realms/{_authSettings.realms}/protocol/openid-connect/token")
                .WithHeader("Content-Type", _authSettings.content_type)
                .AllowAnyHttpStatus()
                .PostUrlEncodedAsync(new
                {
                    _authSettings.grant_type,
                    _authSettings.client_id,
                    _authSettings.client_secret
                });

            if (response.ResponseMessage.IsSuccessStatusCode)
            {
                var result = await response.GetJsonAsync<AccessTokenResponse>();
                return result;
            }

            return await response.GetStringAsync();
        }
        catch (Exception)
        {
            return ErrorCodes.UNAUTHORIZED;
        }
    }
}