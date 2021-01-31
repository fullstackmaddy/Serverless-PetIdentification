using System.Collections.Generic;
using PetIdentification.Dtos;

namespace PetIdentification.Models
{
    public class SignalRRequest
    {
        public List<AdoptionCentreDto> AdoptionCentres { get; set; }

        public string UserId { get; set; }


        
    }
}