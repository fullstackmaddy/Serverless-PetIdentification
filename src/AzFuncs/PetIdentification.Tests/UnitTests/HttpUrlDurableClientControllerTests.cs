using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Moq;
using Newtonsoft.Json;
using PetIdentification.Constants;
using PetIdentification.Dtos;
using PetIdentification.Functions;
using PetIdentification.Models;
using PetIdentification.Tests.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace PetIdentification.Tests.UnitTests
{
    public class HttpUrlDurableClientControllerTests
    {
        private readonly IMapper _mapper;
        private readonly Mock<IDurableOrchestrationContext> _orchestrationContext;
        private readonly HttpUrlDurableClientController _funcController;
        private readonly Mock<IDurableClient> _durableClient;

        public HttpUrlDurableClientControllerTests()
        {
            
            _mapper = InstanceFactory.CreateMapper();

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
                (ActivityFunctionsConstants.IdentifyStrayPetBreedWithUrlAsync, It.IsAny<string>())

            ).ReturnsAsync(InstanceFactory.PredictedTags);

            _orchestrationContext.Setup(
                    x => x.CallActivityAsync<BreedInfo>(
                        ActivityFunctionsConstants.GetBreedInformationAsync,
                        It.IsAny<string>())
                ).ReturnsAsync(InstanceFactory.BreedInfo);

            _orchestrationContext.Setup(
                x => x.CallActivityAsync<List<AdoptionCentre>>(
                        ActivityFunctionsConstants.LocateAdoptionCentresByBreedAsync,
                        It.IsAny<string>())
                ).ReturnsAsync(InstanceFactory.AdoptionCentres);

            _orchestrationContext.Setup(
                x => x.CallActivityAsync<bool>(
                    ActivityFunctionsConstants.PushMessagesToSignalRHub,
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
            InstanceFactory.CreateLogger());

            //Assertions
            result.Should()
                  .BeOfType<string>();
            result.Should()
                .Be("Orchestrator sucessfully executed the functions.");
        }

        [Fact]
        public async Task Does_Orchestration_Catch_Exception()
        {
            //Arrange
            _orchestrationContext.Setup(
                x => x.CallActivityAsync<List<PredictionResult>>
                (ActivityFunctionsConstants.IdentifyStrayPetBreedWithUrlAsync, It.IsAny<string>()))
                .ThrowsAsync(InstanceFactory.Exception);

            //Act

            var result = await _funcController
            .RunOrchestrator(_orchestrationContext.Object,
            InstanceFactory.CreateLogger());

            //Assert
            result.Should()
                .BeEquivalentTo("Orchestrator failed in execution of the functions.");
        }

        [Fact]
        public async Task Does_HTTP_DurableClient_Return_Unsupported_MediaType_When_Content_Is_not_Json()
        {

            var result = await _funcController.HttpUrlDurableClient(
                InstanceFactory.CreateHttpRequest(string.Empty
                , string.Empty), _durableClient.Object, InstanceFactory.CreateLogger());

            //Assertions
            result.Should().BeOfType<UnsupportedMediaTypeResult>();

            (result as UnsupportedMediaTypeResult).StatusCode.Should().Be(415);
        }

        [Fact]
        public async Task Does_HTTP_DurableClient_Return_BadRequest_When_InCorrectDataIsSent()
        {
            //Arrange
            var breedInfo = InstanceFactory.BreedInfo;
            var httpRequest = InstanceFactory.CreateHttpRequest(
                string.Empty, string.Empty,
                JsonConvert.SerializeObject(breedInfo));
            httpRequest.ContentType = "application/json";

            //Act
            var result = await _funcController.HttpUrlDurableClient(
                httpRequest,
                _durableClient.Object,
                InstanceFactory.CreateLogger()
                );

            //Assert

            result.Should().BeOfType<BadRequestObjectResult>();
        }


    }
}
