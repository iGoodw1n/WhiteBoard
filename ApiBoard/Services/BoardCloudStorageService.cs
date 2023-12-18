using ApiBoard.Data;
using ApiBoard.Helpers;
using System.Collections.Concurrent;
using System.Text.Json.Nodes;
using System.Text.Json;
using Microsoft.Azure.Cosmos;
using System.Net;
using Newtonsoft.Json.Linq;

namespace ApiBoard.Services;

public class BoardCloudStorageService
{
    private readonly ConcurrentDictionary<string, string> UsersByGroups = [];

    private readonly CosmosClient _cosmosClient;
    private readonly Database _database;
    private readonly Container _container;

    public BoardCloudStorageService(CosmosDbConnection cosmosDbConnection) => 
        (_cosmosClient, _database, _container) = cosmosDbConnection;

    public async Task<Board> GetBoardById(string id)
    {
        ItemResponse<Board> boardResponse;
        try
        {
            boardResponse = await _container.ReadItemAsync<Board>(id, new PartitionKey(StringStorage.PartitionKey));
            
        }
        catch(CosmosException ex) when(ex.StatusCode == HttpStatusCode.NotFound)
        {
            boardResponse = await _container.CreateItemAsync<Board>(new Board { Id = id }, new PartitionKey(StringStorage.PartitionKey));
        }

        var board = boardResponse.Resource;
        return board;
    }

    public async Task<Dictionary<string, object>> GetAllBoards()
    {
        var query = new QueryDefinition("SELECT * FROM c");
        var iterator = _container.GetItemQueryIterator<Board>(query);

        Dictionary<string, object> boards = [];

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();

            foreach (Board board in response)
            {
                if (board is not null && board.Snapshot is not null)
                {
                    boards.Add(board.Id, new { store = board.Snapshot.Store, schema = JObject.Parse(StringStorage.Schema) });
                }
            }
        }

        return boards;
    }

    public async Task SaveUpdates(Board board)
    {
        await _container.ReplaceItemAsync<Board>(board, board.Id, new PartitionKey(board.PartitionKey));
    }

    public string? GetGroupName(string connectionId)
    {
        if (UsersByGroups.TryGetValue(connectionId, out var group))
        {
            return group;
        }

        return null;
    }

    public void AddToGroup(string connectionId, string groupName)
    {
        UsersByGroups[connectionId] = groupName;
    }

    public void RemoveFromGroup(string connectionId)
    {
        UsersByGroups.TryRemove(connectionId, out var group);
    }
}
