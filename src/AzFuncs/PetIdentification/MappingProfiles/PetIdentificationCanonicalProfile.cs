using AutoMapper;
using PetIdentification.Dtos;
using PetIdentification.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PetIdentification.Profiles
{
    public class PetIdentificationCanonicalProfile : Profile
    {
        public PetIdentificationCanonicalProfile()
        {
            CreateMap<PetIdentificationCanonical, PetIdentificationCanonicalDto>();
        }
    }
}
