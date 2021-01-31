using System.Collections.Generic;
using Newtonsoft.Json;

namespace PetIdentification.Dtos
{
    public class PetIdentificationCanonicalDto
    {
        [JsonProperty(PropertyName = "adoptionCentres")]
        public List<AdoptionCentreDto> AdoptionCentres { get; set; }

        [JsonProperty(PropertyName = "breedInformation")]
        public BreedInfoDto BreedInformation { get; set; }
    }
}
