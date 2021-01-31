using Newtonsoft.Json;

namespace PetIdentification.Dtos
{
    public class BreedInfoDto
    {
        [JsonProperty(PropertyName = "breed")]
        public string Breed { get; set; }

        [JsonProperty(PropertyName = "temprament")]
        public string Temprament { get; set; }

        [JsonProperty(PropertyName = "lifeExpectancy")]
        public string LifeExpectancy { get; set; }

        [JsonProperty(PropertyName = "qualities")]
        public string Qualities { get; set; }
    }
}
