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
using System.Linq;
using System.Threading.Tasks;

namespace PetIdentification.Functions
{

    public class EventGridDurableClientController
    {

        private readonly IMapper _mapper;

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
            logger.LogInformation("Starting the execution of the orchestration: EventGridDurableOrchestration");

            var durableReqDto = context.GetInput<DurableRequestDto>();

            var predictions = await context.CallActivityAsync<List<PredictionResult>>
            (ActivityFunctionsConstants.IdentifyStrayPetBreedAsync, 
            durableReqDto.BlobUrl.ToString());

            var highestPrediction = predictions.OrderBy(x => x.Probability).FirstOrDefault();

            string tagName = highestPrediction.TagName;

            Task<List<AdoptionCentre>> getAdoptionCentres = context.CallActivityAsync<List<AdoptionCentre>>(
                    ActivityFunctionsConstants.LocateAdoptionCentresByBreedAsync,
                    tagName
                );

            Task<BreedInfo> getBreedInfo = context.CallActivityAsync<BreedInfo>(
                    ActivityFunctionsConstants.GetBreedInformationASync,
                    tagName
                );

            await Task.WhenAll(getAdoptionCentres, getBreedInfo);

            var petIdentificationCanonical = new
                PetIdentificationCanonical
            {
                AdoptionCentres = getAdoptionCentres.Result,
                BreedInformation = getBreedInfo.Result
            };

            var petIdentificationCanonicalDto = _mapper
                .Map<PetIdentificationCanonical, PetIdentificationCanonicalDto>
                (petIdentificationCanonical);

            var signalRRequest = new SignalRRequest()
            {
                Message = JsonConvert.SerializeObject(petIdentificationCanonicalDto),
                UserId = durableReqDto.SignalRUserId
            };

            await context.CallActivityAsync(ActivityFunctionsConstants.PushMessagesToSignalRHub, signalRRequest);

            logger.LogInformation("Finished execution of the orchestration: EventGridDurableOrchestration");

            return "Orchestrator EventGridDurableOrchestration executed the functions";

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
            logger.LogInformation("Started the execution of the event grid triggered durable orchestration module.");
            StorageBlobCreatedEventData blobCreatedEventData =
                ((JObject)eventGridEvent.Data).ToObject<StorageBlobCreatedEventData>();

            var result = await client
            .StartNewAsync("EventGridDurableOrchestration", instanceId: new Guid().ToString(), blobCreatedEventData.Url);

        }
        #endregion
    }
}
