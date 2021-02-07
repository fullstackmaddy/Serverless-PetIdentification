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

        [Parameter] public EventCallback<string> OnFileStatusChange { get; set; }

        private const string DefaultMessage = @"Drop a image of the stray pet here, or click to choose a file";

        private const int MaxFileSize = 5 * 1024 * 1024;
        private string fileName;
        private string fileContentType;
        string blobName;

        public async Task UploadFileAsync(IFileListEntry[] files)
        {
            var file = files.FirstOrDefault();

            if (file != null && file.Size < MaxFileSize)
            {
                fileName = file.Name;
                fileContentType = file.Type;


                blobName =
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
            }

            await OnFileStatusChange
                .InvokeAsync(blobName)
                .ConfigureAwait(false);

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
                { "signalruserid", Guid.NewGuid().ToString("N")}
            };
        }


    }
}
