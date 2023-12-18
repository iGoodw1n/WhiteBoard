using Newtonsoft.Json.Linq;

namespace ApiBoard.Data
{
    public class UpdateMessage
    {
        public List<Update> updates { get; set; }
    }

    public class Changes
    {
        public Dictionary<string, JObject> added { get; set; }
        public Dictionary<string, JObject[]> updated { get; set; }
        public Dictionary<string, JObject> removed { get; set; }
    }

    public class Update
    {
        public Changes changes { get; set; }
        public string source { get; set; }
    }
}
