using Chambers.Api.Data.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Chambers.Api.Data.Repositories
{
    public interface IDocumentRepository
    {
        Task<Document> CreateAsync(Document document);

        Task DeleteAsync(Guid guid);

        Task UpdateAsync(Guid guid, int order);

        Task<IEnumerable<Document>> ListAllAsync();
    }
}
