namespace Domain.Boundaries.Auth
{
    public record UserCredentialsInput(
      string Type,
      string Value,
      bool Temporary
  );

}