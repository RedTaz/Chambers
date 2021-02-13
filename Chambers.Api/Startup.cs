using Chambers.Api;
using Chambers.Api.Data.Repositories;
using Chambers.Api.Orchestrators;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("Chambers.Api.Tests")]
[assembly: FunctionsStartup(typeof(Startup))]

namespace Chambers.Api
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddLogging();

            builder.Services.AddTransient<IDocumentRepository, DocumentRepository>();
            builder.Services.AddTransient<IDocumentOrchestrator, DocumentOrchestrator>();
            builder.Services.AddSingleton(new CosmosClient(Environment.GetEnvironmentVariable("Endpoint.Cosmos")));
            builder.Services.AddTransient((svc) => svc.GetService<CosmosClient>().GetContainer("document-service", "documents"));
        }
    }
}
