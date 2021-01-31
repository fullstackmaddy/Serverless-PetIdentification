using System.Collections.Generic;
using System.Threading.Tasks;
using PetIdentification.Models;

namespace PetIdentification.Interfaces
{
    public interface IAdoptionCentreDbHelper
    {
        public Task<IEnumerable<AdoptionCentre>> GetAdoptionCentresByBreedAsync(string breed);
    }
}