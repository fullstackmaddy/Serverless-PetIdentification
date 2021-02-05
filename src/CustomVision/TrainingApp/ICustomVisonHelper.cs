using System.Collections.Generic;
using System.Threading.Tasks;

namespace TrainingApp
{
    public interface ICustomVisionHelper
    {
        public Task CreateProject(string name, string description);

        public Task BatchUploadImages(string directoryPath);

        public Task TrainProject();

        public Task PublishIteration(string predictionResourceId);

    }
}