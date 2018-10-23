using Newtonsoft.Json;

namespace Inside_MMA.Models
{
    public class LogBookArgs
    {
        [JsonProperty]
        public int Ratio { get; set; }
        [JsonProperty]
        public int Size { get; set; }
        [JsonProperty]
        public bool Alert { get; set; }
        [JsonProperty]
        public double TriggerDelta { get; set; }
    }
}