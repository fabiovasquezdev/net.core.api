
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Repository.MongoDB;

namespace API.Application
{
    public class MongoDbMidleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next;
        public async Task Invoke(HttpContext context, 
                                 IMongoUnitOfWork unitOfWork) => 
            await (context.Request.Method == "GET" ? 
                _next(context) :
                unitOfWork.WithTransactionAsync(async () => 
                    await _next(context), 
                    context.RequestAborted));
    }
}