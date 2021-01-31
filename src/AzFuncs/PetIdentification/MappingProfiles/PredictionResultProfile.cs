using AutoMapper;
using PetIdentification.Models;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.Models;

namespace PetIdentification.Profiles

{
    public class PredictionResultProfile : Profile
    {
        public PredictionResultProfile()
        {
            CreateMap<PredictionModel, PredictionResult>()
            .ForMember(destination => destination.TagName, 
                actual => actual.MapFrom(src => src.TagName))
            .ForMember(destination => destination.Probability, 
                actual => actual.MapFrom(src => src.Probability));
            
        }
    }
}