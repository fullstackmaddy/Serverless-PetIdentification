﻿using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using PetIdentificationUI.HttpClients;
using PetIdentificationUI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PetIdentificationUI.Components.UploadWithHttp
{
    public partial class UploadWithHttp : ComponentBase
    {
        [Inject] public HttpAzureFunctionsClient HttpAzureFunctionsClient { get; set; }
        string blobUrl;

        string userId = Guid.NewGuid().ToString();

        public async Task GetFileUploadStatus(string value)
        {
            blobUrl = value;

            await InvokeAzureFunctionAsync()
                .ConfigureAwait(false);
        }

        [Parameter] public string UserId { get; set; }

        private HubConnection _hubConnection;

        private PetIdentificationCanonical _petIdentificationCanonical;

        protected override async Task OnInitializedAsync()
        {

            await CreateHubConnection()
                .ConfigureAwait(false);

            _hubConnection.On<string>("sendPetAdoptionCentres", (message) =>
            {
                _petIdentificationCanonical = JsonConvert
                    .DeserializeObject<PetIdentificationCanonical>(message);
                StateHasChanged();
            });


        }

        private async Task CreateHubConnection()
        {
            //get connection info
            SignalRConnectionInfo connectionInfo = await
                             HttpAzureFunctionsClient
                             .GetHubConnectionInformationAsync(UserId)
                             .ConfigureAwait(false);

            Console.WriteLine("accessToken {0}", connectionInfo.AccessToken);
            Console.WriteLine("url {0}", connectionInfo.Url);

            //Build hb connection
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(
                    connectionInfo.Url,
                    options =>
                    {
                        options.AccessTokenProvider
                            = () => Task.FromResult(connectionInfo.AccessToken);
                    }
                )
                .Build();
        }

        public async ValueTask DisposeAsync()
        {
            await _hubConnection.DisposeAsync()
                .ConfigureAwait(false);
        }

        public async Task InvokeAzureFunctionAsync()
        {
            var durableRequest = new DurableRequest()
            {
                BlobUrl = new Uri(blobUrl),
                CorrelationId = Guid.NewGuid().ToString(),
                SignalRUserId = userId

            };

            await HttpAzureFunctionsClient
                .CallHttpUrlDurableClientFunctionAsync(durableRequest)
                .ConfigureAwait(false);


        }


    }
}
