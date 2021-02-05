using System;   
using System.Collections.Generic;
using System.Text;

namespace TrainingApp
{
    public static class CustomVisionConstants
    {
        public const string ProjectName = "Az-Dev-Stories-Pet-Identification";

        public const string ProjectDescription = "This is project is built for identifying pets.";

        /// You can obtain these values from the Keys and Endpoint page for your Custom Vision Prediction resource in the Azure Portal.
        public const string TrainingKey = "";

        // You can obtain these values from the Keys and Endpoint page for your Custom Vision Prediction resource in the Azure Portal.
        public const string TrainingEndpoint = "";

        // You can obtain this value from the Properties page for your Custom Vision Prediction resource in the Azure Portal. See the "Resource ID" field. This typically has a value such as:
        // /subscriptions/<your subscription ID>/resourceGroups/<your resource group>/providers/Microsoft.CognitiveServices/accounts/<your Custom Vision prediction resource name>
        public const string PredictionResourceId ="";

    }
}
