using ApiBoard.Data;
using ApiBoard.Helpers;
using Microsoft.Azure.Cosmos;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Collections.Frozen;

namespace ApiBoard.Services;

public class BoardCloudStorageService : IDisposable
{
    private readonly CosmosClient _cosmosClient;
    private readonly Database _database;
    private readonly Container _container;
    private readonly ConcurrentDictionary<string, Board> _boards = new();
    private readonly ConcurrentBag<string> _updatedBoards = new();
    private readonly ConcurrentDictionary<string, DateTime> _boardLastSaveToDb = new();

    public BoardCloudStorageService(CosmosDbConnection cosmosDbConnection) => 
        (_cosmosClient, _database, _container) = cosmosDbConnection;

    public async Task<Board> GetBoardById(string id)
    {
        if (_boards.TryGetValue(id, out var board))
        {
            return board;
        }

        board = await GetFromDb(id);
        _boards.AddOrUpdate(
            id,
            _ => board,
            (_,currentBoard) =>
                board.Snapshot.Store.Count > currentBoard.Snapshot.Store.Count ? board : currentBoard
        );
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

    public void SaveUpdates(Board board)
    {
        _updatedBoards.Add(board.Id);
    }

    public void Dispose()
    {
        _cosmosClient.Dispose();
    }

    public (Action SaveChanges, Action CleanCache) GetHandlers()
    {
        return (SaveUpdatesToDbAsync, CleanLocalCache);
    }

    private async Task<Board> GetFromDb(string id)
    {
        ItemResponse<Board> boardResponse;
        try
        {
            boardResponse = await _container.ReadItemAsync<Board>(id, new PartitionKey(StringStorage.PartitionKey));
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            boardResponse = await _container.CreateItemAsync<Board>(new Board { Id = id }, new PartitionKey(StringStorage.PartitionKey));
        }

        return boardResponse.Resource;
    }

    private void SaveUpdatesToDbAsync()
    {
        var updatedBoards = _updatedBoards.ToFrozenSet();
        _updatedBoards.Clear();
        foreach (var board in updatedBoards)
        {
            _container.ReplaceItemAsync(_boards[board], board, new PartitionKey(StringStorage.PartitionKey));
            _boardLastSaveToDb.AddOrUpdate(board, _ => DateTime.Now, (_,_) => DateTime.Now);
        }
    }

    private void CleanLocalCache()
    {
        var lastUpdatedBoards = _boardLastSaveToDb.ToFrozenDictionary();
        foreach (var kvp in lastUpdatedBoards)
        {
            if (DateTime.UtcNow.Subtract(kvp.Value) < TimeSpan.FromMinutes(60))
            {
                continue;
            }

            _boards.Remove(kvp.Key, out var _);
            _boardLastSaveToDb.Remove(kvp.Key, out _);
        }
    }
}
