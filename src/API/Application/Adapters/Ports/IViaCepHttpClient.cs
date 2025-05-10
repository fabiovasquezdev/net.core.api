using System.Threading.Tasks;
using Common;
using Domain.Boundaries.ViaCep;

namespace API.Application.Adapters.Ports
{
    public interface IViaCepHttpClient
    {
        Task<Result<ViaCepResponse>> GetAddressByCep(string cep);
    }
}