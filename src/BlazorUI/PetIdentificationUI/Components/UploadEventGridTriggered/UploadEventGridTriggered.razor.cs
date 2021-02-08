using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using PetIdentificationUI.HttpClients;
using PetIdentificationUI.Models;
using System;
using System.Threading.Tasks;

namespace PetIdentificationUI.Components.UploadEventGridTriggered
{
    public partial class UploadEventGridTriggered : ComponentBase
    {
        string blobUrl;

        [Inject] public HttpAzureFunctionsClient HttpAzureFunctionsClient {get; set;}

        string userId = Guid.NewGuid().ToString();

        public void GetFileUploadStatus(string value)
        {
            blobUrl = value;

        }

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

            await _hubConnection.StartAsync();



        }

        private async Task CreateHubConnection()
        {
            //get connection info
            SignalRConnectionInfo connectionInfo = await
                             HttpAzureFunctionsClient
                             .GetHubConnectionInformationAsync(userId)
                             .ConfigureAwait(false);


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


    }
}
