using API.Application.Adapters.HttpClient.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Common;
using Domain.Boundaries.ViaCep;

namespace API.Application.Adapters.Ports
{
    public class ViaCepHttpClient : IViaCepHttpClient
    {
        private readonly ViaCepSetting _viaCepSettings;

        public ViaCepHttpClient(IOptions<ViaCepSetting> viaCepSettings)
        {
            _viaCepSettings = viaCepSettings.Value;
        }

        public async Task<Result<ViaCepResponse>> GetAddressByCep(string cep)
        {
            try
            {
                var response = await _viaCepSettings.BaseUrl
                    .AppendPathSegment($"{cep}/json")
                    .AllowAnyHttpStatus()
                    .GetAsync();

                if (response.ResponseMessage.IsSuccessStatusCode)
                {
                    var result = await response.GetJsonAsync<ViaCepResponse>();
                    return result;
                }

                return await response.GetStringAsync();
            }
            catch (Exception)
            {
                return ErrorCodes.ADDRESS_NOT_FOUND;
            }
        }
    }
}