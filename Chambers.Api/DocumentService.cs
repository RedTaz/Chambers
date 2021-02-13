using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Chambers.Api.Data.Model;
using Chambers.Api.Data.Repositories;
using System.Text;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;

namespace Chambers.Api
{
    public class DocumentService
    {
        private readonly IDocumentRepository _documentRepository;

        public DocumentService(IDocumentRepository documentRepository)
        {
            _documentRepository = documentRepository;
        }

        [FunctionName("Upload")]
        public async Task<IActionResult> Upload(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req
        )
        {
            var form = await req.ReadFormAsync();
            Document document = BindToDocument(form);

            return await Upload(document);
        }

        public async Task<IActionResult> Upload(Document document)
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

        /// <remarks>
        /// There doesn't seem to be a way of binding files to a model natively using Azure Functions.
        /// </remarks>
        /// <summary>
        /// Binds form data and file uploads to a <see cref="Document"/> model.
        /// </summary>
        private Document BindToDocument(IFormCollection form)
        {
            if (!form.ContainsKey("Name"))
                return null;

            IFormFile file = form.Files.GetFile("Content");

            if (file == null)
                return null;

            Document document = new Document();
            document.Name = form["Name"];

            if (form.ContainsKey("Name"))
            {
                int.TryParse(form["Order"], out int order);
                document.Order = order;
            }
            
            using (var fileStream = file.OpenReadStream())
            {
                document.Content = new byte[file.Length];
                fileStream.Read(document.Content, 0, (int)file.Length);
            }

            return document;
        }

        [FunctionName("List")]
        public async Task<IActionResult> List(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
        {
            IEnumerable<DocumentListItem> results = (await _documentRepository.ListAllAsync())
                .Select(doc => new DocumentListItem()
                {
                    Location = $"//root/{doc.Id}", // arbitrary location since document is stored as data
                    Name = doc.Name,
                    Size = doc.Content.Length
                });

            return new OkObjectResult(results);
        }

        [FunctionName("Order")]
        public async Task<IActionResult> Order(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] DocumentOrderRequest orderRequests)
        {
            foreach (DocumentOrderRequestItem orderRequest in orderRequests)
            {
                await _documentRepository.UpdateAsync(orderRequest.DocumentId, orderRequest.Order);
            }

            return new OkResult();
        }
    }
}
