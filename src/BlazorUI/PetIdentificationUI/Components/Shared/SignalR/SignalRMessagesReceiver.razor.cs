using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using PetIdentificationUI.HttpClients;
using PetIdentificationUI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PetIdentificationUI.Components.Shared.SignalR
{
    public partial class SignalRMessagesReceiver: ComponentBase
    {
        [Inject]
        SignalRConnectionHttpClient SignalRConnectionHttpClient
        {
            get; set;
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
                             SignalRConnectionHttpClient
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
    }
}
