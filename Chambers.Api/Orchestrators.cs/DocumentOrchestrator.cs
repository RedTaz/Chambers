using Chambers.Api.Data.Model;
using Chambers.Api.Data.Repositories;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chambers.Api.Orchestrators
{
    public class DocumentOrchestrator : IDocumentOrchestrator
    {
        private readonly IDocumentRepository _documentRepository;

        public DocumentOrchestrator(IDocumentRepository documentRepository)
        {
            _documentRepository = documentRepository;
        }

        public async Task<IActionResult> UploadAsync(Document document)
        {
            if (document == null)
                return new BadRequestResult();

            Document reponse = await _documentRepository.CreateAsync(document);

            if (document.Content.Length > 5000000)
                return new StatusCodeResult(413);

            // naive check for valid PDF, not foolproof, could be improved with supporting pdf library
            var ascii = Encoding.ASCII.GetString(document.Content);
            if (!ascii.StartsWith("%PDF-"))
                return new UnsupportedMediaTypeResult();

            return new OkObjectResult(reponse);
        }

        public async Task<IActionResult> ListAsync()
        {
            IEnumerable<DocumentListItem> results = (await _documentRepository.ListAllAsync())
                .Select(doc => new DocumentListItem()
                {
                    Location = doc.Id.ToString(), // location = id since document is stored as data
                    Name = doc.Name,
                    Order = doc.Order,
                    Size = doc.Content.Length
                });

            return new OkObjectResult(results.OrderBy(d => d.Order));
        }

        public async Task<IActionResult> OrderAsync(IEnumerable<DocumentOrderRequest> orderRequests)
        {
            foreach (DocumentOrderRequest orderRequest in orderRequests)
            {
                await _documentRepository.UpdateAsync(Guid.Parse(orderRequest.DocumentId), orderRequest.Order);
            }

            // todo, return an IEnumerable<OrderResponse> with update results for each re-order request
            return new OkResult();
        }

        public async Task<IActionResult> GetAsync(string reference)
        {
            if (reference == null)
                return new BadRequestResult();

            Guid.TryParse(reference, out Guid guid);

            if (Guid.Empty.Equals(guid))
                return new BadRequestResult();

            Document document = await _documentRepository.GetAsync(guid);

            if (document == null)
                return new NotFoundResult();

            return new OkObjectResult(document);
        }

        public async Task<IActionResult> DeleteAsync(string reference)
        {
            IActionResult getResult = await GetAsync(reference);

            if (!(getResult is OkObjectResult))
                return getResult;

            await _documentRepository.DeleteAsync(Guid.Parse(reference));

            return new OkResult();
        }
    }
}
