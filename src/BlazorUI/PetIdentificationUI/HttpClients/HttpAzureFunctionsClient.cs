using Newtonsoft.Json;
using PetIdentificationUI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace PetIdentificationUI.HttpClients
{
    public class HttpAzureFunctionsClient
    {
        private readonly HttpClient _httpClient;

        public HttpAzureFunctionsClient(HttpClient httpClient)
        {
            _httpClient = httpClient
                ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task CallHttpUrlDurableClientFunctionAsync
            (DurableRequest durableRequest)
        {
            
            await _httpClient.PostAsJsonAsync<DurableRequest>(
                    "/api/HttpUrlDurableClient",
                    durableRequest
                )
                .ConfigureAwait(false);

        }

        public async Task<SignalRConnectionInfo> GetHubConnectionInformationAsync(string userId)
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("signalruserid", userId);

            var result = await _httpClient.PostAsync(
                    "/api/negotiate",
                    new StringContent(string.Empty)
                )
                .ConfigureAwait(false);

            var responseData = await result
                .Content.ReadAsStringAsync()
                .ConfigureAwait(false);

            var connInfo = JsonConvert
                .DeserializeObject<SignalRConnectionInfo>(responseData);

            return connInfo;


        }
    }
}
