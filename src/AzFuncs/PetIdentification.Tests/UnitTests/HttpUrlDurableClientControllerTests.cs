using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Moq;
using PetIdentification.Constants;
using PetIdentification.Dtos;
using PetIdentification.Functions;
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
    public class HttpUrlDurableClientControllerTests
    {
        private readonly Mapper _mapper;
        private readonly Mock<IDurableOrchestrationContext> _orchestrationContext;
        private HttpUrlDurableClientController _funcController;
        private readonly Mock<IDurableClient> _durableClient;

        public HttpUrlDurableClientControllerTests()
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
                (ActivityFunctionsConstants.IdentifyStrayPetBreedAsync, It.IsAny<string>())

            ).ReturnsAsync(TestFactory.PredictedTags);

            _orchestrationContext.Setup(
                x => x.CallActivityAsync<List<AdoptionCentre>>(
                        ActivityFunctionsConstants.LocateAdoptionCentresByBreedAsync,
                        It.IsAny<string>())
                ).ReturnsAsync(TestFactory.AdoptionCentres);

            _orchestrationContext.Setup(
                x => x.CallActivityAsync<bool>("PushMessagesToSignalRHub",
                    It.IsAny<SignalRRequest>())
                ).ReturnsAsync(true);

            _funcController = new HttpUrlDurableClientController(
                _mapper);

            _durableClient = new Mock<IDurableClient>();
        }


        [Fact]
        public async Task IsOrchestration_Completed_Successfully()
        {
            var result = await _funcController
            .RunOrchestrator(_orchestrationContext.Object,
            TestFactory.CreateLogger());

            //Assertions
            result.Should()
                  .BeOfType<string>();
            result.Should()
                .Be("Orchestrator HttpUrlOrchestration executed the functions");
        }

        [Fact]
        public async Task Does_HTTP_DurableClient_Return_Unsupported_MediaType_When_Content_Is_not_Json()
        {

            var result = await _funcController.HttpUrlDurableClient(
                TestFactory.CreateHttpRequest(string.Empty
                , string.Empty), _durableClient.Object, TestFactory.CreateLogger());

            //Assertions
            result.Should().BeOfType<UnsupportedMediaTypeResult>();

            (result as UnsupportedMediaTypeResult).StatusCode.Should().Be(415);
        }


    }
}
