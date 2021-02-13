# Chambers

## Setup

The solution uses the [Azure Cosmos DB Emulator](https://docs.microsoft.com/en-us/azure/cosmos-db/local-emulator) to store all data.

The service assumes a cosmos database already exists with the following details:
* Database id: document-service
* Container id: documents
* Partition key: /Id

## Coverage

Test coverage is concentrated on the DocumentOrchestrator class which contains the majority of the business logic.


The Azure Function represented by the DocumentService class does not have any coverage, primarily because it
serves as a very thin wrapper for the DocumentOrchestrator. That said, binding the file upload information to
a model was less straightforward than anticipated, ideally some coverage of this binding code would be included
if time allowed.