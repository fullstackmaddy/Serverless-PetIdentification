using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.Models;
using PredictionModels = Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.Models;

namespace TrainingApp
{
    public class CustomVisionHelper : ICustomVisionHelper
    {
        private readonly ICustomVisionTrainingClient _trainingClient;
        private readonly ICustomVisionPredictionClient _predictionClient;
       
        private Guid _projectId;

        private Guid _iterationId;

        private string _publishedIterationName;

       

        public CustomVisionHelper( ICustomVisionTrainingClient trainingClient, 
        ICustomVisionPredictionClient predictionClient
        
        )
        {
            _trainingClient = trainingClient;
            _predictionClient = predictionClient;
            
        }

        public async Task CreateProject(string name, string description)
        {
            
            var project = await _trainingClient.CreateProjectAsync(
                name: name, description: description,
                domainId: null,
                classificationType: Classifier.Multiclass
            );
            
            _projectId = project.Id;
            
        }

        

        public async Task PublishIteration(string predictionResourceId)
        {
            _publishedIterationName = string.Format("Iteration_{0}", _iterationId.ToString());
            await _trainingClient.PublishIterationAsync(
                _projectId,
                _iterationId,
                _publishedIterationName,
                predictionResourceId
            );
            
        }

        public async Task TrainProject()
        {
           Iteration iteration =  await _trainingClient.TrainProjectAsync(_projectId);

           while(iteration.Status == "Training")
           {
               Thread.Sleep(
                   new TimeSpan(0, 0, 15)
               );

               iteration  = await _trainingClient.GetIterationAsync(_projectId, iteration.Id);

           }

           _iterationId = iteration.Id;
           
            
        }

        public async Task BatchUploadImages(string parentDirectoryPath)
        {
            var childDirectories = Directory.GetDirectories(parentDirectoryPath).ToList();
            foreach(var childDirectory in childDirectories)
            {
                var directoryName = Path.GetFileName(childDirectory);
                
                //Create Tag for the directory name

                var tag = await CreateTagAsync(directoryName);

                //Upload the files in the directory in a batch
                
                var imgs = Directory.GetFiles(childDirectory).ToList();
                
                List<ImageFileCreateEntry> imgFiles = imgs.Select(
                    x => new ImageFileCreateEntry(
                        Path.GetFileName(x),
                        File.ReadAllBytes(x)
                    )
                ).ToList();
                
                await _trainingClient.CreateImagesFromFilesAsync(
                    _projectId, 
                    new ImageFileCreateBatch(imgFiles,
                    new List<Guid>(){tag.Id}
                ));
                
            }
            
        }




        #region Private Methods
        private async Task<Tag> CreateTagAsync(string name)
        {
            return await _trainingClient.CreateTagAsync(_projectId, name);
            
        }
        #endregion



    }
}