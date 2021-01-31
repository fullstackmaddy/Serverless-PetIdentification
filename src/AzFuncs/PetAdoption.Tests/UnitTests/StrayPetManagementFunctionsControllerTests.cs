using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Moq;
using PetIdentification.Dtos;
using PetIdentification.Functions;
using PetIdentification.Interfaces;
using PetIdentification.Models;
using PetIdentification.Profiles;
using PetAdoption.Tests.Helpers;
using Xunit;
using Microsoft.AspNetCore.Mvc;

namespace PetAdoption.Tests.UnitTests
{
    public class StrayPetManagementFunctionsControllerTests
    {
        private readonly Mock<IPredictionHelper> _predictionHelper;

        private readonly Mock<IAdoptionCentreDbHelper> _adoptionCentreDbHelper;

        private readonly Mock<IBreedInfoDbHelper> _breednfoDbHelper;

        private readonly IMapper _mapper;

        private readonly PetIdentificationManagementFunctionsController _funcController;

        private readonly Mock<IDurableOrchestrationContext> _orchestrationContext;

        private readonly Mock<IDurableClient> _durableClient;

        public StrayPetManagementFunctionsControllerTests()
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

            _breednfoDbHelper = new Mock<IBreedInfoDbHelper>();

            _breednfoDbHelper.Setup(
                x => x.GetBreedInformationAsync(It.IsAny<string>())
                ).ReturnsAsync(TestFactory.BreedInfo);



            _funcController = new PetIdentificationManagementFunctionsController(
                _adoptionCentreDbHelper.Object,
                _breednfoDbHelper.Object,
                _predictionHelper.Object,
                _mapper
            );

            #region Orchestration Function Mocks
            _orchestrationContext = new Mock<IDurableOrchestrationContext>();
            _orchestrationContext.Setup(
                x => x.GetInput<DurableRequestDto>()
            )
            .Returns(
                new DurableRequestDto()
                {
                    BlobUrl = new Uri("http://localhost"),
                    SignalRUserId = "123"
                }
            );
            _orchestrationContext.Setup(
                x => x.CallActivityAsync<List<PredictionResult>>
                ("IdentifyStrayPetBreedAsync", It.IsAny<string>())
                
            ).ReturnsAsync(TestFactory.PredictedTags);

            _orchestrationContext.Setup(
                x => x.CallActivityAsync<List<AdoptionCentre>>(
                    "LocateAdoptionCentresByBreedAsync", It.IsAny<string>())
            ).ReturnsAsync(TestFactory.AdoptionCentres);

            _orchestrationContext.Setup(
                x => x.CallActivityAsync<bool>("PushMessagesToSignalRHub",
                It.IsAny<SignalRRequest>())
            ).ReturnsAsync(true);

            #endregion

            _durableClient = new Mock<IDurableClient>();

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
            result[0].TagName.Should().Be("dog-pug");

        }

        [Fact]
        public async Task Does_LocateAdoptionCentresByBreedAsync_Return_List_Of_AdoptionCentres()
        {
            var result = await _funcController
            .LocateAdoptionCentresByBreedAsync(string.Empty, TestFactory.CreateLogger(LoggerTypes.List));

            //Assertions

            result.Should().BeOfType<List<AdoptionCentre>>();
            result.Should().HaveCount(1);
            result[0].ShelteredBreed.Should().Be("Pug");
        }

        [Fact]
        public async Task Does_GetBreedInformationAsync_Return_Breed_Information()
        {
            var logger = TestFactory.CreateLogger(LoggerTypes.List);

            var result = await _funcController
                .GetBreedInformationASync(string.Empty,
                TestFactory.CreateLogger());

            result.Should().BeOfType<BreedInfo>();
            (result as BreedInfo).Breed.Should().BeEquivalentTo("Pug");

        }

        [Fact]
        public async Task IsOrchestration_Completed_Successfully()
        {
            var result = await _funcController
            .RunOrchestrator(_orchestrationContext.Object, TestFactory.CreateLogger(LoggerTypes.List));

            //Assertions
            result.Should().BeOfType<string>();
            result.Should().Be("Orchestrator executed the functions.");
        }


        [Fact]
        public async Task Does_HTTP_DurableClient_Return_Unsupported_MediaType_When_Content_Is_not_Json()
        {

            var result = await _funcController.StrayPetManagementDurableHttpClient(
                TestFactory.CreateHttpRequest(string.Empty
                , string.Empty), _durableClient.Object, TestFactory.CreateLogger());

            //Assertions
            result.Should().BeOfType<UnsupportedMediaTypeResult>();

            (result as UnsupportedMediaTypeResult).StatusCode.Should().Be(415);
        }




    }

}