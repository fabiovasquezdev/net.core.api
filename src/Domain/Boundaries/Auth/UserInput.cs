
namespace Domain.Boundaries.Auth
{
    public record UserInput(
      string FirstName,
      string LastName,
      string Username,
      string Email,
      bool EmailVerified,
      bool Enabled,
      object Attributes
  );
}