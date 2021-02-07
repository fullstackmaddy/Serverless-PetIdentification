using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using PetIdentificationUI.Repositories.Interfaces;

namespace PetIdentificationUI.Repositories.Implementations
{
    public class BlobRepository : IBlobRepository
    {
        private readonly BlobServiceClient _blobServiceClient;

        public BlobRepository(BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient ??
                throw new ArgumentNullException(nameof(blobServiceClient));

        }

        public async Task<string> UploadBlobAsync(
            string containerName,
            Stream stream,
            string contentType,
            string blobName,
            IDictionary<string, string> metaDataKeyValuePairs)
        {
            //var get BlobClient

            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            var blobClient = blobContainerClient
                .GetBlobClient(blobName);

            var blobHttpHeaders = new BlobHttpHeaders();
            blobHttpHeaders.ContentType = contentType;


            //Upload the files to the blob
            await blobClient
                .UploadAsync(stream, blobHttpHeaders)
                .ConfigureAwait(false);

            await blobClient
                .SetMetadataAsync(metaDataKeyValuePairs)
                .ConfigureAwait(false);

            return blobClient.Uri.ToString();
            
        }
    }
}
