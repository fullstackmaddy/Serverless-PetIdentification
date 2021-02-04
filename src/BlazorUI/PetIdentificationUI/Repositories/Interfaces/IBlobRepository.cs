using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PetIdentificationUI.Repositories.Interfaces
{
    public interface IBlobRepository
    {
        public Task UploadBlobAsync(Stream stream, 
            string contentType,
            string blobName,
            IDictionary<string, string> metaDataKeyValuePairs);

    }
}
