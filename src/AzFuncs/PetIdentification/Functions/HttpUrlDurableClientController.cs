using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Extensions.Logging;
using PetIdentification.Interfaces;
using PetIdentification.Models;
using PetIdentification.Dtos;
using PetIdentification.Constants;
using System.Linq;
using AutoMapper;
using Newtonsoft.Json.Linq;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;


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
            logger.LogInformation("Starting the execution of the orchestration:HttpUrlOrchestration.");

            var durableReqDto = context.GetInput<DurableRequestDto>();

            //var imageUrl = context.GetInput<string>();

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

            await context.CallActivityAsync(ActivityFunctionsConstants.PushMessagesToSignalRHub,
                signalRRequest);

            logger.LogInformation("Finished execution of the orchestration");

            return "Orchestrator HttpUrlOrchestration executed the functions";

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
            logger.LogInformation("Started the execution of the HttpUrlDurableClient");
            if (string.IsNullOrWhiteSpace(request.ContentType) ||
               (request.ContentType != "application/json"))
            {
                return new UnsupportedMediaTypeResult();
            }


            var requestBody = string.Empty;

            using (StreamReader reader = new StreamReader(request.Body))
            {
                requestBody = await reader.ReadToEndAsync();
            }

            var durableReqDto = JsonConvert.DeserializeObject<DurableRequestDto>(requestBody);


            var result = await durableClient
                .StartNewAsync("HttpUrlOrchestration", instanceId: new Guid().ToString(), durableReqDto);

            return new AcceptedResult();

        }

        #endregion




    }
}
