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

        [Parameter] public EventCallback<bool> OnFileStatusChange { get; set; }

        private const string DefaultMessage = @"Drop a image of the stray pet here, or click to choose a file";

        private const int MaxFileSize = 5 * 1024 * 1024;
        private bool doesFileSizeExceedLimit = false;
        private bool isUnacceptableFileType = false;
        private string fileName;
        private string fileContentType;
        private bool isFileProcessed;

        public async Task UploadFileAsync(IFileListEntry[] files)
        {
            var file = files.FirstOrDefault();

            

            if (file == null || file.Size > MaxFileSize)
            {

                isFileProcessed = false;
                
            }
           
            else
            {
                fileName = file.Name;
                fileContentType = file.Type;
                

                string blobName =
                    await Task.Factory.StartNew(
                            () => CreateBlobName()
                        )
                    .ConfigureAwait(false);
                var headers =
                    await Task.Factory.StartNew(
                            () => CreateMetadataHeaderPairs()
                        )
                    .ConfigureAwait(false);

                await BlobRepository
                    .UploadBlobAsync(
                        stream: file.Data,
                        contentType: fileContentType,
                        blobName: blobName,
                        metaDataKeyValuePairs: headers
                    )
                    .ConfigureAwait(false);
                
                
                isFileProcessed = true;
               
            }

            await OnFileStatusChange
                .InvokeAsync(isFileProcessed)
                .ConfigureAwait(false);

        }

        public bool IsThereErrorInFileUplod()
        {
            if (fileName != null && (doesFileSizeExceedLimit || isUnacceptableFileType))
            {
                isFileProcessed = false;
                return true;
            }
            
            return false;
        }

        public string CreateBlobName()
        {
            string extension;

            if (fileContentType == "image/jpeg")
            {
                extension = "jpg";
            }
            else
            {
                extension = "png";
            }
            return string.Format("{0}.{1}", Guid.NewGuid().ToString(), extension);

        }



        private Dictionary<string, string> CreateMetadataHeaderPairs()
        {
            return new Dictionary<string, string>()
            {
                { "SignalRUserId", Guid.NewGuid().ToString("N")}
            };
        }


    }
}
