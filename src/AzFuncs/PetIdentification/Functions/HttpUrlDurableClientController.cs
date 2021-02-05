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

namespace PetIdentification.Functions
{
    public class HttpUrlDurableClientController
    {
        #region Properties&Fields
        
        private readonly IMapper _mapper;

        private string _correlationId;
        
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
            try
            {
                logger.LogInformation(
                        new EventId((int)LoggingConstants.EventId.HttpUrlOrchestrationStarted),
                        LoggingConstants.Template,
                        LoggingConstants.EventId.HttpUrlOrchestrationStarted.ToString(),
                        _correlationId,
                        LoggingConstants.ProcessingFunction.HttpUrlOrchestration.ToString(),
                        LoggingConstants.FunctionType.Orchestration.ToString(),
                        LoggingConstants.ProcessStatus.Started.ToString(),
                        "Execution started."
                        );

                var durableReqDto = context.GetInput<DurableRequestDto>();



                var predictions = await context.CallActivityAsync<List<PredictionResult>>
                (ActivityFunctionsConstants.IdentifyStrayPetBreedWithUrlAsync,
                durableReqDto.BlobUrl.ToString());
                

                var highestPrediction = predictions.OrderBy(x => x.Probability).FirstOrDefault();

                string tagName = highestPrediction.TagName;

                var adoptionCentres = await context.CallActivityAsync<List<AdoptionCentre>>(
                        ActivityFunctionsConstants.LocateAdoptionCentresByBreedAsync,
                        tagName
                    );



                var breedInfo = await context.CallActivityAsync<BreedInfo>(
                        ActivityFunctionsConstants.GetBreedInformationAsync,
                        tagName
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
                    UserId = durableReqDto.SignalRUserId
                };

                await context.CallActivityAsync(ActivityFunctionsConstants.PushMessagesToSignalRHub,
                    signalRRequest);

                logger.LogInformation(
                   new EventId((int)LoggingConstants.EventId.HttpUrlOrchestrationFinished),
                   LoggingConstants.Template,
                   LoggingConstants.EventId.HttpUrlOrchestrationFinished.ToString(),
                   _correlationId,
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
                   _correlationId,
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
            
            if (string.IsNullOrWhiteSpace(request.ContentType) ||
               (request.ContentType != "application/json"))
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

           _correlationId = durableReqDto.CorrelationId;

            try
            {
                
                

                logger.LogInformation(
                    new EventId((int)LoggingConstants.EventId.HttpUrlDurableClientStarted),
                    LoggingConstants.Template,
                    LoggingConstants.EventId.HttpUrlDurableClientStarted.ToString(),
                    _correlationId,
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
                   _correlationId,
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
                   _correlationId,
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
