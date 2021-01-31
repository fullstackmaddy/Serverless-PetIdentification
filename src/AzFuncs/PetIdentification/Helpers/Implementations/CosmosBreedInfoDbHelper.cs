using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using PetIdentification.Constants;
using PetIdentification.Interfaces;
using PetIdentification.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PetIdentification.Helpers.Implementations
{
    public class CosmosBreedInfoDbHelper : IBreedInfoDbHelper
    {
        private readonly IDocumentClient _documentClient;

        public CosmosBreedInfoDbHelper(IDocumentClient documentClient)
        {
            _documentClient = documentClient ??
                throw new ArgumentNullException(nameof(documentClient));

        }
        public async Task<BreedInfo> GetBreedInformationAsync(string breed)
        {
            Uri collectionUri = UriFactory.CreateDocumentCollectionUri(
                 CosmosDBConstants.DBName,
                 CosmosDBConstants.BreedInfoCollectionName
            );

            IDocumentQuery<BreedInfo> query = _documentClient
                .CreateDocumentQuery<BreedInfo>(
                collectionUri)
                .Where(x => x.Breed == breed)
                .AsDocumentQuery();

            var document = await _documentClient
                 .ReadDocumentAsync(
                 UriFactory.CreateDocumentUri(
                     CosmosDBConstants.DBName,
                     CosmosDBConstants.BreedInfoCollectionName,
                     breed));

            return new BreedInfo()
            {
                Breed = breed,
                LifeExpectancy = (dynamic)document
                .Resource.GetPropertyValue<string>("lifeExpectancy"),
                Temprament = (dynamic)document
                .Resource.GetPropertyValue<string>("temprament"),
                Qualities = (dynamic) document
                .Resource.GetPropertyValue<string>("qualities")
            };

            
        }
    }
}
