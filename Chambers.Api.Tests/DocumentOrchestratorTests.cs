using NUnit.Framework;
using Chambers.Api;
using Chambers.Api.Data.Model;
using Chambers.Api.Tests.Utilities;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System;
using Chambers.Api.Data.Repositories;
using NSubstitute;
using System.Collections.Generic;
using System.Linq;
using Chambers.Api.Orchestrators;

namespace Chambers.Api.Tests
{
    public class DocumentOrchestratorTests
    {
        [Test]
        public async Task Document_Upload_200_Response()
        {
            Document document = new Document();
            document.Content = TestDocument.Get(TestDocumentType.Pdf);
            document.Name = "my_document.pdf";

            DocumentOrchestrator orchestrator = new DocumentOrchestrator(Substitute.For<IDocumentRepository>());

            IActionResult result = await orchestrator.UploadAsync(document);
            var typedResult = result as OkObjectResult;

            Assert.IsNotNull(typedResult);
            Assert.AreEqual(200, typedResult.StatusCode);
        }

        [Test]
        public async Task Document_Upload_Stores_Document()
        {
            var repo = Substitute.For<IDocumentRepository>();

            DocumentOrchestrator orchestrator = new DocumentOrchestrator(repo);

            Document document = new Document();
            document.Content = TestDocument.Get(TestDocumentType.Pdf);
            document.Name = "my_document.pdf";

            await orchestrator.UploadAsync(document);

            await repo.Received(1).CreateAsync(document);
        }

        [Test]
        public async Task Document_Upload_Rejects_NonPdf()
        {
            var repo = Substitute.For<IDocumentRepository>();

            DocumentOrchestrator orchestrator = new DocumentOrchestrator(repo);

            Document document = new Document();
            document.Content = TestDocument.Get(TestDocumentType.Word);
            document.Name = "my_document.docx";

            IActionResult result = await orchestrator.UploadAsync(document);
            var typedResult = result as UnsupportedMediaTypeResult;

            Assert.IsNotNull(typedResult);
            Assert.AreEqual(415, typedResult.StatusCode);
        }

        [Test]
        public async Task Document_Upload_Rejects_Large_Payload()
        {
            var repo = Substitute.For<IDocumentRepository>();

            DocumentOrchestrator orchestrator = new DocumentOrchestrator(repo);

            Document document = new Document();
            document.Content = TestDocument.Get(TestDocumentType.LargePdf);
            document.Name = "my_document.pdf";

            IActionResult result = await orchestrator.UploadAsync(document);
            var typedResult = result as StatusCodeResult;

            Assert.IsNotNull(typedResult);
            Assert.AreEqual(413, typedResult.StatusCode);
        }

        private IEnumerable<Document> GetDocumentList()
        {
            return new[]
                {
                    new Document()
                    {
                        Id = Guid.NewGuid(),
                        Location = string.Empty,
                        Content = new byte[] { },
                        Order = 4,
                        Name = "order-4"
                    },
                    new Document()
                    {
                        Id = Guid.NewGuid(),
                        Location = string.Empty,
                        Content = new byte[] { },
                        Order = 2,
                        Name = "order-2"
                    }
                };
        }

        [Test]
        public async Task Document_List_Returns_All_Documents()
        {
            var repo = Substitute.For<IDocumentRepository>();
            repo.ListAllAsync().Returns(GetDocumentList());

            DocumentOrchestrator orchestrator = new DocumentOrchestrator(repo);

            IActionResult result = await orchestrator.ListAsync();
            var typedResult = result as OkObjectResult;
            var value = typedResult.Value as IEnumerable<DocumentListItem>;

            Assert.IsNotNull(typedResult);
            Assert.AreEqual(200, typedResult.StatusCode);
            Assert.That(value.ToList(), Has.Count.EqualTo(2));
        }

        [Test]
        public async Task Document_List_Returns_Documents_In_Order()
        {
            var repo = Substitute.For<IDocumentRepository>();
            repo.ListAllAsync().Returns(GetDocumentList());

            DocumentOrchestrator orchestrator = new DocumentOrchestrator(repo);

            IActionResult result = await orchestrator.ListAsync();
            var typedResult = result as OkObjectResult;
            var value = typedResult.Value as IEnumerable<DocumentListItem>;

            Assert.AreEqual("order-2", value.First().Name);
            Assert.AreEqual("order-4", value.Skip(1).First().Name);
        }

        [Test]
        public async Task Document_Reorder_Updates_Each_Document()
        {
            var repo = Substitute.For<IDocumentRepository>();

            DocumentOrchestrator orchestrator = new DocumentOrchestrator(repo);

            var requests = new[]
            {
                new DocumentOrderRequest() {
                    DocumentId = Guid.NewGuid().ToString(),
                    Order = 2
                },
                new DocumentOrderRequest() {
                    DocumentId = Guid.NewGuid().ToString(),
                    Order = 1
                }
            };

            await orchestrator.OrderAsync(requests);

            await repo.Received(2).UpdateAsync(Arg.Any<Guid>(), Arg.Any<int>());
        }

        [Test]
        public async Task Document_Get_Returns_Result()
        {
            var repo = Substitute.For<IDocumentRepository>();

            Guid expectedGuid = Guid.NewGuid();
            byte[] expectedContent = new byte[] { 0x1 };
            string expectedLocation = "location";
            string expectedName = "name";
            int expectedOrder = 1;

            repo.GetAsync(Arg.Any<Guid>()).Returns(new Document()
            {
                Id = expectedGuid,
                Content = expectedContent,
                Location = expectedLocation,
                Name = expectedName,
                Order = expectedOrder
            });

            DocumentOrchestrator orchestrator = new DocumentOrchestrator(repo);
            IActionResult result = await orchestrator.GetAsync(expectedGuid.ToString());
            var typedResult = result as OkObjectResult;
            var value = typedResult.Value as Document;

            Assert.IsNotNull(typedResult);
            Assert.AreEqual(200, typedResult.StatusCode);

            Assert.AreEqual(value.Id, expectedGuid);
            Assert.AreEqual(value.Content, expectedContent);
            Assert.AreEqual(value.Location, expectedLocation);
            Assert.AreEqual(value.Name, expectedName);
            Assert.AreEqual(value.Order, expectedOrder);
        }

        [Test]
        public async Task Document_Get_Returns_Not_Found()
        {
            var repo = Substitute.For<IDocumentRepository>();
            repo.GetAsync(Arg.Any<Guid>()).Returns((Document)null);

            DocumentOrchestrator orchestrator = new DocumentOrchestrator(repo);
            IActionResult result = await orchestrator.GetAsync(Guid.NewGuid().ToString());
            var typedResult = result as NotFoundResult;

            Assert.IsNotNull(typedResult);
            Assert.AreEqual(404, typedResult.StatusCode);
        }

        [Test]
        public async Task Document_Get_Returns_Bad_Request()
        {
            DocumentOrchestrator orchestrator = new DocumentOrchestrator(Substitute.For<IDocumentRepository>());
            IActionResult result = await orchestrator.GetAsync("invalid-guid");
            var typedResult = result as BadRequestResult;

            Assert.IsNotNull(typedResult);
            Assert.AreEqual(400, typedResult.StatusCode);
        }

        [Test]
        public async Task Document_Delete()
        {
            var repo = Substitute.For<IDocumentRepository>();

            Guid reference = Guid.NewGuid();

            repo.GetAsync(Arg.Any<Guid>()).Returns(new Document()
            {
                Id = reference,
            });

            DocumentOrchestrator orchestrator = new DocumentOrchestrator(repo);
            await orchestrator.DeleteAsync(reference.ToString());

            await repo.Received(1).DeleteAsync(reference);
        }

        [Test]
        public async Task Document_Delete_Returns_Not_Found()
        {
            var repo = Substitute.For<IDocumentRepository>();

            string reference = Guid.NewGuid().ToString();

            repo.GetAsync(Arg.Any<Guid>()).Returns((Document)null);

            DocumentOrchestrator orchestrator = new DocumentOrchestrator(repo);
            await orchestrator.DeleteAsync(reference);

            IActionResult result = await orchestrator.DeleteAsync(reference);
            var typedResult = result as NotFoundResult;

            Assert.IsNotNull(typedResult);
            Assert.AreEqual(404, typedResult.StatusCode);
        }
    }
}