using BlazorInputFile;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using PetIdentificationUI.HttpClients;
using PetIdentificationUI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PetIdentificationUI.Components.UploadWithForm
{
    public partial class UploadWithForm: ComponentBase
    {
        [Inject] public HttpAzureFunctionsClient HttpAzureFunctionsClient { get; set; }

        private const string DefaultMessage = @"Drop a image of the stray pet here, or click to choose a file";

        private const int MaxFileSize = 5 * 1024 * 1024;

        PetIdentificationCanonical _petIdentificationCanonical = null;


        public async Task UploadImageAsync(IFileListEntry[] files)
        {
            var file = files.FirstOrDefault();

            if (file != null && file.Size < MaxFileSize)
            {
               
                var result = await HttpAzureFunctionsClient
                    .PostFormAsync(
                        new Dictionary<string, string>()
                        {
                            {"correlationId", Guid.NewGuid().ToString() },
                            {"signalRUserId", Guid.NewGuid().ToString() }
                        },
                        file.Data,
                        file.Type
                    )
                    .ConfigureAwait(false);

                _petIdentificationCanonical =
                    JsonConvert.DeserializeObject<PetIdentificationCanonical>
                    (result);

            }

        }

    }
}
