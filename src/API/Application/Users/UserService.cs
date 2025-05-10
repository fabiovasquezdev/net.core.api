using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using API.Application.Users.Ports;
using Common;
using Domain.Boundaries.User;
using Domain.Entities;
using Domain.Events;
using MassTransit;
using Repository.MongoDB;

namespace API.Application.Users;

public sealed class UserService : IUserService
{
    private readonly IMongoDBRepository<User> _repository;
    private readonly IMongoUnitOfWork _unitOfWork;
    private readonly ITopicProducer<UserCreated> _userCreated;
    private readonly ITopicProducer<UserChanged> _userChanged;
    private readonly ITopicProducer<UserStatusChanged> _userStatusChanged;

    public UserService(
        IMongoDBRepository<User> repository,
        IMongoUnitOfWork unitOfWork,
        ITopicProducer<UserCreated> userCreated,
        ITopicProducer<UserChanged> userChanged,
        ITopicProducer<UserStatusChanged> userStatusChanged)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _userCreated = userCreated;
        _userChanged = userChanged;
        _userStatusChanged = userStatusChanged;
    }

    public async Task<Result<User>> CreateUserAsync(CreateUserInput input, CancellationToken cancellationToken)
    {
        var existingUser = _repository.GetFirstOrDefault(x => x.Email == input.Email);
        if (existingUser != null)
            return ErrorCodes.USER_ALREADY_EXISTS;

        var user = new User(
            input.FirstName,
            input.LastName,
            input.Email,
            input.PhoneNumber,
            input.Type
        );

        if (!user.IsValid())
            return user.IsValid();

        _repository.Add(user);
        _unitOfWork.SaveChanges();

        await _userCreated.Produce(new UserCreated(Guid.NewGuid(), user), cancellationToken);

        return user;
    }

    public async Task<Result<User>> UpdateUserAsync(Guid userId, UpdateUserInput input, CancellationToken cancellationToken)
    {
        var user = _repository.GetFirstOrDefault(x => x.Id == userId);
        if (user == null)
            return ErrorCodes.USER_NOT_FOUND;

        var update = user with
        {
            FirstName = input.FirstName,
            LastName = input.LastName,
            PhoneNumber = input.PhoneNumber
        };

        if (!update.IsValid())
            return update.IsValid();

        _repository.Update(update);
        _unitOfWork.SaveChanges();

        await _userChanged.Produce(new UserChanged(Guid.NewGuid(), update), cancellationToken);

        return update;
    }

    public async Task<Result<User>> ActivateUserAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = _repository.GetFirstOrDefault(x => x.Id == id);
        if (user == null)
            return ErrorCodes.USER_NOT_FOUND;

        var update = user with { Status = UserStatus.Active };
        _repository.Update(update);
        _unitOfWork.SaveChanges();

        await _userStatusChanged.Produce(new UserStatusChanged(Guid.NewGuid(), update), cancellationToken);

        return update;
    }

    public async Task<Result<User>> DeactivateUserAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = _repository.GetFirstOrDefault(x => x.Id == id);
        if (user == null)
            return ErrorCodes.USER_NOT_FOUND;

        var update = user with { Status = UserStatus.Inactive };
        _repository.Update(update);
        _unitOfWork.SaveChanges();

        await _userStatusChanged.Produce(new UserStatusChanged(Guid.NewGuid(), update), cancellationToken);

        return update;
    }

    public async Task<Result<User>> BlockUserAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = _repository.GetFirstOrDefault(x => x.Id == id);
        if (user == null)
            return ErrorCodes.USER_NOT_FOUND;

        var update = user with { Status = UserStatus.Blocked };
        _repository.Update(update);
        _unitOfWork.SaveChanges();

        await _userStatusChanged.Produce(new UserStatusChanged(Guid.NewGuid(), update), cancellationToken);

        return update;
    }

    public Result<User> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = _repository.GetFirstOrDefault(x => x.Id == id);
        if (user == null)
            return ErrorCodes.USER_NOT_FOUND;

        return user;
    }

    public Result<User> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var user = _repository.GetFirstOrDefault(x => x.Email == email);
        if (user == null)
            return ErrorCodes.USER_NOT_FOUND;

        return user;
    }

    public Result<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken)
    {
        var users = _repository.GetAll().ToList();
        return users;
    }
}