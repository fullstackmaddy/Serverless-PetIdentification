using System.Collections.Generic;
using Newtonsoft.Json;

namespace PetIdentificationUI.Models
{
    public class PetIdentificationCanonical
    {
        [JsonProperty(PropertyName = "adoptionCentres")]
        public List<AdoptionCentre> AdoptionCentres { get; set; }

        [JsonProperty(PropertyName = "breedInformation")]
        public BreedInfo BreedInformation { get; set; }
    }
}
