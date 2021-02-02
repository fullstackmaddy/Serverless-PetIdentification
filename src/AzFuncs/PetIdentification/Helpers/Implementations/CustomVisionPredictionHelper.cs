using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.Models;
using Microsoft.Extensions.Configuration;
using PetIdentification.Interfaces;
using PetIdentification.Models;
using PetIdentification.Profiles;

namespace PetIdentification.Helpers
{
    public class CustomVisionPredictionHelper : IPredictionHelper
    {
        #region Properties
        private readonly IConfiguration _config;
        private readonly ICustomVisionPredictionClient _predictionClient;
        private readonly IMapper _mapper;
        #endregion

        #region Constructors
        public CustomVisionPredictionHelper(IConfiguration config, 
        ICustomVisionPredictionClient predictionClient,
        IMapper mapper)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _predictionClient = predictionClient ?? 
                throw new ArgumentNullException(nameof(predictionClient));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            
        }
        #endregion

        #region PublicMethods
        public async Task<IEnumerable<PredictionResult>> PredictBreedAsync(string imageUrl)
        {
            
            Guid projectId = new Guid(
                _config["CustomVisionProjectId"].ToString()
            );
            
            string iterationName = _config["CustomVisionPublishedIterationName"].ToString();
            
            var result = await _predictionClient.ClassifyImageUrlAsync(
                projectId,
                iterationName,
                new ImageUrl(
                    imageUrl
                )
            );
            
            
            return _mapper.Map<List<PredictionModel>, List<PredictionResult>>
            (
                result.Predictions.ToList()
            );

            
        }

        public async Task<IEnumerable<PredictionResult>> PredictBreedAsync(Stream s)
        {
            Guid projectId = new Guid(
                _config["CustomVisionProjectId"].ToString()
            );

            string iterationName = _config["CustomVisionPublishedIterationName"].ToString();

            var result = await _predictionClient.ClassifyImageAsync(
                projectId,
                iterationName,
                s)
                .ConfigureAwait(false);

            return _mapper.Map<List<PredictionModel>, List<PredictionResult>>
           (
               result.Predictions.ToList()
           );
        }

        #endregion
    }
}