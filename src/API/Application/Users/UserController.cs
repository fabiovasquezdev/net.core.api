using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using API.Application.Users.Ports;
using Common;
using Common.WebSocket;
using Domain.Boundaries.User;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Repository.MongoDB;

namespace API.Application.Users
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController(
            IUserService userService,
            IMongoDBRepository<User> userRepository,
            ISessionObservable<User> sessionUser,
            ILogger<UserController> logger) : ControllerBase
    {
        private readonly IUserService _userService = userService;
        private readonly IMongoDBRepository<User> _userRepository = userRepository;
        private readonly ISessionObservable<User> _sessionUser = sessionUser;
        private readonly ILogger<UserController> _logger = logger;

        [HttpGet("listen")]
        public async Task ListenUserAsync([FromQuery] string search, CancellationToken cancellationToken)
        {
            if (!HttpContext.WebSockets.IsWebSocketRequest)
                return;

            Expression<Func<User, bool>> filter = u =>
                u.Status != UserStatus.Inactive &&
                (string.IsNullOrEmpty(search) || u.FirstName.Contains(search));

            var users = _userRepository.GetAll(filter);

            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

            foreach (var user in users)
                await WebSocketAbstractions.SendWebSocketMessageAsync(webSocket, user, cancellationToken);

            var compiledFilter = filter.Compile();
            using var _ = _sessionUser.Subscribe(user => compiledFilter(user) ?
                WebSocketAbstractions.SendWebSocketMessageAsync(webSocket, user, cancellationToken) :
                Task.CompletedTask);

            await WebSocketAbstractions.LogReceivedWebSocketMessagesAsync(webSocket, _logger, cancellationToken);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = _userService.GetByIdAsync(id, cancellationToken);
            if (!result)
                return NotFound(result.Error);

            return Ok(result.Value);
        }

        [HttpGet("email/{email}")]
        public IActionResult GetByEmail(string email, CancellationToken cancellationToken)
        {
            var result = _userService.GetByEmailAsync(email, cancellationToken);
            if (!result)
                return NotFound(result.Error);

            return Ok(result.Value);
        }

        [HttpGet]
        public IActionResult GetAll(CancellationToken cancellationToken)
        {
            var result = _userService.GetAllAsync(cancellationToken);
            if (!result)
                return BadRequest(result.Error);

            return Ok(result.Value);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserInput input, CancellationToken cancellationToken)
        {
            var result = await _userService.CreateUserAsync(input, cancellationToken);
            if (result.Success)
                return Ok(result.Value);

            return BadRequest(result.Error);
        }

        [HttpPut("{userId}")]
        public async Task<IActionResult> UpdateUser(Guid userId, [FromBody] UpdateUserInput input, CancellationToken cancellationToken)
        {
            var result = await _userService.UpdateUserAsync(userId, input, cancellationToken);
            if (result.Success)
                return Ok();

            return BadRequest(result.Error);
        }

        [HttpPost("{id}/activate")]
        public async Task<IActionResult> ActivateUser(Guid id, CancellationToken cancellationToken)
        {
            var result = await _userService.ActivateUserAsync(id, cancellationToken);
            if (result.Success)
                return Ok();

            return BadRequest(result.Error);
        }

        [HttpPost("{id}/deactivate")]
        public async Task<IActionResult> DeactivateUser(Guid id, CancellationToken cancellationToken)
        {
            var result = await _userService.DeactivateUserAsync(id, cancellationToken);
            if (result.Success)
                return Ok();

            return BadRequest(result.Error);
        }

        [HttpPost("{id}/block")]
        public async Task<IActionResult> BlockUser(Guid id, CancellationToken cancellationToken)
        {
            var result = await _userService.BlockUserAsync(id, cancellationToken);
            if (result.Success)
                return Ok();

            return BadRequest(result.Error);
        }
    }
}