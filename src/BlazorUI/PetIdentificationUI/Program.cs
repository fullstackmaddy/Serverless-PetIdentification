using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using PetIdentificationUI.Repositories.Implementations;
using PetIdentificationUI.Repositories.Interfaces;
using Azure.Storage.Blobs;
using PetIdentificationUI.HttpClients;

namespace PetIdentificationUI
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            builder.Services.AddHttpClient<HttpAzureFunctionsClient>(
                    client =>
                    client.BaseAddress = new Uri("https://petidentification20210209190334.azurewebsites.net")
                ); ;

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            builder.Services.AddScoped<IBlobRepository, BlobRepository>();

            builder.Services.AddSingleton<BlobServiceClient>(
                    new BlobServiceClient(
                        builder.Configuration
                        ["storageConnectionString"]
                        .ToString())
                );

            
            await builder.Build().RunAsync();
        }
    }
}
