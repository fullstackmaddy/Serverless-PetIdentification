using Newtonsoft.Json;

namespace PetIdentification.Models
{
    public class BreedInfo
    {

        [JsonProperty(PropertyName = "breed")]
        public string Breed { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "lifeExpectancy")]
        public string LifeExpectancy { get; set; }

        [JsonProperty(PropertyName = "height")]
        public string Height { get; set; }

        [JsonProperty(PropertyName = "weight")]
        public string Weight { get; set; }

        [JsonProperty(PropertyName = "qualities")]
        public string Qualities { get; set; }
    }
}
