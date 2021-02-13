using Chambers.Api.Data.Model;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Chambers.Api.Data.Repositories
{
    public class DocumentRepository : IDocumentRepository
    {
        public readonly Container _cosmosContainer;

        public DocumentRepository(Container cosmosContainer)
        {
            _cosmosContainer = cosmosContainer;
        }

        public async Task<Document> CreateAsync(Document document)
        {
            if (document.Content.Length == 0)
                throw new ArgumentException("File content can not be empty.");

            document.Id = Guid.NewGuid();

            await _cosmosContainer.CreateItemAsync(document);

            return document;
        }

        public async Task<IEnumerable<Document>> ListAllAsync()
        {
            var query = "SELECT * FROM c";

            QueryDefinition queryDefinition = new QueryDefinition(query);
            FeedIterator<Document> queryResultSetIterator = _cosmosContainer.GetItemQueryIterator<Document>(queryDefinition);

            List<Document> results = new List<Document>();

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<Document> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (Document doc in currentResultSet)
                {
                    results.Add(doc);
                }
            }

            return results;
        }

        public Task DeleteAsync(Guid guid)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(Guid guid, int order)
        {
            _cosmosContainer.ReadItemAsync(guid.ToString(), new PartitionKey(guid.ToString()));
        }
    }
}
