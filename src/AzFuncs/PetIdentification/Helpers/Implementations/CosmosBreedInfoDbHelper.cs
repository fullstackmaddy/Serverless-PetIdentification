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

            SqlQuerySpec sqlQuerySpec = new SqlQuerySpec()
            {
                QueryText = "Select * FROM BreedInformation b WHERE b.breed = @breed OFFSET 0 LIMIT 1",
                Parameters = new SqlParameterCollection()
                {
                    new SqlParameter("@breed", breed)
                }
            };
            new FeedOptions { EnableCrossPartitionQuery = true };

            var query = await Task.Factory.StartNew(
                    () => _documentClient
                .CreateDocumentQuery<BreedInfo>(
                   collectionUri,
                   sqlQuerySpec,
                   new FeedOptions { EnableCrossPartitionQuery = true }
                    )
                ).ConfigureAwait(false);

            return query.ToList()[0];

            
        }
    }
}
