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
        private readonly BlobContainerClient _blobContainerClient;

        public BlobRepository(BlobContainerClient blobContainerClient)
        {
            _blobContainerClient = blobContainerClient ??
                throw new ArgumentNullException(nameof(blobContainerClient));

        }

        public async Task UploadBlobAsync(Stream stream,
            string contentType,
            string blobName,
            IDictionary<string, string> metaDataKeyValuePairs)
        {
            //var get BlobClient

            var blobClient = _blobContainerClient
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

            
        }
    }
}
