using AutoMapper;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PetIdentification.Constants;
using PetIdentification.Interfaces;
using PetIdentification.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
            logger.LogInformation($"Started the execution of IdentifyStrayPetBreedWithUrlAsync function");

            var result = await _predictionHelper.PredictBreedAsync(imageUrl);

            logger.LogInformation($"Finshed calling the IdentifyStrayPetBreedWithUrlAsync function from prediction helper");

            return result.ToList(); ;
        }

        [FunctionName(ActivityFunctionsConstants.IdentifyStrayPetBreedWithStreamAsync)]
        public async Task<List<PredictionResult>> IdentifyStrayPetBreedWithStreamAsync(
        [ActivityTrigger] Stream s,
        ILogger logger)
        {
            logger.LogInformation($"Started the execution of IdentifyStrayPetBreedWithStreamAsync function");

            var result = await _predictionHelper.PredictBreedAsync(s);

            logger.LogInformation($"Finshed calling the IdentifyStrayPetBreedWithStreamAsync function from prediction helper");

            return result.ToList(); ;
        }


        [FunctionName(ActivityFunctionsConstants.LocateAdoptionCentresByBreedAsync)]
        public async Task<List<AdoptionCentre>> LocateAdoptionCentresByBreedAsync(
            [ActivityTrigger] string breed,
            ILogger logger
        )
        {
            logger.LogInformation("Started the execution of the LocateAdoptionCentresByBreedAsync activity function");
            var result = await _adoptionCentreDbHelper.GetAdoptionCentresByBreedAsync(breed);

            logger.LogInformation("Finished the execution of LocateAdoptionCentresByBreedAsync activity function");

            return result.ToList();

        }

        [FunctionName(ActivityFunctionsConstants.GetBreedInformationASync)]
        public async Task<BreedInfo> GetBreedInformationASync(
            [ActivityTrigger] string breed,
            ILogger logger)
        {
            logger.LogInformation("Started the execution of the GetBreedInformationASync activity function");

            var result = await _breedInfoDbHelper.GetBreedInformationAsync(breed);

            return result;
        }

        [FunctionName(ActivityFunctionsConstants.PushMessagesToSignalRHub)]
        public async Task<bool> PushMessagesToSignalRHub(
            [ActivityTrigger] SignalRRequest request,
            [SignalR(HubName = SignalRConstants.HubName)] IAsyncCollector<SignalRMessage> signalRMessages,
            ILogger logger
        )
        {

            logger.LogInformation("Sending out signal R push notification");

            await signalRMessages.AddAsync(
                new SignalRMessage
                {
                    // the message will only be sent to this user ID
                    UserId = request.UserId,
                    Target = "sendPetAdoptionCentres",
                    Arguments = new[] { request.Message }
                });

            return true;

        }

        [FunctionName(ActivityFunctionsConstants.GetSignalUserIdFromBlobMetadataAsync)]
        public async Task<string> GetSignalUserIdFromBlobMetadataAsync(
            [ActivityTrigger] string blobUrl,
            ILogger logger)
        {
            logger.LogInformation("Retreiving the blob metadata using blob url");

            var blobMetadata = await _blobHelper.GetBlobMetaDataAsync(blobUrl);

            var signalRUserId =
                blobMetadata.Where(x => x.Key == SignalRConstants.CustomHeaderName)
                .FirstOrDefault()
                .Value;

            return signalRUserId;

        }

        #endregion




    }
}
