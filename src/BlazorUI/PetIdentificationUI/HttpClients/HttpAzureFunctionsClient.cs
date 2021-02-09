using Newtonsoft.Json;
using PetIdentificationUI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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

        public async Task<string> PostFormAsync(
                Dictionary<string, string> keyValuePairs,
                Stream stream,
                string fileContentType
            )
        {

            var form = new MultipartFormDataContent();

            foreach (var pair in keyValuePairs)
            {
                form.Add(
                        new StringContent(pair.Value)
                        , pair.Key
                    );
            }


            var fileContent = new ByteArrayContent(
                    await ReadStreamAsync(stream)
                    .ConfigureAwait(false)
                );
            fileContent.Headers.ContentType =
                MediaTypeHeaderValue.Parse(fileContentType);

            form.Add(fileContent, Guid.NewGuid().ToString(), "File");

            var result = await _httpClient.PostAsync(
                    "/api/HttpFormDataDurableClient",
                    form)
                .ConfigureAwait(false);

            return 
                await result.Content
                .ReadAsStringAsync()
                .ConfigureAwait(false);

        }

        private async Task<byte[]> ReadStreamAsync(Stream stream)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                await stream.CopyToAsync(ms)
                    .ConfigureAwait(false);

                return ms.ToArray();
            }
        }
    }
}
