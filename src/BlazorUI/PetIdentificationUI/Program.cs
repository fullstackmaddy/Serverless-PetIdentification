using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using PetIdentificationUI.Repositories.Implementations;
using PetIdentificationUI.Repositories.Interfaces;
using Azure.Storage.Blobs;

namespace PetIdentificationUI
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            builder.Services.AddScoped<IBlobRepository, BlobRepository>();

            builder.Services.AddSingleton<BlobContainerClient>(
                    new BlobContainerClient(

                        builder.Configuration
                        .GetSection("servicesUrls")["storageConnectionString"]
                        .ToString(),
                        "uploads"
                ));

            await builder.Build().RunAsync();
        }
    }
}
