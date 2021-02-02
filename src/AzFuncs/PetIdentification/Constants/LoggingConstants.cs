namespace PetIdentification.Constants
{
    public class LoggingConstants
    {
        public const string Template
            = "{EventDescription}{CorrelationId}{ProcessingFunction}{FunctionType}{ProcessStatus}{LogMessage}";

        public enum ProcessingFunction
        {
            IdentifyStrayPetBreedWithUrlAsync,
            IdentifyStrayPetBreedWithStreamAsync,
            LocateAdoptionCentresByBreedAsync,
            GetBreedInformationASync,
            PushMessagesToSignalRHub,
            GetSignalUserIdFromBlobMetadataAsync,
            EventGridDurableOrchestration,
            EventGridDurableClient,
            HttpFormDataOrchestration,
            HttpFormDataDurableClient,
            HttpUrlOrchestration,
            HttpUrlDurableClient
        }

        public enum EventId
        {
            Exceptioned = 001,

            IdentifyStrayPetBreedWithUrlAsyncStarted = 105,
            IdentifyStrayPetBreedWithUrlAsyncFinished = 106,
            LocateAdoptionCentresByBreedAsyncStarted = 107,
            LocateAdoptionCentresByBreedAsyncFinsihed = 108,
            GetBreedInformationASyncStarted = 109,
            GetBreedInformationASyncFinished = 110,
            GetSignalUserIdFromBlobMetadataAsyncStarted = 111,
            GetSignalUserIdFromBlobMetadataAsyncFinshed = 112,
            PushMessagesToSignalRHubStarted = 113,
            PushMessagesToSignalRHubFinished = 114,

            HttpUrlDurableClientStarted =1001,
            HttpUrlDurableClientFinished = 1002,
            HttpUrlOrchestrationStarted=1003,
            HttpUrlOrchestrationFinished = 1004,
            

            HttpFormDataDurableClientStarted = 2001,
            HttpFormDataDurableClientFinished = 2002,
            HttpFormDataOrchestrationStarted = 2003,
            HttpFormDataOrchestrationFinished = 2004,


            EventGridDurableClientStarted = 3001,
            EventGridDurableClientFinished = 3002,
            EventGridDurableOrchestrationStarted = 3003,
            EventGridDurableOrchestrationFinsihed = 3004

        }

        public enum ProcessStatus
        {
            Started,
            Finished,
            Failed
        }

        public enum FunctionType
        {
            Activity,
            Orchestration,
            Client
        }
    }
}
