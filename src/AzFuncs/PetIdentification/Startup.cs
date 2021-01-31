using System;
using AutoMapper;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using PetIdentification.Helpers;
using PetIdentification.Interfaces;

[assembly: FunctionsStartup(typeof(PetIdentification.Startup))]
namespace PetIdentification
{
    public class Startup: FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
           
            builder.Services.AddSingleton<IDocumentClient>(
                x => new DocumentClient(
                    new Uri(
                        Environment.GetEnvironmentVariable("CosmosDBUri")
                    ),
                    Environment.GetEnvironmentVariable("CosmosDBAuthKey")
                )
            );
            builder.Services.AddSingleton<ICustomVisionPredictionClient>(
                x => new CustomVisionPredictionClient(
                    new ApiKeyServiceClientCredentials(
                        System.Environment.GetEnvironmentVariable("CustomVisionKey")
                    )
                )
                {
                    Endpoint = 
                    System.Environment.GetEnvironmentVariable("CustomVisionEndPoint")
                }
            );
            builder.Services.AddTransient<IAdoptionCentreDbHelper, CosmosAdoptionCentreDbHelper>();
            builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            builder.Services.AddTransient<IPredictionHelper, CustomVisionPredictionHelper>();
        }
    }
}
