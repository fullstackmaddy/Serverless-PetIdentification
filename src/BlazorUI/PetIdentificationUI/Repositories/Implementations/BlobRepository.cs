using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using PetIdentificationUI.Repositories.Interfaces;

namespace PetIdentificationUI.Repositories.Implementations
{
    public class BlobRepository : IBlobRepository
    {
        public Task GetBlobNameAsync(string fileContentType)
        {
            throw new NotImplementedException();
        }

        public Task SetBlobMetaDataAsync(IDictionary<string, string> headerValuePairs)
        {
            throw new NotImplementedException();
        }

        public Task UploadBlobAsync(Stream s)
        {
            throw new NotImplementedException();
        }
    }
}
