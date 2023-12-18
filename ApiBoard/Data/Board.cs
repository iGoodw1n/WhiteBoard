using ApiBoard.Helpers;
using Newtonsoft.Json;

namespace ApiBoard.Data
{
    public class Board
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "partitionKey")]
        public string PartitionKey { get; set; } = StringStorage.PartitionKey;

        public Snapshot Snapshot { get; set; } = new();
    }
}
