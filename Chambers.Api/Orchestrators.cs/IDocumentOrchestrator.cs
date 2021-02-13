using Chambers.Api.Data.Model;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Chambers.Api.Orchestrators
{
    public interface IDocumentOrchestrator
    {
        Task<IActionResult> UploadAsync(Document document);
        Task<IActionResult> ListAsync();
        Task<IActionResult> OrderAsync(IEnumerable<DocumentOrderRequest> orderRequests);
        Task<IActionResult> GetAsync(string reference);
        Task<IActionResult> DeleteAsync(string reference);
    }
}
