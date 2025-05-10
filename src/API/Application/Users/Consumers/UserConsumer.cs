using System.Threading.Tasks;
using Common;
using Domain.Entities;
using Domain.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace API.Application.Users.Consumers
{
    public class UserConsumer(
            ILogger<UserConsumer> logger,
            ISessionObservable<User> sessionUser) :
        IConsumer<UserCreated>,
        IConsumer<UserChanged>,
        IConsumer<UserStatusChanged>
    {
        private readonly ILogger<UserConsumer> _logger = logger;
        private readonly ISessionObservable<User> _sessionUser = sessionUser;

        public async Task Consume(ConsumeContext<UserCreated> context)
        {
            _logger.LogInformation("[USER CREATED] email: {Email}",
                context.Message.User.Email);

            var result = _sessionUser.SendNotification(context.Message.User);

            _logger.LogInformation("[USER CREATED SENT] email: {Email}, {Result}",
                context.Message.User.Email, result);

          
            await Task.CompletedTask;
        }

        public async Task Consume(ConsumeContext<UserChanged> context)
        {
            _logger.LogInformation("[USER CHANGED] email: {Email}",
                context.Message.User.Email);

            var result = _sessionUser.SendNotification(context.Message.User);

            _logger.LogInformation("[USER CHANGED SENT] email: {Email}, {Result}",
                context.Message.User.Email, result);

         
            await Task.CompletedTask;
        }

        public async Task Consume(ConsumeContext<UserStatusChanged> context)
        {
            _logger.LogInformation("[USER STATUS CHANGED] email: {Email}, status: {Status}",
                context.Message.User.Email, context.Message.User.Status);

            var result = _sessionUser.SendNotification(context.Message.User);

            _logger.LogInformation("[USER STATUS CHANGED SENT] email: {Email}, status: {Status}, {Result}",
                context.Message.User.Email, context.Message.User.Status, result);

            await Task.CompletedTask;
        }
    }
}