using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;

namespace ApiBoard.Data
{
    public class Snapshot
    {
        [JsonProperty("store")]
        public ConcurrentDictionary<string, JObject> Store { get; set; } = [];
    }
}