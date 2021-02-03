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
using AutoMapper;
using PetIdentification.Profiles;
using System;

namespace PetIdentification.Tests.Helpers
{
    public class InstanceFactory
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

        

        public static IMapper CreateMapper()
        {
            var adoptionCentreProfile = new AdoptionCentreProfile();
            var breedInfoProfile = new BreedInfoProfile();
            
            var profiles = new List<Profile>()
            {
                new PredictionResultProfile(),
                new AdoptionCentreProfile(),
                new BreedInfoProfile(),
                new PetIdentificationCanonicalProfile()
            };

            var config = new MapperConfiguration(x => x.AddProfiles(profiles));

            return config.CreateMapper();
        }

        public static Exception Exception = new Exception("Exception thrown for testing pruposes.");

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

        /// <summary>
        /// Returns an instance of the HttpRequest with form data in it.
        /// </summary>
        /// <param name="queryStringKey"></param>
        /// <param name="queryStringValue"></param>
        /// <param name="formFields">Key value pairs to be sent into headers</param>
        /// <param name="formFiles">Key value pairs for the fully qualified path for the file
        /// and the content type of the file.
        /// </param>
        /// <returns></returns>
        public static HttpRequest CreateHttpRequest(string queryStringKey, string queryStringValue,
            Dictionary<string, StringValues> formFields, Dictionary<string, string> formFiles)
        { 
            var context = new DefaultHttpContext();
            var request = context.Request;

            var formFileCollection = new FormFileCollection();
            int i = 0;
            foreach (var file in formFiles.Keys)
            {

                
                FileInfo fi = new FileInfo(file);
                FileStream fs = new FileStream(fi.FullName,
                    FileMode.Open, FileAccess.Read);

                formFileCollection.Add(
                    new FormFile(
                            baseStream: fs,
                            baseStreamOffset: 0,
                            length: fs.Length,
                            name: string.Format("File{0}", i.ToString()),
                            fileName: fi.Name
                        )
                        {
                            Headers = new HeaderDictionary(),
                            ContentType = formFiles[file]
                       
                        }
                    );
                fs.Dispose();
                i++;

            }

            var formCollection = new FormCollection(formFields, formFileCollection);
            request.ContentType = "multipart/form-data; boundary=--------------------------689444905826361168275961";
            request.Form = formCollection; ;
            request.Query = new QueryCollection(CreateDictionary(queryStringKey, queryStringValue));
            return request;
        
        }


        public static IFormFile CreateUploadFile()
        {
           
            FileInfo fi = new FileInfo(@"../../../TestFiles/StrayPuppy.jpg");
            FileStream fs = new FileStream(fi.FullName,
                FileMode.Open, FileAccess.Read);

            var file = new FormFile(
                            baseStream: fs,
                            baseStreamOffset: 0,
                            length: fs.Length,
                            name: "File",
                            fileName: fi.Name
                        )
                        {
                            Headers = new HeaderDictionary(),
                            ContentType = "image/jpeg"

                        };
            
            return file;
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