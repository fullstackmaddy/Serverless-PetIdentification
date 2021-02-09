using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PetIdentification.Constants;
using PetIdentification.Dtos;
using PetIdentification.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PetIdentification.Functions
{
    public class HttpFormDataDurableClientController
    {

        #region Properties&Fields

        private readonly IMapper _mapper;
        #endregion

        #region Constructors

        public HttpFormDataDurableClientController
        (
            IMapper mapper
        )
        {
            _mapper = mapper ??
            throw new ArgumentNullException(nameof(mapper));
        }

        #endregion

        #region Orchestration
        [FunctionName("HttpFormDataOrchestration")]
        public async Task<string> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context,
            ILogger logger
        )
        {

            var tuple = context.GetInput<(string, byte[])>();

            var correlationId = tuple.Item1;
            var imageBytes = tuple.Item2;

            try
            {
                logger.LogInformation(
                        new EventId((int)LoggingConstants.EventId.HttpFormDataOrchestrationStarted),
                        LoggingConstants.Template,
                        LoggingConstants.EventId.HttpFormDataOrchestrationStarted.ToString(),
                        correlationId,
                        LoggingConstants.ProcessingFunction.HttpFormDataOrchestration.ToString(),
                        LoggingConstants.FunctionType.Orchestration.ToString(),
                        LoggingConstants.ProcessStatus.Started.ToString(),
                        "Execution started."
                        );


                var retryOption = new RetryOptions(
                        firstRetryInterval: TimeSpan.FromMilliseconds(400),
                        maxNumberOfAttempts: 3
                    );
                
                List<PredictionResult> predictions;
                predictions = await context
                        .CallActivityWithRetryAsync<List<PredictionResult>>(
                            ActivityFunctionsConstants.IdentifyStrayPetBreedWithStreamAsync,
                            retryOption,
                            (correlationId,Convert.ToBase64String(imageBytes))
                        );

                var highestPrediction = predictions
                    .OrderByDescending(x => x.Probability).FirstOrDefault();

                if (highestPrediction == null)
                    throw new Exception("Unable to predict the tag");

                string tagName = highestPrediction.TagName;

                var adoptionCentres = await context.CallActivityAsync<List<AdoptionCentre>>(
                        ActivityFunctionsConstants.LocateAdoptionCentresByBreedAsync,
                        ( correlationId, tagName)
                    );
                var breedInfo = await context.CallActivityAsync<BreedInfo>(
                        ActivityFunctionsConstants.GetBreedInformationAsync,
                        (correlationId, tagName)
                    );

                var petIdentificationCanonical = new
                    PetIdentificationCanonical
                {
                    AdoptionCentres = adoptionCentres,
                    BreedInformation = breedInfo
                };

                var petIdentificationCanonicalDto = _mapper
                    .Map<PetIdentificationCanonical, PetIdentificationCanonicalDto>
                    (petIdentificationCanonical);

                logger.LogInformation(
                    new EventId((int)LoggingConstants.EventId.HttpFormDataOrchestrationFinished),
                    LoggingConstants.Template,
                    LoggingConstants.EventId.HttpFormDataOrchestrationFinished.ToString(),
                    correlationId,
                    LoggingConstants.ProcessingFunction.HttpFormDataOrchestration.ToString(),
                    LoggingConstants.FunctionType.Orchestration.ToString(),
                    LoggingConstants.ProcessStatus.Finished.ToString(),
                    "Execution Finished."
                    );

                return JsonConvert.SerializeObject(petIdentificationCanonicalDto);
            }
            catch (Exception ex)
            {

                logger.LogError(
                 new EventId((int)LoggingConstants.EventId.HttpFormDataOrchestrationFinished),
                 LoggingConstants.Template,
                 LoggingConstants.EventId.HttpFormDataOrchestrationFinished.ToString(),
                 correlationId,
                 LoggingConstants.ProcessingFunction.HttpFormDataOrchestration.ToString(),
                 LoggingConstants.FunctionType.Orchestration.ToString(),
                 LoggingConstants.ProcessStatus.Failed.ToString(),
                 string.Format("Execution failed. Exception {0}.", ex.ToString())
                 );

                return "Orchestrator failed in execution of the functions.";
            }

        }

        #endregion

        #region DurableClient
        [FunctionName("HttpFormDataDurableClient")]
        public async Task<IActionResult> HttpUrlDurableClient(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest request,
            [DurableClient] IDurableClient durableClient,
            ILogger logger
        )
        {

            if (!request.HasFormContentType)
            {
                return new UnsupportedMediaTypeResult();
            }

            FormFile file = request.Form.Files[0] as FormFile;

            List<string> allowedFileExtensions = new List<string>()
            {
                "image/jpeg",
                "image/png"
            };

            if (!allowedFileExtensions.Contains(file.ContentType))
                return new BadRequestObjectResult("Only jpeg and png images are supported."); ;

            if (string.IsNullOrEmpty(request.Form["correlationId"]))
                return new BadRequestObjectResult("CorrelationId field is mandatory.");


            var correlationId = Guid.ParseExact(request.Form["correlationId"], "D").ToString(); ;

            try
            {
                logger.LogInformation(
                new EventId((int)LoggingConstants.EventId.HttpFormDataDurableClientStarted),
                LoggingConstants.Template,
                LoggingConstants.EventId.HttpFormDataDurableClientStarted.ToString(),
                correlationId,
                LoggingConstants.ProcessingFunction.HttpFormDataDurableClient.ToString(),
                LoggingConstants.FunctionType.Client.ToString(),
                LoggingConstants.ProcessStatus.Started.ToString(),
                "Execution started."
                );

                var instanceId = Guid.NewGuid().ToString();

                await durableClient
                    .StartNewAsync("HttpFormDataOrchestration",
                    instanceId: instanceId,
                    (
                        correlationId,
                        await GetByteArrayFromFormFileAsync(file)
                        .ConfigureAwait(false)
                    ));

                var orchestrationStatus = await durableClient.GetStatusAsync(instanceId);

               

                while (orchestrationStatus.RuntimeStatus == OrchestrationRuntimeStatus.Running
                    || orchestrationStatus.RuntimeStatus == OrchestrationRuntimeStatus.Pending)
                {
                    await Task.Delay(300);
                    orchestrationStatus = await durableClient.GetStatusAsync(instanceId);

                }

                if (orchestrationStatus.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
                {
                    logger.LogInformation(
                        new EventId((int)LoggingConstants.EventId.HttpFormDataDurableClientFinished),
                        LoggingConstants.Template,
                        LoggingConstants.EventId.HttpFormDataDurableClientFinished.ToString(),
                        correlationId,
                        LoggingConstants.ProcessingFunction.HttpFormDataDurableClient.ToString(),
                        LoggingConstants.FunctionType.Client.ToString(),
                        LoggingConstants.ProcessStatus.Finished.ToString(),
                        "Execution finished."
                        );
                    return new ContentResult()
                    {
                        ContentType = "application/json",
                        Content = orchestrationStatus.Output.ToString(),
                        StatusCode = 200,
                    };
                }
                else
                
                {
                    throw new Exception("Error executing orchestration");
                }


            }
            catch (Exception ex)
            {
                logger.LogError(
                new EventId((int)LoggingConstants.EventId.HttpFormDataDurableClientFinished),
                LoggingConstants.Template,
                LoggingConstants.EventId.HttpFormDataDurableClientFinished.ToString(),
                correlationId,
                LoggingConstants.ProcessingFunction.HttpFormDataDurableClient.ToString(),
                LoggingConstants.FunctionType.Client.ToString(),
                LoggingConstants.ProcessStatus.Failed.ToString(),
                string.Format(
                    "Execution Failed. Exception {0}.", ex.ToString())
                );

                throw ex;

            }

        }

        #endregion

        #region PrivateMethods

        private async Task<byte[]> GetByteArrayFromFormFileAsync(IFormFile file)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                await file.CopyToAsync(ms)
                    .ConfigureAwait(false);

                return ms.ToArray();
            }
        }

        #endregion



    }
}
