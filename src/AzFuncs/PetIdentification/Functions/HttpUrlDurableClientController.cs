using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using PetIdentification.Models;
using PetIdentification.Dtos;
using PetIdentification.Constants;
using System.Linq;
using AutoMapper;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Net.Mime;

namespace PetIdentification.Functions
{
    public class HttpUrlDurableClientController
    {
        #region Properties&Fields
        
        private readonly IMapper _mapper;

        #endregion

        #region Constructors

        public HttpUrlDurableClientController
        (
            IMapper mapper
        )
        {
            _mapper = mapper ??
            throw new ArgumentNullException(nameof(mapper));
        }

        #endregion

        #region Orchestration
        [FunctionName("HttpUrlOrchestration")]
        public async Task<string> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context,
            ILogger logger
        )
        {
            var durableReqDto = context.GetInput<DurableRequestDto>();
            var correlationId = durableReqDto.CorrelationId;

            try
            {
                logger.LogInformation(
                        new EventId((int)LoggingConstants.EventId.HttpUrlOrchestrationStarted),
                        LoggingConstants.Template,
                        LoggingConstants.EventId.HttpUrlOrchestrationStarted.ToString(),
                        correlationId,
                        LoggingConstants.ProcessingFunction.HttpUrlOrchestration.ToString(),
                        LoggingConstants.FunctionType.Orchestration.ToString(),
                        LoggingConstants.ProcessStatus.Started.ToString(),
                        "Execution started."
                        );

                var retryOption = new RetryOptions(
                       firstRetryInterval: TimeSpan.FromMilliseconds(400),
                       maxNumberOfAttempts: 3
                   );


                var predictions = await context.CallActivityWithRetryAsync<List<PredictionResult>>
                    (ActivityFunctionsConstants.IdentifyStrayPetBreedWithUrlAsync,
                    retryOption,
                    (correlationId,
                    durableReqDto.BlobUrl.AbsoluteUri));


                var highestPrediction = predictions.OrderByDescending(x => x.Probability).FirstOrDefault();

                string tagName = highestPrediction.TagName;

                var adoptionCentres = await context.CallActivityAsync<List<AdoptionCentre>>(
                        ActivityFunctionsConstants.LocateAdoptionCentresByBreedAsync,
                        (correlationId, tagName)
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

                var signalRRequest = new SignalRRequest()
                {
                    Message = JsonConvert.SerializeObject(petIdentificationCanonicalDto),
                    UserId = durableReqDto.SignalRUserId,
                    CorrelationId = correlationId
                };

                await context.CallActivityAsync(ActivityFunctionsConstants.PushMessagesToSignalRHub,
                    signalRRequest);

                logger.LogInformation(
                   new EventId((int)LoggingConstants.EventId.HttpUrlOrchestrationFinished),
                   LoggingConstants.Template,
                   LoggingConstants.EventId.HttpUrlOrchestrationFinished.ToString(),
                   correlationId,
                   LoggingConstants.ProcessingFunction.HttpUrlOrchestration.ToString(),
                   LoggingConstants.FunctionType.Orchestration.ToString(),
                   LoggingConstants.ProcessStatus.Finished.ToString(),
                   "Execution Finished."
                   );

                return "Orchestrator sucessfully executed the functions.";
            }
            catch (Exception ex)
            {

                logger.LogError(
                   new EventId((int)LoggingConstants.EventId.HttpUrlOrchestrationFinished),
                   LoggingConstants.Template,
                   LoggingConstants.EventId.HttpUrlOrchestrationFinished.ToString(),
                   correlationId,
                   LoggingConstants.ProcessingFunction.HttpUrlOrchestration.ToString(),
                   LoggingConstants.FunctionType.Orchestration.ToString(),
                   LoggingConstants.ProcessStatus.Failed.ToString(),
                   string.Format("Execution failed. Exception {0}.", ex.ToString())
                   );
                return "Orchestrator failed in execution of the functions.";
            } 

        }

        #endregion

        #region DurableClient
        [FunctionName("HttpUrlDurableClient")]
        public async Task<IActionResult> HttpUrlDurableClient(
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest request,
            [DurableClient] IDurableClient durableClient,
            ILogger logger
        )
        {
            
            if (string.IsNullOrWhiteSpace(request.ContentType)
               )
            {
                return new UnsupportedMediaTypeResult();
            }

            var contentType = new ContentType(request.ContentType);

            if (contentType.MediaType != "application/json")
            {
                return new UnsupportedMediaTypeResult();
            }



            DurableRequestDto durableReqDto;
            var requestBody = string.Empty;

            using (StreamReader reader = new StreamReader(request.Body))
            {
                requestBody = await reader.ReadToEndAsync();
            }

            try
            {
                durableReqDto = JsonConvert.DeserializeObject<DurableRequestDto>(requestBody);
            }
            catch (Exception)
            {

                return new BadRequestObjectResult("Mandatory fields not provided.");
            }

           var correlationId = durableReqDto.CorrelationId;

            try
            {
                
                

                logger.LogInformation(
                    new EventId((int)LoggingConstants.EventId.HttpUrlDurableClientStarted),
                    LoggingConstants.Template,
                    LoggingConstants.EventId.HttpUrlDurableClientStarted.ToString(),
                    correlationId,
                    LoggingConstants.ProcessingFunction.HttpUrlDurableClient.ToString(),
                    LoggingConstants.FunctionType.Client.ToString(),
                    LoggingConstants.ProcessStatus.Started.ToString(),
                    "Execution started."
                    );


                await durableClient
                    .StartNewAsync("HttpUrlOrchestration", 
                    instanceId: Guid.NewGuid().ToString(), durableReqDto)
                    .ConfigureAwait(false);

                logger.LogInformation(
                   new EventId((int)LoggingConstants.EventId.HttpUrlDurableClientFinished),
                   LoggingConstants.Template,
                   LoggingConstants.EventId.HttpUrlDurableClientFinished.ToString(),
                   correlationId,
                   LoggingConstants.ProcessingFunction.HttpUrlDurableClient.ToString(),
                   LoggingConstants.FunctionType.Client.ToString(),
                   LoggingConstants.ProcessStatus.Finished.ToString(),
                   "Execution Finished."
                   );

                return new AcceptedResult();
            }
            catch (Exception ex)
            {
                logger.LogError(
                   new EventId((int)LoggingConstants.EventId.HttpUrlDurableClientFinished),
                   LoggingConstants.Template,
                   LoggingConstants.EventId.HttpUrlDurableClientFinished.ToString(),
                   correlationId,
                   LoggingConstants.ProcessingFunction.HttpUrlDurableClient.ToString(),
                   LoggingConstants.FunctionType.Client.ToString(),
                   LoggingConstants.ProcessStatus.Failed.ToString(),
                   string.Format("Execution Failed. Exception {0}", ex.ToString())
                   );

                throw;
            }

        }

        #endregion




    }
}
