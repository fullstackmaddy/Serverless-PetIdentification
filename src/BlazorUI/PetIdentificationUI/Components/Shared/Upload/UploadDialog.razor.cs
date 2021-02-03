using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PetIdentificationUI.Repositories.Interfaces;
using BlazorInputFile;

namespace PetIdentificationUI.Components.Shared.Upload
{
    
    public partial class UploadDialog : ComponentBase
    {
        [Inject] public IBlobRepository BlobRepository { get; set; }

        private const string DefaultMessage = @"Drop a image of the stray pet here, or click to choose a file";

        private const int MaxFileSize = 5 * 1024 * 1024;
        private bool doesFileSizeExceedLimit = false;
        private bool isUnacceptableFileType = false;
        private string status = DefaultMessage;
        private string fileName;
        private string fileContentType;

        private async Task UploadFileAsync(IFileListEntry[] files)
        {
            var file = files.FirstOrDefault();

            if (file == null)
            {
                status = "Please upload an image";
                return;
            }
            else if (file.Size > MaxFileSize)
            {
                status = "We only support upload of images lesser than 5 MB";
            }

        }

        private bool IsThereErrorInFileUplod()
        {
            if (fileName != null && (doesFileSizeExceedLimit || isUnacceptableFileType))
                return true;
            
            return false;
        }


    }
}
