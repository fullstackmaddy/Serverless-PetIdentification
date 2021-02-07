using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PetIdentificationUI.Components
{
    public partial class UploadWithHttp : ComponentBase
    {
        string blobName;

        string userId = Guid.NewGuid().ToString();

        public void GetFileUploadStatus(string value)
        {
            blobName = value;
        }


    }
}
