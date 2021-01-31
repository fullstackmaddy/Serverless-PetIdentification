using System.Threading.Tasks;
using System.Collections.Generic;
using PetIdentification.Models;
using System.IO;

namespace PetIdentification.Interfaces
{
    public interface IPredictionHelper
    {
        public Task<IEnumerable<PredictionResult>> PredictBreedAsync(string imageUrl);

        public Task<IEnumerable<PredictionResult>> PredictBreedAsync(Stream imageData);

    }
}