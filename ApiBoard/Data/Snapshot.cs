using ApiBoard.Helpers;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApiBoard.Data
{
    public class Snapshot
    {
        [JsonPropertyName("store")]
        public ConcurrentDictionary<string, JsonElement> Store { get; set; } = [];

        //[JsonPropertyName("schema")]
        //public string Schema { get; set; } = StringStorage.Schema.Replace("\r\n", "");
    }
}