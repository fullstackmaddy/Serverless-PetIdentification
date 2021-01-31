using Newtonsoft.Json;
using System;
namespace PetIdentificationUI.Models
{
    public class StrayAdoptionAppState
    {
        [JsonProperty(PropertyName = "blobUrl")]
        public Uri BlobUrl { get; set; }

        [JsonProperty(PropertyName = "signalRUserId")]
        public string SignalRUserId { get; set; }
    }
}