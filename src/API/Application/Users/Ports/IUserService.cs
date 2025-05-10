using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Domain.Boundaries.User;
using Domain.Entities;

namespace API.Application.Users.Ports
{
    public interface IUserService
    {
        Task<Result<User>> CreateUserAsync(CreateUserInput input, CancellationToken cancellationToken);
        Task<Result<User>> UpdateUserAsync(Guid userId, UpdateUserInput input, CancellationToken cancellationToken);
        Task<Result<User>> ActivateUserAsync(Guid id, CancellationToken cancellationToken);
        Task<Result<User>> DeactivateUserAsync(Guid id, CancellationToken cancellationToken);
        Task<Result<User>> BlockUserAsync(Guid id, CancellationToken cancellationToken);
        Result<User> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Result<User> GetByEmailAsync(string email, CancellationToken cancellationToken);
        Result<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken);
    }
}