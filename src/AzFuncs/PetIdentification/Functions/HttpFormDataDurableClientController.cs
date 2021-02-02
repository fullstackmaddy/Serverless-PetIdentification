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
using System.Linq;
using System.Threading.Tasks;

namespace PetIdentification.Functions
{
    public class HttpFormDataDurableClientController
    {

        #region Properties&Fields

        private readonly IMapper _mapper;

        private string _signalRUserId;

        private string _correlationId;

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
            try
            {
                logger.LogInformation(
                        new EventId((int)LoggingConstants.EventId.HttpFormDataOrchestrationStarted),
                        LoggingConstants.Template,
                        LoggingConstants.EventId.HttpFormDataOrchestrationStarted.ToString(),
                        _correlationId,
                        LoggingConstants.ProcessingFunction.HttpFormDataOrchestration.ToString(),
                        LoggingConstants.FunctionType.Orchestration.ToString(),
                        LoggingConstants.ProcessStatus.Started.ToString(),
                        "Execution started."
                        );

                var durableReqDto = context.GetInput<DurableRequestDto>();

                //var imageUrl = context.GetInput<string>();

                var predictions = await context.CallActivityAsync<List<PredictionResult>>
                (
                    ActivityFunctionsConstants.IdentifyStrayPetBreedWithUrlAsync,
                    durableReqDto.BlobUrl.ToString());

                var highestPrediction = predictions.OrderBy(x => x.Probability).FirstOrDefault();

                string tagName = highestPrediction.TagName;

                Task<List<AdoptionCentre>> getAdoptionCentres = context.CallActivityAsync<List<AdoptionCentre>>(
                        ActivityFunctionsConstants.LocateAdoptionCentresByBreedAsync,
                        tagName
                    );

                Task<BreedInfo> getBreedInfo = context.CallActivityAsync<BreedInfo>(
                        ActivityFunctionsConstants.GetBreedInformationAsync,
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

                await context.CallActivityAsync("PushMessagesToSignalRHub", signalRRequest);

                logger.LogInformation(
                    new EventId((int)LoggingConstants.EventId.HttpFormDataOrchestrationFinished),
                    LoggingConstants.Template,
                    LoggingConstants.EventId.HttpFormDataOrchestrationFinished.ToString(),
                    _correlationId,
                    LoggingConstants.ProcessingFunction.HttpFormDataOrchestration.ToString(),
                    LoggingConstants.FunctionType.Orchestration.ToString(),
                    LoggingConstants.ProcessStatus.Finished.ToString(),
                    "Execution Finished."
                    );

                return "Orchestrator sucessfully executed the functions.";
            }
            catch (Exception ex)
            {

                logger.LogError(
                 new EventId((int)LoggingConstants.EventId.HttpFormDataOrchestrationFinished),
                 LoggingConstants.Template,
                 LoggingConstants.EventId.HttpFormDataOrchestrationFinished.ToString(),
                 _correlationId,
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
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest request,
            [DurableClient] IDurableClient durableClient,
            ILogger logger
        )
        {
            
            if (!request.HasFormContentType)
            {
                return new UnsupportedMediaTypeResult();
            }

            var file = request.Form.Files[0];
            _signalRUserId = request.Form["signalRUserId"];
            _correlationId = request.Form["correlationId"];

            List<string> allowedFileExtensions = new List<string>()
            {
                "image/jpeg",
                "image/png"
            };

            if (!allowedFileExtensions.Contains(file.ContentType))
                return new BadRequestObjectResult("Only jpeg and png images are supported."); ;

            if (string.IsNullOrWhiteSpace(_signalRUserId))
                return new BadRequestObjectResult("SignalRUserId field is mandatory.");

            if(string.IsNullOrEmpty(_correlationId))
                return new BadRequestObjectResult("CorrelationId field is mandatory.");


            try
            {
                logger.LogInformation(
                new EventId((int)LoggingConstants.EventId.HttpFormDataDurableClientStarted),
                LoggingConstants.Template,
                LoggingConstants.EventId.HttpFormDataDurableClientStarted.ToString(),
                _correlationId,
                LoggingConstants.ProcessingFunction.HttpFormDataDurableClient.ToString(),
                LoggingConstants.FunctionType.Client.ToString(),
                LoggingConstants.ProcessStatus.Started.ToString(),
                "Execution started."
                );

                await durableClient
                    .StartNewAsync("HttpFormDataOrchestration", instanceId: new Guid().ToString(), file);
                
                logger.LogInformation(
                new EventId((int)LoggingConstants.EventId.HttpFormDataDurableClientFinished),
                LoggingConstants.Template,
                LoggingConstants.EventId.HttpFormDataDurableClientFinished.ToString(),
                _correlationId,
                LoggingConstants.ProcessingFunction.HttpFormDataDurableClient.ToString(),
                LoggingConstants.FunctionType.Client.ToString(),
                LoggingConstants.ProcessStatus.Finished.ToString(),
                "Execution finished."
                );
                
                return new AcceptedResult();
            }
            catch (Exception ex)
            {
                logger.LogError(
                new EventId((int)LoggingConstants.EventId.HttpFormDataDurableClientFinished),
                LoggingConstants.Template,
                LoggingConstants.EventId.HttpFormDataDurableClientFinished.ToString(),
                _correlationId,
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



    }
}
