using Domain.Entities;

namespace Domain.Boundaries.User;

public record UserOutput(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    UserType Type,
    UserStatus Status,
    DateTime CreatedDate,
    DateTime? UpdatedDate,
    bool IsActive);
