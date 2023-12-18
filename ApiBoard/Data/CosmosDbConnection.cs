using Microsoft.Azure.Cosmos;

namespace ApiBoard.Data;

public record class CosmosDbConnection(CosmosClient CosmosClient, Database Database, Container Container)
{
}
