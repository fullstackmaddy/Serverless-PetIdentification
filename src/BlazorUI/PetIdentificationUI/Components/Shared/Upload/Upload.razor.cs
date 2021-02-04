using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PetIdentificationUI.Components.Shared.Upload
{
    public partial class Upload
    {
        bool isFileProcessed = false;

        public void GetFileUploadStatus(bool value)
        {
            isFileProcessed = value;
        }
    }
}
