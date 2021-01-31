using AutoMapper;
using FluentAssertions;
using Moq;
using PetIdentification.Functions;
using PetIdentification.Interfaces;
using PetIdentification.Models;
using PetIdentification.Profiles;
using PetIdentification.Tests.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PetIdentification.Tests.UnitTests
{
    public class ActivityFunctionsControllerTests
    {
        private readonly Mapper _mapper;
        private readonly Mock<IPredictionHelper> _predictionHelper;
        private readonly Mock<IAdoptionCentreDbHelper> _adoptionCentreDbHelper;
        private readonly Mock<IBreedInfoDbHelper> _breedInfoDbHelper;
        private readonly ActivityFunctionsController _funcController;

        public ActivityFunctionsControllerTests()
        {
            PredictionResultProfile val = new PredictionResultProfile();
            List<Profile> mappingProfiles = new List<Profile>
            {
                new PredictionResultProfile(),
                new AdoptionCentreProfile()
            };
            var config = new MapperConfiguration(x =>
            {
                x.AddProfiles((IEnumerable<Profile>)mappingProfiles);
            });

            _mapper = new Mapper(config);
            _predictionHelper = new Mock<IPredictionHelper>();

            _predictionHelper.Setup(
                x => x.PredictBreedAsync(It.IsAny<string>())
            )
            .ReturnsAsync(TestFactory.PredictedTags);

            _adoptionCentreDbHelper = new Mock<IAdoptionCentreDbHelper>();

            _adoptionCentreDbHelper.Setup(
                x => x.GetAdoptionCentresByBreedAsync(It.IsAny<string>())
            ).ReturnsAsync(TestFactory.AdoptionCentres);

            _breedInfoDbHelper = new Mock<IBreedInfoDbHelper>();

            _breedInfoDbHelper.Setup(
                x => x.GetBreedInformationAsync(It.IsAny<string>())
                ).ReturnsAsync(TestFactory.BreedInfo);

            _funcController = new ActivityFunctionsController(
                    adoptionCentreDbHelper: _adoptionCentreDbHelper.Object,
                    breedInfoDbHelper: _breedInfoDbHelper.Object,
                    predictionHelper: _predictionHelper.Object
                );

        }

        [Fact]
        public async Task Does_IdentifyStrayPetBreedAsync_Return_Prediction_Result()
        {

            var result = await _funcController
            .PredictStrayPetBreedAsync(string.Empty, TestFactory.CreateLogger(LoggerTypes.List));

            //Assertions

            result.Should().BeOfType<List<PredictionResult>>();
            result.Should().HaveCount(1);
            result[0].Probability.Should().Be(1.0);
            result[0].TagName.Should().Be("pug");

        }

        [Fact]
        public async Task Does_LocateAdoptionCentresByBreedAsync_Return_List_Of_AdoptionCentres()
        {
            var result = await _funcController
            .LocateAdoptionCentresByBreedAsync(string.Empty, TestFactory.CreateLogger(LoggerTypes.List));

            //Assertions

            result.Should().BeOfType<List<AdoptionCentre>>();
            result.Should().HaveCount(1);
            result[0].ShelteredBreed.Should().Be("pug");
        }

        [Fact]
        public async Task Does_GetBreedInformationAsync_Return_Breed_Information()
        {
            var logger = TestFactory.CreateLogger(LoggerTypes.List);

            var result = await _funcController
                .GetBreedInformationASync(string.Empty,
                TestFactory.CreateLogger());

            result.Should().BeOfType<BreedInfo>();
            (result as BreedInfo).Breed.Should().BeEquivalentTo("pug");

        }
    }
}
