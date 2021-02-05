using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PetIdentificationUI.HttpClients;
using PetIdentificationUI.Models;
using Newtonsoft.Json;

namespace PetIdentificationUI.Components.Shared.Upload
{
    public partial class SignalR: ComponentBase
    {
        [Inject]
        SignalRConnectionHttpClient SignalRConnectionHttpClient
        {
            get; set;
        }
        [Parameter] public string UserId { get; set; }

        private HubConnection _hubConnection;

        private PetIdentificationCanonical _canonicalModel;

        protected override async Task OnInitializedAsync()
        {
            
            await CreateHubConnection()
                .ConfigureAwait(false);

            _hubConnection.On<string>("sendPetAdoptionCentres", (message) =>
            {
                _canonicalModel = JsonConvert
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

            Console.WriteLine("accessToken {0}",connectionInfo.AccessToken);
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
