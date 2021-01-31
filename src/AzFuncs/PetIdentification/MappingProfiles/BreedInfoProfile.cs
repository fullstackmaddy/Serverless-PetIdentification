using AutoMapper;
using PetIdentification.Dtos;
using PetIdentification.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PetIdentification.MappingProfiles
{
    public class BreedInfoProfile : Profile
    {
        public BreedInfoProfile()
        {
            CreateMap<BreedInfoDto, BreedInfo>();
            CreateMap<BreedInfo, BreedInfoDto>();
        }
    }
}
