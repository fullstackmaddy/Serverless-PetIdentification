using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Primitives;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Text;
using PetIdentification.Dtos;
using PetIdentification.Models;

namespace PetIdentification.Tests.Helpers
{
    public class TestFactory
    {
        public static List<PredictionResult> PredictedTags = new List<PredictionResult>()
        {
            new PredictionResult(){Probability = 1.0, TagName = "pug"}
                
        };

        public static List<AdoptionCentreDto> AdoptionCentreDtos = new List<AdoptionCentreDto>()
        {
            new AdoptionCentreDto(){
                Address = "Dummy Address",
                Name = "Pug Adoption Centre",
                ShelteredBreed = "pug",
                ZipCode = "ABC123"
            }

        };

        public static List<AdoptionCentre> AdoptionCentres = new List<AdoptionCentre>()
        {
            new AdoptionCentre(){
                Address = "Dummy Address",
                Name = "Pug Adoption Centre",
                ShelteredBreed = "pug",
                ZipCode = "ABC123"
            }
        };

        public static BreedInfo BreedInfo = new BreedInfo()
        {
            Breed = "pug",
            LifeExpectancy = "8 to 12 years",
            Qualities = "Some quality",
            Temprament = "Quiet, Loyal",
        };

        public static IConfigurationRoot BuildConfiguration(Dictionary<string, string> configurationItems)
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configurationItems)
                .Build();
            
            return configuration;

        }

        private static Dictionary<string, StringValues> CreateDictionary(string key, string value)
        {
            var qs = new Dictionary<string, StringValues>
            {
                { key, value }
            };
            return qs;
        }

        public static HttpRequest CreateHttpRequest(string queryStringKey, string queryStringValue)
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            
            request.Query = new QueryCollection(CreateDictionary(queryStringKey, queryStringValue));
            return request;
        }

        public static HttpRequest CreateHttpRequest(string queryStringKey, string queryStringValue, string jsonBody)
        {
            var context = new DefaultHttpContext();
            Stream s = new MemoryStream(
                Encoding.ASCII.GetBytes(jsonBody)
            );
            context.Request.Body = s;
            context.Request.ContentLength = s.Length;
            var request = context.Request;
            
            request.Query = new QueryCollection(CreateDictionary(queryStringKey, queryStringValue));
            return request;
        }

        public static ILogger CreateLogger(LoggerTypes type = LoggerTypes.Null)
        {
            ILogger logger;

            if (type == LoggerTypes.List)
            {
                logger = new ListLogger();
            }
            else
            {
                logger = NullLoggerFactory.Instance.CreateLogger("Null Logger");
            }

            return logger;
        }
    }
}