using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using PetIdentification.Constants;

namespace PetIdentification.Functions
{
    public class SignalRManagementFunctionsController
    {
        #region Constructors
        public SignalRManagementFunctionsController()
        {
            
        }
        #endregion


        #region Functions
        [FunctionName("negotiate")]
        public async Task<SignalRConnectionInfo> GetSignalRInfo(
            [HttpTrigger(
                AuthorizationLevel.Anonymous,
                "post"
            )]HttpRequest request,
            IBinder binder)
        {
            //read the headers for http request
            var userId = request.Headers["x-blazor-app-session-id"];

            SignalRConnectionInfoAttribute connInfoAttribute = new
                SignalRConnectionInfoAttribute()
                {
                    HubName = SignalRConstants.HubName,
                    UserId = userId
                };

            SignalRConnectionInfo connectionInfo = await binder
            .BindAsync<SignalRConnectionInfo>(connInfoAttribute);

            return connectionInfo;
        }
        #endregion

    }

    
  
}
