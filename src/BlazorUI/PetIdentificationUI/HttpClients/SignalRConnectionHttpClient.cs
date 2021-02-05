using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PetIdentificationUI.Models;

namespace PetIdentificationUI.HttpClients
{
    public class SignalRConnectionHttpClient
    {

        private readonly HttpClient _httpClient;

        public SignalRConnectionHttpClient(HttpClient httpClient)
        {
            this._httpClient = httpClient;

        }

        public async Task<SignalRConnectionInfo> GetHubConnectionInformationAsync(string userId)
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("SignalRUserId", userId);

            var result = await _httpClient.PostAsync(
                    "/api/negotiate",
                    new StringContent(string.Empty)
                );

            var responseData = await result
                .Content.ReadAsStringAsync()
                .ConfigureAwait(false);

            var connInfo = JsonConvert
                .DeserializeObject<SignalRConnectionInfo>(responseData);

            return connInfo;

            
        }
    }
}
