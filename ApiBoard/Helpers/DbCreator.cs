using ApiBoard.Data;
using Microsoft.Azure.Cosmos;

namespace ApiBoard.Helpers;

public static class DbCreator
{
    public static async Task<CosmosDbConnection> EnsureDbCreatedAsync(string envForEndPoint, string envForprimaryKey)
    {
        var endpointUri = Environment.GetEnvironmentVariable(envForEndPoint);
        var primaryKey = Environment.GetEnvironmentVariable(envForprimaryKey);
        var cosmosClient = new CosmosClient(endpointUri, primaryKey, new CosmosClientOptions() { ApplicationName = "WhiteBoardApplication" });
        Database database = await cosmosClient.CreateDatabaseIfNotExistsAsync("WhiteBoardDb");
        var container = await database.CreateContainerIfNotExistsAsync("Boards", "/partitionKey");

        return new CosmosDbConnection(cosmosClient, database, container);
    }
}
