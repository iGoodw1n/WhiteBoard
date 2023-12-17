using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApiBoard.Data
{
    public class UpdateMessage
    {
        public List<Update> updates { get; set; }
    }

    public class Changes
    {
        public Dictionary<string, JsonElement> added { get; set; }
        public Dictionary<string, JsonElement[]> updated { get; set; }
        public Dictionary<string, JsonElement> removed { get; set; }
    }

    public class Update
    {
        public Changes changes { get; set; }
        public string source { get; set; }
    }
}
