using System;
using Newtonsoft.Json;

namespace PetIdentificationUI.Models
{
    public class DurableRequest
    {
        [JsonProperty(PropertyName = "blobUrl",
            Required = Required.Always)]
        public Uri BlobUrl { get; set; }

        [JsonProperty(PropertyName = "signalRUserId", 
            Required = Required.Always)]
        public string SignalRUserId { get; set; }

        [JsonProperty(PropertyName = "correlationId",
            Required = Required.Always)]
        public string CorrelationId { get; set; }
    }
}