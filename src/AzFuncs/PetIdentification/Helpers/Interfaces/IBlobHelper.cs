using System.Collections.Generic;
using System.Threading.Tasks;

namespace PetIdentification.Interfaces
{
    public interface IBlobHelper
    {
        public Task<IDictionary<string,string>> GetBlobMetaDataAsync(string blobUrl);
    }
}
