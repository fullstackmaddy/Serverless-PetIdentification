using Newtonsoft.Json;

namespace PetIdentification.Models
{
    public class AdoptionCentre
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "address")]
        public string Address { get; set; }

        [JsonProperty(PropertyName = "zipCode")]
        public string ZipCode { get; set; }

        [JsonProperty(PropertyName = "shelteredBreed")]
        public string ShelteredBreed { get; set; }
    }
    
}