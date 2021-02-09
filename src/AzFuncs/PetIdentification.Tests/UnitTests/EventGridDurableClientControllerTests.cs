using AutoMapper;
using FluentAssertions;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Moq;
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
    public class EventGridDurableClientControllerTests
    {
        private readonly IMapper _mapper;
        private readonly Mock<IDurableOrchestrationContext> _orchestrationContext;
        private readonly EventGridDurableClientController _funcController;

        public EventGridDurableClientControllerTests()
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
                    x => x.CallActivityWithRetryAsync<List<PredictionResult>>(
                            ActivityFunctionsConstants.IdentifyStrayPetBreedWithUrlAsync,
                            It.IsAny<RetryOptions>(),
                            It.IsAny<(string,string)>()
                        )
                )
                .ReturnsAsync(InstanceFactory.PredictedTags);

            _orchestrationContext.Setup(
                x => x.CallActivityAsync<List<AdoptionCentre>>(
                        ActivityFunctionsConstants.LocateAdoptionCentresByBreedAsync,
                        It.IsAny<(string, string)>())
                ).ReturnsAsync(InstanceFactory.AdoptionCentres);

            _orchestrationContext.Setup(
                x => x.CallActivityAsync<bool>(ActivityFunctionsConstants.PushMessagesToSignalRHub,
                    It.IsAny<SignalRRequest>())
                ).ReturnsAsync(true);

            _orchestrationContext.Setup(
                    x => x.CallActivityAsync<BreedInfo>(
                        ActivityFunctionsConstants.GetBreedInformationAsync,
                        It.IsAny<(string, string)>())
                ).ReturnsAsync(InstanceFactory.BreedInfo);

            _orchestrationContext.Setup(
                x => x.CallActivityAsync<string>(
                        ActivityFunctionsConstants.GetSignalUserIdFromBlobMetadataAsync,
                        It.IsAny<string>()
                    )
                ).ReturnsAsync("1234");

            _funcController = new EventGridDurableClientController(
                _mapper);

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
                        x => x.CallActivityWithRetryAsync<List<PredictionResult>>(
                                ActivityFunctionsConstants.IdentifyStrayPetBreedWithUrlAsync,
                                It.IsAny<RetryOptions>(),
                                It.IsAny<(string, string)>()
                            )
                    )
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
        public void Does_GetBlobName_Return_CorrectName()
        {
            //Arrange
            var guid = Guid.NewGuid().ToString();
            var imageUrl = string.Format("http://localhost/blobcontainer/{0}.jpg", guid);

            //Act
            var result = _funcController.GetBlobName(imageUrl);

            //Assert
            result.Should().BeEquivalentTo(guid);

        }

    }
}
