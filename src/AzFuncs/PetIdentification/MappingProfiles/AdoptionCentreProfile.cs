using AutoMapper;
using PetIdentification.Models;
using PetIdentification.Dtos;

namespace PetIdentification.Profiles
{
    public class AdoptionCentreProfile : Profile
    {
        public AdoptionCentreProfile()
        {
            CreateMap<AdoptionCentre, AdoptionCentreDto>();
            
        }
        
    }
}