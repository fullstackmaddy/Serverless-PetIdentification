using PetIdentification.Models;
using System.Threading.Tasks;

namespace PetIdentification.Interfaces
{
    public interface IBreedInfoDbHelper
    {

        public Task<BreedInfo> GetBreedInformationAsync(string breed);
    }
}
