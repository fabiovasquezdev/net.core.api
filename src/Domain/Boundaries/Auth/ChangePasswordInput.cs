namespace Domain.Boundaries.Auth
{
    public record ChangePasswordInput(
     string Username,
     string CurrentPassword,
     string NewPassword
 );
}