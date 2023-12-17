using System.Text.Json.Nodes;

namespace ApiBoard.Data
{
    public class FullData
    {
        public Snapshot Store { get; set; }
        public string Schema { get; set; }
    }
}
