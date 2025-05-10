using Domain.Entities;
using MassTransit;

namespace Domain.Events
{
    public record class UserCreated(Guid CorrelationId, User User) : CorrelatedBy<Guid>;
    public record UserChanged(Guid CorrelationId, User User) : CorrelatedBy<Guid>;
    public record UserStatusChanged(Guid CorrelationId, User User) : CorrelatedBy<Guid>;
}