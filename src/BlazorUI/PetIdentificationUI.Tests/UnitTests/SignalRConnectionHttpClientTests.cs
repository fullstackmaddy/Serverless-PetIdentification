using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using FluentAssertions;
using System.Net.Http;
using Moq;
using System.Net;
using PetIdentificationUI.Tests.Helpers;
using Moq.Protected;
using System.Threading.Tasks;
using System.Threading;
using PetIdentificationUI.HttpClients;

namespace PetIdentificationUI.Tests.UnitTests
{
    public class SignalRConnectionHttpClientTests
    {
        private readonly Mock<HttpMessageHandler> _mockHandler;

        private readonly SignalRConnectionHttpClient _signalRConnectionHttpClient;

        public SignalRConnectionHttpClientTests()
        {
            _mockHandler = new Mock<HttpMessageHandler>();

            var response = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(InstanceFactory.ConnectionInfo)

            };

            _mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            HttpClient client = new HttpClient(_mockHandler.Object)
            {
                BaseAddress = new Uri("http://localhost")
            };

            _signalRConnectionHttpClient = new SignalRConnectionHttpClient(client);

        }

        [Fact]
        public async Task Is_ConnectionInfo_Serialiazed_Properly()
        {

            //Act
            var result = await _signalRConnectionHttpClient
                    .GetHubConnectionInformationAsync(userId: "123");

            //Assert

            result.AccessToken.Should().BeEquivalentTo("dummyToken");

            result.Url.Should().BeEquivalentTo("http://localhost/dummyUrl");


        }

    }
}
