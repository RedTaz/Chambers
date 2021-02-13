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
using Chambers.Api.Orchestrators;

namespace Chambers.Api
{
    public class DocumentService
    {
        private readonly IDocumentOrchestrator _documentOrchestrator;

        public DocumentService(IDocumentOrchestrator documentOrchestrator)
        {
            _documentOrchestrator = documentOrchestrator;
        }

        [FunctionName("Upload")]
        public async Task<IActionResult> Upload(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req
        )
        {
            var form = await req.ReadFormAsync();
            Document document = BindToDocument(form);

            return await _documentOrchestrator.UploadAsync(document);
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
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req
        )
        {
            return await _documentOrchestrator.ListAsync();
        }

        [FunctionName("Order")]
        public async Task<IActionResult> Order(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestMessage req
        )
        {
            string content = await req.Content.ReadAsStringAsync();
            IEnumerable<DocumentOrderRequest> orderRequests 
                = JsonConvert.DeserializeObject<IEnumerable<DocumentOrderRequest>>(content);

            return await _documentOrchestrator.OrderAsync(orderRequests);
        }

        [FunctionName("Get")]
        public async Task<IActionResult> Get(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Get/{reference}")] HttpRequest req,
            string reference
        )
        {
            return await _documentOrchestrator.GetAsync(reference);
        }

        [FunctionName("Delete")]
        public async Task<IActionResult> Delete(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Delete/{reference}")] HttpRequest req,
            string reference
        )
        {
            return await _documentOrchestrator.DeleteAsync(reference);
        }
    }
}
