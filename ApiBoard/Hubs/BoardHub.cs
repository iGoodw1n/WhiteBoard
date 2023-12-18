using ApiBoard.Data;
using ApiBoard.Helpers;
using ApiBoard.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Primitives;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ApiBoard.Hubs
{
    public class BoardHub : Hub
    {
        private BoardCloudStorageService _storageService;
        private GroupService _groupService;

        public BoardHub(BoardCloudStorageService boardStorageService, GroupService groupService)
        {
            _storageService = boardStorageService;
            _groupService = groupService;
        }
        public async override Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            var group = Context.GetHttpContext().Request.Query["boardId"];

            if (string.IsNullOrEmpty(group.ToString()))
            {
                Context.Abort();
                return;
            }

            await HandleUserConnection(group);
        }

        public async Task Update(UpdateMessage message)
        {
            var groupName = GetGroupName();
            if (groupName is null)
            {
                Context.Abort();
                return;
            }

            var board = await _storageService.GetBoardById(groupName);
            await UpdateData(message, groupName, board.Snapshot);
            _storageService.SaveUpdates(board);
        }

        public async Task Recovery()
        {
            var groupName = GetGroupName();
            if (groupName is null)
            {
                Context.Abort();
                return;
            }

            await Clients.Caller.SendAsync("recovery", _storageService.GetBoardById(groupName));
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            _groupService.RemoveFromGroup(Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }

        private async Task HandleUserConnection(StringValues group)
        {
            await InitClient(group);
            await AddToGroup(group);
        }
        private async Task AddToGroup(StringValues group)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, group.ToString());
            _groupService.AddToGroup(Context.ConnectionId, group.ToString());
        }

        private async Task InitClient(StringValues group)
        {
            var board = await _storageService.GetBoardById(group.ToString());
            await Clients.Caller
                .SendAsync(
                "init",
                new
                {
                    store = board.Snapshot.Store,
                    schema = JsonSerializer.Deserialize<JsonObject>(StringStorage.Schema)
                });
        }

        private string? GetGroupName()
        {
            return _groupService.GetGroupName(Context.ConnectionId);
        }

        private async Task UpdateData(UpdateMessage message, string groupName, Snapshot snapshot)
        {
            try
            {
                HandleUpdates(message, snapshot);
            }
            catch
            {
                await Clients.Caller.SendAsync("recovery", snapshot);
            }

            await Clients.OthersInGroup(groupName).SendAsync("update", message);
        }

        private static void HandleUpdates(UpdateMessage message, Snapshot board)
        {
            foreach (var update in message.updates)
            {
                var changes = update.changes;
                UpdateAdds(board, changes);
                UpdateUpdates(board, changes);
                UpdateDeletes(board, changes);
            }
        }

        private static void UpdateAdds(Snapshot board, Changes changes)
        {
            foreach (var added in changes.added)
            {
                board.Store.AddOrUpdate(added.Key, (_) => added.Value, (_, _) => added.Value);
            }
        }

        private static void UpdateUpdates(Snapshot board, Changes changes)
        {
            foreach (var updated in changes.updated)
            {
                board.Store[updated.Key] = updated.Value[1];
            }
        }

        private static void UpdateDeletes(Snapshot board, Changes changes)
        {
            foreach (var removed in changes.removed)
            {
                board.Store.Remove(removed.Key, out var _);
            }
        }
    }
}
