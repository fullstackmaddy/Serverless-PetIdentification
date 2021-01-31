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
    public class HttpFormDataDurableClientControllerTests
    {
        private readonly IMapper _mapper;
        private readonly Mock<IDurableOrchestrationContext> _orchestrationContext;
        private HttpFormDataDurableClientController _funcController;
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
                x => x.CallActivityAsync<List<PredictionResult>>
                (ActivityFunctionsConstants.IdentifyStrayPetBreedAsync, It.IsAny<string>())

            ).ReturnsAsync(InstanceFactory.PredictedTags);

            _orchestrationContext.Setup(
                x => x.CallActivityAsync<List<AdoptionCentre>>(
                        ActivityFunctionsConstants.LocateAdoptionCentresByBreedAsync,
                        It.IsAny<string>())
                ).ReturnsAsync(InstanceFactory.AdoptionCentres);

            _orchestrationContext.Setup(
                x => x.CallActivityAsync<bool>("PushMessagesToSignalRHub",
                    It.IsAny<SignalRRequest>())
                ).ReturnsAsync(true);

            _funcController = new HttpFormDataDurableClientController(
                _mapper);

            _durableClient = new Mock<IDurableClient>();
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

    }
}
