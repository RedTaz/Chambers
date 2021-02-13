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
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Microsoft.AspNetCore.Http.Internal;
using System.IO;

namespace Chambers.Api.Tests
{
    public class DocumentServiceTests
    {
        [Test]
        public async Task Document_Upload_200_Response()
        {
            Document document = new Document();
            document.Content = TestDocument.Get(TestDocumentType.Pdf);
            document.Name = "my_document.pdf";

            DocumentService service = new DocumentService(Substitute.For<IDocumentRepository>());

            IActionResult result = await service.Upload(document);
            var typedResult = result as OkObjectResult;

            Assert.IsNotNull(typedResult);
            Assert.AreEqual(200, typedResult.StatusCode);
        }

        [Test]
        public async Task Document_Upload_Stores_Document()
        {
            var repo = Substitute.For<IDocumentRepository>();

            DocumentService service = new DocumentService(repo);

            Document document = new Document();
            document.Content = TestDocument.Get(TestDocumentType.Pdf);
            document.Name = "my_document.pdf";

            await service.Upload(document);

            await repo.Received(1).CreateAsync(document);
        }

        [Test]
        public async Task Document_Upload_Rejects_NonPdf()
        {
            var repo = Substitute.For<IDocumentRepository>();

            DocumentService service = new DocumentService(repo);

            Document document = new Document();
            document.Content = TestDocument.Get(TestDocumentType.Word);
            document.Name = "my_document.docx";

            IActionResult result = await service.Upload(document);
            var typedResult = result as UnsupportedMediaTypeResult;

            Assert.IsNotNull(typedResult);
            Assert.AreEqual(415, typedResult.StatusCode);
        }

        [Test]
        public async Task Document_Upload_Rejects_Large_Payload()
        {
            var repo = Substitute.For<IDocumentRepository>();

            DocumentService service = new DocumentService(repo);

            Document document = new Document();
            document.Content = TestDocument.Get(TestDocumentType.LargePdf);
            document.Name = "my_document.pdf";

            IActionResult result = await service.Upload(document);
            var typedResult = result as StatusCodeResult;

            Assert.IsNotNull(typedResult);
            Assert.AreEqual(413, typedResult.StatusCode);
        }

        [Test]
        public async Task Document_List_Returns_All_Documents()
        {
            var repo = Substitute.For<IDocumentRepository>();
            repo.ListAllAsync().Returns(
                new[]
                {
                    new Document()
                    {
                        Id = Guid.NewGuid(),
                        Location = string.Empty,
                        Content = new byte[] { },
                        Name = "Doc1"
                    },
                    new Document()
                    {
                        Id = Guid.NewGuid(),
                        Location = string.Empty,
                        Content = new byte[] { },
                        Name = "Doc2"
                    }
                }
            ); ;

            DocumentService service = new DocumentService(repo);

            Document document = new Document();
            document.Content = TestDocument.Get(TestDocumentType.Pdf);
            document.Name = "my_document.pdf";

            IActionResult result = await service.List(null);
            var typedResult = result as OkObjectResult;
            var value = typedResult.Value as IEnumerable<DocumentListItem>;

            Assert.IsNotNull(typedResult);
            Assert.AreEqual(200, typedResult.StatusCode);
            Assert.That(value.ToList(), Has.Count.EqualTo(2));
        }

        [Test]
        public void Document_Reorder()
        {

        }

        [Test]
        public void Document_Download()
        {

        }

        [Test]
        public void Document_Delete()
        {

        }
    }
}