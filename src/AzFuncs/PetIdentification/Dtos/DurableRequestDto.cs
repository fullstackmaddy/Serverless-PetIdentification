using System;
using Newtonsoft.Json;

namespace PetIdentification.Dtos
{
    public class DurableRequestDto
    {
        [JsonProperty(PropertyName = "blobUrl")]
        public Uri BlobUrl { get; set; }

        [JsonProperty(PropertyName = "signalRUserId")]
        public string SignalRUserId { get; set; }

        [JsonProperty(PropertyName = "correlationId")]
        public string CorrelationId { get; set; }
    }
}