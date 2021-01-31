using System.Threading.Tasks;
using System.Collections.Generic;
using PetIdentification.Models;

namespace PetIdentification.Interfaces
{
    public interface IPredictionHelper
    {
        public Task<IEnumerable<PredictionResult>> PredictBreedAsync(string imageUrl);

    }
}