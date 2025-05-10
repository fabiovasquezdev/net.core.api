using Domain.Entities;

namespace Domain.Boundaries.User;

public record CreateUserInput(
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    string Password,
    UserType Type);

public record UpdateUserInput(
    string FirstName,
    string LastName,
    string PhoneNumber);

public record ChangePasswordInput(
    string CurrentPassword,
    string NewPassword);
