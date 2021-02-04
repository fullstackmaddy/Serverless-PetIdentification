using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using PetIdentificationUI.Repositories.Interfaces;

namespace PetIdentificationUI.Repositories.Implementations
{
    public class BlobRepository : IBlobRepository
    {
        public BlobRepository()
        {

        }

        public Task SetBlobMetaDataAsync(IDictionary<string, string> headerValuePairs, string blobName)
        {
            throw new NotImplementedException();
        }

        public Task UploadBlobAsync(Stream s, string blobName)
        {
            throw new NotImplementedException();
        }
    }
}
