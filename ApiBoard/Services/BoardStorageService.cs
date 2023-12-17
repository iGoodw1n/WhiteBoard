using ApiBoard.Data;
using ApiBoard.Helpers;
using System.Collections.Concurrent;
using System.Text.Json.Nodes;
using System.Text.Json;

namespace ApiBoard.Services;

public class BoardStorageService
{
    private readonly ConcurrentDictionary<string, Snapshot> SnapshotsByBoard = [];
    private readonly ConcurrentDictionary<string, string> UsersByGroups = [];

    public Snapshot GetBoardById(string id)
    {
        if (!SnapshotsByBoard.TryGetValue(id, out var snapshot))
        {
            snapshot = new Snapshot();
            SnapshotsByBoard[id] = new Snapshot();
        }

        return snapshot;
    }

    public Dictionary<string, object> GetAllBoards()
    {
        return SnapshotsByBoard
            .Select(kvp => KeyValuePair.Create(
                kvp.Key,
                (object)new { kvp.Value.Store, schema = JsonSerializer.Deserialize<JsonObject>(StringStorage.Schema) }))
            .ToDictionary();
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
