using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using PetIdentification.Constants;
using PetIdentification.Interfaces;
using PetIdentification.Models;

namespace PetIdentification.Helpers
{
    public class CosmosAdoptionCentreDbHelper : IAdoptionCentreDbHelper
    {
        #region Properties
        private readonly IDocumentClient _documentClient;
        private List<AdoptionCentre> _adoptionCentres;
        #endregion

        #region Constructors
        public CosmosAdoptionCentreDbHelper(IDocumentClient documentClient)
        {
            _documentClient = documentClient ??
                throw new ArgumentNullException(nameof(documentClient));
            
        }
        #endregion

        #region PublicMethods
        public async Task<IEnumerable<AdoptionCentre>> GetAdoptionCentresByBreedAsync(string breed)
        {
            Uri collectionUri = UriFactory.CreateDocumentCollectionUri(
                 CosmosDBConstants.DBName,
                 CosmosDBConstants.AdoptionCentreCollectionName
            );

            IDocumentQuery<AdoptionCentre> query = _documentClient
                .CreateDocumentQuery<AdoptionCentre>(collectionUri)
                .Where( x => x.ShelteredBreed == breed)
                .AsDocumentQuery();

            _adoptionCentres = new List<AdoptionCentre>();
            
            while(query.HasMoreResults)
            {
                foreach(AdoptionCentre a in await query.ExecuteNextAsync())
                {
                    _adoptionCentres.Add(a);
                }
            }

            return _adoptionCentres;
            
        }
        #endregion
    }
}