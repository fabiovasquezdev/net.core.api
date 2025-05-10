using Common;

namespace Domain.Entities;

public record class User : Entity
{
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public string Email { get; init; }
    public string PhoneNumber { get; init; }
    public UserType Type { get; init; }
    public UserStatus Status { get; init; }

    protected User()
    {
    }

    public User(
        string firstName,
        string lastName,
        string email,
        string phoneNumber,
        UserType type)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PhoneNumber = phoneNumber;
        Type = type;
        Status = UserStatus.Active;
    }

    public override Result IsValid() => true switch
    {
        true when string.IsNullOrEmpty(Email) => ErrorCodes.INVALID_USER_EMAIL,

        _ => null
    };
}

public enum UserStatus
{
    Active,
    Inactive,
    Blocked
}

public enum UserType
{
    Admin,
    Manager,
    Employee
}

