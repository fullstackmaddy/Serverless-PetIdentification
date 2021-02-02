using System;
using Newtonsoft.Json;

namespace PetIdentification.Dtos
{
    public class DurableRequestDto
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