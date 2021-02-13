using Chambers.Api.Data.Model;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
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

            await _cosmosContainer.CreateItemAsync(document, new PartitionKey(document.Id.ToString()));

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

        public async Task<Document> GetAsync(Guid guid)
        {
            try
            {
                return await _cosmosContainer.ReadItemAsync<Document>(guid.ToString(), new PartitionKey(guid.ToString()));
            }
            catch (Exception)
            {
                // todo disambiguate cosmos 404 exception from general network exceptions
                return null;
            }
        }

        public async Task DeleteAsync(Guid guid)
        {
            await _cosmosContainer.DeleteItemAsync<Document>(guid.ToString(), new PartitionKey(guid.ToString()));
        }

        public async Task UpdateAsync(Guid guid, int order)
        {
            Document document = await GetAsync(guid);

            document.Order = order;

            await _cosmosContainer.UpsertItemAsync(document);
        }
    }
}
