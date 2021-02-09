using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Primitives;
using Moq;
using Newtonsoft.Json;
using PetIdentification.Constants;
using PetIdentification.Dtos;
using PetIdentification.Functions;
using PetIdentification.Models;
using PetIdentification.Tests.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace PetIdentification.Tests.UnitTests
{
    public class HttpFormDataDurableClientControllerTests
    {
        private readonly IMapper _mapper;
        private readonly Mock<IDurableOrchestrationContext> _orchestrationContext;
        private readonly HttpFormDataDurableClientController _funcController;
        private readonly Mock<IDurableClient> _durableClient;

        public HttpFormDataDurableClientControllerTests()
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
                    x => x.CallActivityWithRetryAsync<List<PredictionResult>>
                    (
                        ActivityFunctionsConstants.IdentifyStrayPetBreedWithStreamAsync,
                        It.IsAny<RetryOptions>(),
                        It.IsAny<string>()

                    )
                )
                .ReturnsAsync(InstanceFactory.PredictedTags);


            _orchestrationContext.Setup(
                x => x.CallActivityAsync<List<AdoptionCentre>>(
                        ActivityFunctionsConstants.LocateAdoptionCentresByBreedAsync,
                        It.IsAny<string>())
                ).ReturnsAsync(InstanceFactory.AdoptionCentres);

            _orchestrationContext.Setup(
                x => x.CallActivityAsync<bool>(ActivityFunctionsConstants.PushMessagesToSignalRHub,
                    It.IsAny<SignalRRequest>())
                ).ReturnsAsync(true);

            _orchestrationContext.Setup(
                    x => x.CallActivityAsync<BreedInfo>(
                        ActivityFunctionsConstants.GetBreedInformationAsync,
                        It.IsAny<string>())
                ).ReturnsAsync(InstanceFactory.BreedInfo);

            _orchestrationContext.Setup(
                    x => x.GetInput<IFormFile>()

                ).Returns(InstanceFactory.CreateUploadFile());
            
            _funcController = new HttpFormDataDurableClientController(
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
                .Be(
                    JsonConvert.SerializeObject(
                            new PetIdentificationCanonical()
                            {
                                AdoptionCentres = InstanceFactory.AdoptionCentres,
                                BreedInformation = InstanceFactory.BreedInfo
                            }
                        )
                );
        }

        [Fact]
        public async Task Does_Orchestration_Catch_Exception()
        {
            //Arrange
            _orchestrationContext.Setup(
                   x => x.CallActivityWithRetryAsync<List<PredictionResult>>
                   (
                       ActivityFunctionsConstants.IdentifyStrayPetBreedWithStreamAsync,
                       It.IsAny<RetryOptions>(),
                       It.IsAny<string>()

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
        public async Task Does_HTTP_DurableClient_Return_Unsupported_MediaType_When_Content_Is_not_Form_Data()
        {

            var result = await _funcController.HttpUrlDurableClient(
                InstanceFactory.CreateHttpRequest(string.Empty
                , string.Empty), _durableClient.Object, InstanceFactory.CreateLogger());

            //Assertions
            result.Should().BeOfType<UnsupportedMediaTypeResult>();

            (result as UnsupportedMediaTypeResult).StatusCode.Should().Be(415);
        }

        [Fact]
        public async Task Does_Http_DurableClient_Return_Unsupported_MediaType_When_Content_Type_Is_Applicationjson()
        {
            var httpRequest = InstanceFactory.CreateHttpRequest(string.Empty
                , string.Empty);

            httpRequest.ContentType = "application/json";

            var result = await _funcController.HttpUrlDurableClient(
                httpRequest, _durableClient.Object, InstanceFactory.CreateLogger());

            //Assertions
            result.Should().BeOfType<UnsupportedMediaTypeResult>();

            (result as UnsupportedMediaTypeResult).StatusCode.Should().Be(415);
        }

        [Fact]
        public async Task Does_HTTP_DurableClient_Return_BadRequest_When_File_Content_Is_Not_JPEG_Or_PNG()
        {
            
            var formFields = new Dictionary<string, StringValues>()
                {
                    { "signalRUserId", "10234"}
                };
            var formFiles = new Dictionary<string, string>()
            {
                {@"../../../TestFiles/SomeRandomTextFile.txt", "text/plain"}
            };

            var httpRequest = InstanceFactory
                .CreateHttpRequest(string.Empty, string.Empty,
                formFields, formFiles);

            var result = await _funcController.HttpUrlDurableClient(
                httpRequest,
                _durableClient.Object,
                InstanceFactory.CreateLogger());

            result.Should().BeOfType<BadRequestObjectResult>();
            (result as BadRequestObjectResult).Value
                .Should().BeEquivalentTo("Only jpeg and png images are supported.");
        }

        
        [Fact]
        public async Task Does_HTTP_DurableClient_Return_BadRequest_When_CorrelationId_IsNotProvided()
        {
            var formFields = new Dictionary<string, StringValues>()
                {
                    { "signalRUserId", "10234"}
                };

            var formFiles = new Dictionary<string, string>()
            {
                {@"../../../TestFiles/StrayPuppy.jpg", "image/jpeg"}
            };
            
            var httpRequest = InstanceFactory
                .CreateHttpRequest(string.Empty, string.Empty,
                formFields, formFiles);

            var result = await _funcController.HttpUrlDurableClient(
                httpRequest,
                _durableClient.Object,
                InstanceFactory.CreateLogger());

            result.Should().BeOfType<BadRequestObjectResult>();
            (result as BadRequestObjectResult).Value
                .Should().BeEquivalentTo("CorrelationId field is mandatory.");

        }




    }
}
