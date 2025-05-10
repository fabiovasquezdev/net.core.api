using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace API.Middleware
{
    public class JwtMiddleware(RequestDelegate next)
    {
        public const string XCORRELATIONID = "X-Correlation-Id";
        public const string XAUTHORIZATION = "Authorization";
        private readonly RequestDelegate _next = next;

        public async Task Invoke(HttpContext context,
                                 IContextUserService contextUser)
        {
            var token = context.Request.Headers[XAUTHORIZATION].FirstOrDefault()?.Split(" ").Last() ??
                            (context.Request.Query.TryGetValue(XAUTHORIZATION, out var tk) ?
                                tk.FirstOrDefault()?.Split(" ").Last() : null);

            if (!Guid.TryParse(context.Request.Headers[XCORRELATIONID], out var _))
                context.Request.Headers.Append(XCORRELATIONID, Guid.NewGuid().ToString());

            if (token is null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }

            AttachUserToContext(contextUser, token);
            if (string.IsNullOrWhiteSpace(context.Request.Headers[XCORRELATIONID]))
                context.Request.Headers.Append(XCORRELATIONID, Guid.NewGuid().ToString());
            contextUser.SetCorrelationId(Guid.Parse(context.Request.Headers[XCORRELATIONID]));
            await _next(context);
        }

        private static void AttachUserToContext(IContextUserService contextUser, string token)
        {
            try
            {
                var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
                string userName = jwt.Claims.First(x => x.Type == "name").Value;
                string sub = jwt.Claims.First(x => x.Type == "sub").Value;

                ContextUser cxtUser = new(
                                          Guid.Parse(sub),
                                          userName,
                                          jwt.Claims.ToDictionary(x => x.Type, x => x.Value));
                contextUser.SetUser(cxtUser);
                contextUser.SetToken(token);
            }
            catch { }
        }

        public static ContextUser GetUser(string token)
        {
            ContextUser cxtUser = new(Guid.Empty,
                                              string.Empty,
                                              new Dictionary<string, string>());
            try
            {
                var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
                string userName = jwt.Claims.First(x => x.Type == "name").Value;
                string sub = jwt.Claims.First(x => x.Type == "sub").Value;

                cxtUser = new(Guid.Parse(sub),
                               userName,
                               jwt.Claims.ToDictionary(x => x.Type, x => x.Value));


            }
            catch { }
            return cxtUser;
        }

    }

    public static class ContextUserExtension
    {
        public static IServiceCollection AddContextUser(this IServiceCollection services) =>
            services.AddScoped<IContextUserService,
                               ContextUserService>();
    }

    public interface IContextUserService
    {
        ContextUser User { get; }
        string Token { get; }
        Guid CorrelationId { get; }

        void SetUser(ContextUser contextUser);
        void SetToken(string token);
        void SetCorrelationId(Guid correlationId);
    }

    internal sealed class ContextUserService : IContextUserService
    {
        private ContextUser _contextUser = new(
                                               Guid.Empty,
                                               string.Empty,
                                               new Dictionary<string, string>());

        private string _token = string.Empty;
        public ContextUser User => _contextUser;

        public string Token => _token;

        private Guid _correlationId = Guid.NewGuid();

        public Guid CorrelationId => _correlationId;

        public void SetCorrelationId(Guid correlationId)
        {
            _correlationId = correlationId;
        }

        public void SetToken(string token) =>
            _token = token;

        public void SetUser(ContextUser contextUser) =>
            _contextUser = contextUser;
    }

    public record struct ContextUser(Guid Sub,
                                     string Name,
                                     IDictionary<string, string> Claims);
}