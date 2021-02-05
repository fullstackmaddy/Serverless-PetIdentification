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

namespace PetIdentification.Helpers
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
                     breed))
                 .ConfigureAwait(false);

            return new BreedInfo()
            {
                Breed = breed,

                LifeExpectancy = (dynamic)document
                .Resource.GetPropertyValue<string>("lifeExpectancy"),

                Qualities = (dynamic) document
                .Resource.GetPropertyValue<string>("qualities"),

                Type = (dynamic)document
                .Resource.GetPropertyValue<string>("type"),

                Weight = (dynamic)document
                .Resource.GetPropertyValue<string>("weight"),

                Height = (dynamic)document
                .Resource.GetPropertyValue<string>("height"),
            };

            
        }
    }
}
