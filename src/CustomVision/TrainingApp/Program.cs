using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace TrainingApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var provider = BuildServiceProvider();

            Console.WriteLine("Starting the application");


            try
            {
                var imagesParentDirectory = new FileInfo("../../../../Data/training-images");
                var customVisionHelper = provider.GetService<ICustomVisionHelper>();

                Console.WriteLine("Creating the project");

                await customVisionHelper.CreateProject(
                        name: CustomVisionConstants.ProjectName,
                        description: CustomVisionConstants.ProjectDescription
                    );

                Console.WriteLine("Finished Creating project");

                Console.WriteLine("Uploading images in the batchs");

                await customVisionHelper.BatchUploadImages(imagesParentDirectory.FullName);


                Console.WriteLine("Finished uploading images");

                Console.WriteLine("Training the project");

                await customVisionHelper.TrainProject();

                Console.WriteLine("Project training complete");

                Console.WriteLine("Publishing iteration");

                await customVisionHelper
                    .PublishIteration(CustomVisionConstants.PredictionResourceId);

                Console.WriteLine("Iteration published");

                Console.WriteLine("Work Finished. Press any key to continue...");
                

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occured");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("Press any key to continue...");
            }

            Console.ReadKey();

        }

        static IServiceProvider BuildServiceProvider()
        {
            var collection = new ServiceCollection()
                .AddLogging(
                    builder =>
                    builder.AddConsole()
                )
                .AddSingleton<ICustomVisionTrainingClient>(
                    new CustomVisionTrainingClient(
                        new Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.ApiKeyServiceClientCredentials
                        (CustomVisionConstants.TrainingKey)
                        )
                    { Endpoint = CustomVisionConstants.TrainingEndpoint }

                    )
                .AddSingleton<ICustomVisionPredictionClient>(
                    new CustomVisionPredictionClient(
                        new Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.ApiKeyServiceClientCredentials
                        (CustomVisionConstants.PredictionKey)
                        )
                    { Endpoint = CustomVisionConstants.PredictionEndpoint }
                    )
                .AddSingleton<ICustomVisionHelper, CustomVisionHelper>();

            return collection.BuildServiceProvider();
        }
    }
}
