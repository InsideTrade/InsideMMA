using Newtonsoft.Json;

namespace Inside_MMA.Models
{
    public enum Level2ArgsType { Type, AlertSize, AlertTwoSize }

    public class Level2Args
    {
        [JsonProperty]
        public string Type { get; set; }
        [JsonProperty]
        public int AlertSize { get; set; }
        [JsonProperty]
        public int AlertTwoSize { get; set; }
    }
}