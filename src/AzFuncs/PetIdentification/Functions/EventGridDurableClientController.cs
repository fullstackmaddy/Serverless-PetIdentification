using AutoMapper;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

    public class EventGridDurableClientController
    {

        private readonly IMapper _mapper;

        private string _signalRUserId;

        public EventGridDurableClientController(IMapper mapper)
        {
            _mapper = mapper ??
           throw new ArgumentNullException(nameof(mapper));

        }

        #region Orchestration
        [FunctionName("EventGridDurableOrchestration")]
        public async Task<string> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context,
            ILogger logger
        )
        {
            var tuple = context.GetInput<(string, string)>();

            var correlationId = tuple.Item1;
            var imageBlobUrl = tuple.Item2;
            
            try
            {
                logger.LogInformation(
                new EventId((int)LoggingConstants.EventId.EventGridDurableOrchestrationStarted),
                LoggingConstants.Template,
                LoggingConstants.EventId.EventGridDurableOrchestrationStarted.ToString(),
                correlationId,
                LoggingConstants.ProcessingFunction.EventGridDurableOrchestration.ToString(),
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
                    (correlationId,imageBlobUrl));
                
                _signalRUserId = await context.CallActivityAsync<string>(
                        ActivityFunctionsConstants.GetSignalUserIdFromBlobMetadataAsync,
                        (correlationId, imageBlobUrl)
                    );

                var highestPrediction = predictions.OrderByDescending(x => x.Probability).FirstOrDefault();

                string tagName = highestPrediction.TagName;

                var adoptionCentres = await context.CallActivityAsync<List<AdoptionCentre>>(
                        ActivityFunctionsConstants.LocateAdoptionCentresByBreedAsync,
                        (correlationId,tagName)
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
                    UserId = _signalRUserId,
                    CorrelationId = correlationId
                };

                await context.CallActivityAsync(ActivityFunctionsConstants.PushMessagesToSignalRHub, signalRRequest);

                logger.LogInformation(
                new EventId((int)LoggingConstants.EventId.EventGridDurableOrchestrationFinsihed),
                LoggingConstants.Template,
                LoggingConstants.EventId.EventGridDurableOrchestrationFinsihed.ToString(),
                correlationId,
                LoggingConstants.ProcessingFunction.EventGridDurableOrchestration.ToString(),
                LoggingConstants.FunctionType.Orchestration.ToString(),
                LoggingConstants.ProcessStatus.Finished.ToString(),
                "Execution finished."
                );

                return "Orchestrator sucessfully executed the functions.";

            }
            catch (Exception ex)
            {
                logger.LogError(
                new EventId((int)LoggingConstants.EventId.EventGridDurableOrchestrationFinsihed),
                LoggingConstants.Template,
                LoggingConstants.EventId.EventGridDurableOrchestrationFinsihed.ToString(),
                correlationId,
                LoggingConstants.ProcessingFunction.EventGridDurableOrchestration.ToString(),
                LoggingConstants.FunctionType.Orchestration.ToString(),
                LoggingConstants.ProcessStatus.Failed.ToString(),
                string.Format("Execution failed. Exception {0}.", ex.ToString())
                );

                return "Orchestrator failed in execution of the functions.";
            }
            

        }

        #endregion

        #region DurableClient
        [FunctionName("EventGridDurableClient")]
        public async Task EventGridDurableClient(
            [EventGridTrigger] EventGridEvent eventGridEvent,
            [DurableClient] IDurableClient client,
            ILogger logger
        )
        {
            StorageBlobCreatedEventData blobCreatedEventData =
               ((JObject)eventGridEvent.Data).ToObject<StorageBlobCreatedEventData>();

            var correlationId = GetBlobName(blobCreatedEventData.Url);

            try
            {
               

                logger.LogInformation(
                new EventId((int)LoggingConstants.EventId.EventGridDurableClientStarted),
                LoggingConstants.Template,
                LoggingConstants.EventId.EventGridDurableClientStarted.ToString(),
                correlationId,
                LoggingConstants.ProcessingFunction.EventGridDurableClient.ToString(),
                LoggingConstants.FunctionType.Client.ToString(),
                LoggingConstants.ProcessStatus.Started.ToString(),
                "Execution started."
                );


                await client
                 .StartNewAsync("EventGridDurableOrchestration", instanceId: Guid.NewGuid().ToString(),
                 (correlationId, blobCreatedEventData.Url))
                 .ConfigureAwait(false);

                logger.LogInformation(
                new EventId((int)LoggingConstants.EventId.EventGridDurableClientFinished),
                LoggingConstants.Template,
                LoggingConstants.EventId.EventGridDurableClientFinished.ToString(),
                correlationId,
                LoggingConstants.ProcessingFunction.EventGridDurableClient.ToString(),
                LoggingConstants.FunctionType.Client.ToString(),
                LoggingConstants.ProcessStatus.Finished.ToString(),
                "Execution started."
                );
            }
            catch (Exception ex)
            {
                logger.LogError(
                new EventId((int)LoggingConstants.EventId.EventGridDurableClientFinished),
                LoggingConstants.Template,
                LoggingConstants.EventId.EventGridDurableClientFinished.ToString(),
                correlationId,
                LoggingConstants.ProcessingFunction.EventGridDurableClient.ToString(),
                LoggingConstants.FunctionType.Client.ToString(),
                LoggingConstants.ProcessStatus.Failed.ToString(),
                string.Format(
                    "Execution Failed. Exception {0}.", ex.ToString())
                );

            }
            

        }


        public string GetBlobName(string blobUrl)
        {
           
            return Path.GetFileNameWithoutExtension(blobUrl);
           
        }
        #endregion
    }
}
