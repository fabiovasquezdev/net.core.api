namespace Domain.Boundaries.Auth
{
    public record AccessTokenResponse(
      string AccessToken,
      string TokenType,
      int ExpiresIn,
      string RefreshToken,
      string Scope,
      string Sub
  );
}