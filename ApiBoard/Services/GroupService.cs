using System.Collections.Concurrent;

namespace ApiBoard.Services
{
    public class GroupService
    {
        private readonly ConcurrentDictionary<string, string> UsersByGroups = [];
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
}
