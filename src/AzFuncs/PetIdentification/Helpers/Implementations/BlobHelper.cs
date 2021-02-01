using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using PetIdentification.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PetIdentification.Helpers
{
    public class BlobHelper : IBlobHelper
    {
        #region Properties&Fields
        private readonly BlobContainerClient _blobContainerClient;

        #endregion

        #region Constructors
        public BlobHelper(BlobContainerClient blobContainerClient)
        {
            _blobContainerClient = blobContainerClient ??
                throw new ArgumentNullException(nameof(blobContainerClient));

        }
        #endregion

        #region Methods
        public async Task<IDictionary<string, string>> GetBlobMetaDataAsync(string blobUrl)
        {
            BlobUriBuilder blobUriBuilder = new BlobUriBuilder(
                new Uri(blobUrl));

            var blobClient = _blobContainerClient.GetBlobClient(blobUriBuilder.BlobName);

            BlobProperties blobProperties = await blobClient.GetPropertiesAsync();

            return blobProperties.Metadata;

        }
        #endregion
    }
}
