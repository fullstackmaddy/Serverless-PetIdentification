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
    public class HttpAzureFunctionsClientTests
    {
        private readonly Mock<HttpMessageHandler> _mockHandler;

        private readonly HttpAzureFunctionsClient _httpAzureFunctionsClient;

        public HttpAzureFunctionsClientTests()
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

            _httpAzureFunctionsClient = new HttpAzureFunctionsClient(client);

        }

        [Fact]
        public async Task Is_ConnectionInfo_Serialiazed_Properly()
        {

            //Act
            var result = await _httpAzureFunctionsClient
                    .GetHubConnectionInformationAsync(userId: "123");

            //Assert

            result.AccessToken.Should().BeEquivalentTo("dummyToken");

            result.Url.Should().BeEquivalentTo("http://localhost/dummyUrl");


        }
    }
}
