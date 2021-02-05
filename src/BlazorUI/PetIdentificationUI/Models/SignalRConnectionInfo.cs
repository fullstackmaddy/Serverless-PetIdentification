using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PetIdentificationUI.Models
{
    public class SignalRConnectionInfo
    {
        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }

        [JsonProperty(PropertyName = "accessToken")]
        public string AccessToken { get; set; }
    }
}
