using System.Collections.Generic;
using Xunit;
using FluentAssertions;
using AutoMapper;
using PetIdentification.Profiles;
using PetIdentification.Models;
using PetIdentification.Tests.Helpers;
using PetIdentification.Dtos;

namespace PetIdentification.Tests.UnitTests
{
    public class PetIdentificationCanonicalMapperTests
    {
        private readonly IMapper _mapper;

        public PetIdentificationCanonicalMapperTests()
        {
            var adoptionCentreProfile = new AdoptionCentreProfile();
            var breedInfoProfile = new BreedInfoProfile();

            var profiles = new List<Profile>()
            {
                new AdoptionCentreProfile(),
                new BreedInfoProfile(),
                new PetIdentificationCanonicalProfile()
            };

            var config = new MapperConfiguration(x => x.AddProfiles(profiles));

            _mapper = config.CreateMapper();
        }

        [Fact]
        public void Does_Canonical_DTO_Has_Expected_Mapping()
        {
            PetIdentificationCanonical petIdentificationCanonical =
                new PetIdentificationCanonical()
                {
                    AdoptionCentres = TestFactory.AdoptionCentres,
                    BreedInformation = TestFactory.BreedInfo
                };

            var result = _mapper
                .Map<PetIdentificationCanonical, PetIdentificationCanonicalDto>(petIdentificationCanonical);

            //Assertions

            result.Should().BeOfType<PetIdentificationCanonicalDto>();
            var canonicalDto = result as PetIdentificationCanonicalDto;

            canonicalDto.BreedInformation.Breed.Should().BeEquivalentTo("Pug");
            canonicalDto.AdoptionCentres.Count.Should().Be(1);
            canonicalDto.AdoptionCentres[0].ShelteredBreed.Should().BeEquivalentTo("Pug");
        }
    }
}
