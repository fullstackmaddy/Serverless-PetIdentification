using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using PetIdentification.Constants;
using PetIdentification.Interfaces;
using PetIdentification.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PetIdentification.Functions
{
    public class ActivityFunctionsController
    {
        #region Properties
        private readonly IAdoptionCentreDbHelper _adoptionCentreDbHelper;
        private readonly IPredictionHelper _predictionHelper;
        private readonly IBreedInfoDbHelper _breedInfoDbHelper;
        private readonly IBlobHelper _blobHelper;

        private string _correlationId;

        public string CorrelationId 
        {
            get {
                return _correlationId;
            }
            set
            {
                _correlationId = value;
            }
        }

        #endregion

        #region Constructors

        public ActivityFunctionsController
        (
            IAdoptionCentreDbHelper adoptionCentreDbHelper,
            IBreedInfoDbHelper breedInfoDbHelper,
            IPredictionHelper predictionHelper,
            IBlobHelper blobHelper
        )
        {

            _adoptionCentreDbHelper = adoptionCentreDbHelper ??
            throw new ArgumentNullException(nameof(adoptionCentreDbHelper));
            _predictionHelper = predictionHelper ??
            throw new ArgumentNullException(nameof(predictionHelper));
            _breedInfoDbHelper = breedInfoDbHelper ??
                throw new ArgumentNullException(nameof(breedInfoDbHelper));
            _blobHelper = blobHelper ??
                throw new ArgumentNullException(nameof(blobHelper));
        }

        #endregion

        #region ActivityFunctions
        [FunctionName(ActivityFunctionsConstants.IdentifyStrayPetBreedWithUrlAsync)]
        public async Task<List<PredictionResult>> IdentifyStrayPetBreedWithUrlAsync(
            [ActivityTrigger] string imageUrl,
            ILogger logger)
        {
            
            logger.LogInformation(
                new EventId((int)LoggingConstants.EventId.IdentifyStrayPetBreedWithUrlAsyncStarted),
                LoggingConstants.Template,
                LoggingConstants.EventId.IdentifyStrayPetBreedWithUrlAsyncStarted.ToString(),
                _correlationId,
                LoggingConstants.ProcessingFunction.IdentifyStrayPetBreedWithUrlAsync.ToString(),
                LoggingConstants.FunctionType.Activity.ToString(),
                LoggingConstants.ProcessStatus.Started.ToString(),
                "Execution finished."
                );

            var result = await _predictionHelper.PredictBreedAsync(imageUrl).ConfigureAwait(false);

            logger.LogInformation(
                new EventId((int)LoggingConstants.EventId.IdentifyStrayPetBreedWithUrlAsyncFinished),
                LoggingConstants.Template,
                LoggingConstants.EventId.IdentifyStrayPetBreedWithUrlAsyncFinished.ToString(),
                _correlationId,
                LoggingConstants.ProcessingFunction.IdentifyStrayPetBreedWithUrlAsync.ToString(),
                LoggingConstants.FunctionType.Activity.ToString(),
                LoggingConstants.ProcessStatus.Finished.ToString(),
                "Execution finished."
                );

            return result.ToList(); ;
        }

        [FunctionName(ActivityFunctionsConstants.IdentifyStrayPetBreedWithStreamAsync)]
        public async Task<List<PredictionResult>> IdentifyStrayPetBreedWithStreamAsync(
        [ActivityTrigger] Stream s,
        ILogger logger)
        {
            logger.LogInformation(
                new EventId((int)LoggingConstants.EventId.IdentifyStrayPetBreedWithStreamAsyncStarted),
                LoggingConstants.Template,
                LoggingConstants.EventId.IdentifyStrayPetBreedWithStreamAsyncStarted.ToString(),
                _correlationId,
                LoggingConstants.ProcessingFunction.IdentifyStrayPetBreedWithStreamAsync.ToString(),
                LoggingConstants.FunctionType.Activity.ToString(),
                LoggingConstants.ProcessStatus.Started.ToString(),
                "Execution Started."
                );
            

            var result = await _predictionHelper.PredictBreedAsync(s).ConfigureAwait(false);

            logger.LogInformation(
                new EventId((int)LoggingConstants.EventId.IdentifyStrayPetBreedWithStreamAsyncFinished),
                LoggingConstants.Template,
                LoggingConstants.EventId.IdentifyStrayPetBreedWithStreamAsyncFinished.ToString(),
                _correlationId,
                LoggingConstants.ProcessingFunction.IdentifyStrayPetBreedWithStreamAsync.ToString(),
                LoggingConstants.FunctionType.Activity.ToString(),
                LoggingConstants.ProcessStatus.Finished.ToString(),
                "Execution Finished."
                );

            return result.ToList(); ;
        }


        [FunctionName(ActivityFunctionsConstants.LocateAdoptionCentresByBreedAsync)]
        public async Task<List<AdoptionCentre>> LocateAdoptionCentresByBreedAsync(
            [ActivityTrigger] string breed,
            ILogger logger
        )
        {
            
            logger.LogInformation(
                new EventId((int)LoggingConstants.EventId.LocateAdoptionCentresByBreedAsyncStarted),
                LoggingConstants.Template,
                LoggingConstants.EventId.LocateAdoptionCentresByBreedAsyncStarted.ToString(),
                _correlationId,
                LoggingConstants.ProcessingFunction.LocateAdoptionCentresByBreedAsync.ToString(),
                LoggingConstants.FunctionType.Activity.ToString(),
                LoggingConstants.ProcessStatus.Started.ToString(),
                "Execution Started."
                );

            var result = await _adoptionCentreDbHelper.GetAdoptionCentresByBreedAsync(breed)
                .ConfigureAwait(false);

            logger.LogInformation(
                new EventId((int)LoggingConstants.EventId.LocateAdoptionCentresByBreedAsyncFinsihed),
                LoggingConstants.Template,
                LoggingConstants.EventId.LocateAdoptionCentresByBreedAsyncFinsihed.ToString(),
                _correlationId,
                LoggingConstants.ProcessingFunction.LocateAdoptionCentresByBreedAsync.ToString(),
                LoggingConstants.FunctionType.Activity.ToString(),
                LoggingConstants.ProcessStatus.Finished.ToString(),
                "Execution Finished."
                );

            return result.ToList();

        }

        [FunctionName(ActivityFunctionsConstants.GetBreedInformationAsync)]
        public async Task<BreedInfo> GetBreedInformationASync(
            [ActivityTrigger] string breed,
            ILogger logger)
        {
            
            logger.LogInformation(
                new EventId((int)LoggingConstants.EventId.GetBreedInformationAsyncStarted),
                LoggingConstants.Template,
                LoggingConstants.EventId.GetBreedInformationAsyncStarted.ToString(),
                _correlationId,
                LoggingConstants.ProcessingFunction.GetBreedInformationAsync.ToString(),
                LoggingConstants.FunctionType.Activity.ToString(),
                LoggingConstants.ProcessStatus.Started.ToString(),
                "Execution Started."
                );


            var result = await _breedInfoDbHelper.GetBreedInformationAsync(breed)
                .ConfigureAwait(false);

            logger.LogInformation(
                new EventId((int)LoggingConstants.EventId.GetBreedInformationAsyncFinished),
                LoggingConstants.Template,
                LoggingConstants.EventId.GetBreedInformationAsyncFinished.ToString(),
                _correlationId,
                LoggingConstants.ProcessingFunction.GetBreedInformationAsync.ToString(),
                LoggingConstants.FunctionType.Activity.ToString(),
                LoggingConstants.ProcessStatus.Finished.ToString(),
                "Execution Finished."
                );

            return result;
        }

        [FunctionName(ActivityFunctionsConstants.PushMessagesToSignalRHub)]
        public async Task<bool> PushMessagesToSignalRHub(
            [ActivityTrigger] SignalRRequest request,
            [SignalR(HubName = SignalRConstants.HubName)] IAsyncCollector<SignalRMessage> signalRMessages,
            ILogger logger
        )
        {

            logger.LogInformation(
                new EventId((int)LoggingConstants.EventId.PushMessagesToSignalRHubStarted),
                LoggingConstants.Template,
                LoggingConstants.EventId.PushMessagesToSignalRHubStarted.ToString(),
                _correlationId,
                LoggingConstants.ProcessingFunction.PushMessagesToSignalRHub.ToString(),
                LoggingConstants.FunctionType.Activity.ToString(),
                LoggingConstants.ProcessStatus.Started.ToString(),
                "Execution Started."
                );

            await signalRMessages.AddAsync(
                new SignalRMessage
                {

                    UserId = request.UserId,
                    Target = "sendPetAdoptionCentres",
                    Arguments = new[] { request.Message }
                })
                .ConfigureAwait(false);

            logger.LogInformation(
               new EventId((int)LoggingConstants.EventId.PushMessagesToSignalRHubFinished),
               LoggingConstants.Template,
               LoggingConstants.EventId.PushMessagesToSignalRHubFinished.ToString(),
               _correlationId,
               LoggingConstants.ProcessingFunction.PushMessagesToSignalRHub.ToString(),
               LoggingConstants.FunctionType.Activity.ToString(),
               LoggingConstants.ProcessStatus.Finished.ToString(),
               "Execution Finished."
               );

            return true;

        }

        [FunctionName(ActivityFunctionsConstants.GetSignalUserIdFromBlobMetadataAsync)]
        public async Task<string> GetSignalUserIdFromBlobMetadataAsync(
            [ActivityTrigger] string blobUrl,
            ILogger logger)
        {

            logger.LogInformation(
               new EventId((int)LoggingConstants.EventId.GetSignalUserIdFromBlobMetadataAsyncStarted),
               LoggingConstants.Template,
               LoggingConstants.EventId.GetSignalUserIdFromBlobMetadataAsyncStarted.ToString(),
               _correlationId,
               LoggingConstants.ProcessingFunction.GetSignalUserIdFromBlobMetadataAsync.ToString(),
               LoggingConstants.FunctionType.Activity.ToString(),
               LoggingConstants.ProcessStatus.Started.ToString(),
               "Execution Started."
               );


            var blobMetadata = await _blobHelper.GetBlobMetaDataAsync(blobUrl)
                .ConfigureAwait(false);

            var signalRUserId =
                blobMetadata.Where(x => x.Key == SignalRConstants.CustomHeaderName)
                .FirstOrDefault()
                .Value;

            logger.LogInformation(
               new EventId((int)LoggingConstants.EventId.GetSignalUserIdFromBlobMetadataAsyncFinshed),
               LoggingConstants.Template,
               LoggingConstants.EventId.GetSignalUserIdFromBlobMetadataAsyncFinshed.ToString(),
               _correlationId,
               LoggingConstants.ProcessingFunction.GetSignalUserIdFromBlobMetadataAsync.ToString(),
               LoggingConstants.FunctionType.Activity.ToString(),
               LoggingConstants.ProcessStatus.Finished.ToString(),
               "Execution Finished."
               );

            return signalRUserId;

        }

        #endregion




    }
}
