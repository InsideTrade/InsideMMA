using System;
using Newtonsoft.Json;

namespace Inside_MMA.Models
{
    public enum AllTradesFilterType { FilterSize, SelectSize, SelectPrice, MiOnly, BuySell, Time, EatenSize, MinOIDelta, OIDelta }
    public class AllTradesArgs
    {
        [JsonProperty]
        public bool IsFiltering { get; set; }
        [JsonProperty]
        public int FilterSize { get; set; }
        [JsonProperty]
        public bool IsSelecting{ get; set; }
        [JsonProperty]
        public int SelectSize { get; set; }
        [JsonProperty]
        public bool IsSelectingPrice { get; set; }
        [JsonProperty]
        public double SelectPrice { get; set; }
        [JsonProperty]
        public bool MiOnly { get; set; }
        [JsonProperty]
        public string BuySell { get; set; }
        [JsonProperty]
        public bool IsFilteringTime { get; set; }
        [JsonProperty]
        public TimeSpan From { get; set; }
        [JsonProperty]
        public TimeSpan To { get; set; }
        [JsonProperty]
        public int EatenSize { get; set; }
        [JsonProperty]
        public int MinOIDelta { get; set; }
        [JsonProperty]
        public int OIDelta { get; set; }
    }
}