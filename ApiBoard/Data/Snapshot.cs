using ApiBoard.Helpers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApiBoard.Data
{
    public class Snapshot
    {
        [JsonPropertyName("store")]
        public Dictionary<string, JsonElement> Store { get; set; } = [];

        //[JsonPropertyName("schema")]
        //public string Schema { get; set; } = StringStorage.Schema.Replace("\r\n", "");
    }
}