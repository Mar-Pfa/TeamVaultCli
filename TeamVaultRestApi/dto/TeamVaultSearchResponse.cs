using System.Collections.Generic;
using Newtonsoft.Json;

namespace TeamVaultRestApi.dto
{
    public class TeamVaultSearchResponse
    {
        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("next")]
        public string Next { get; set; }

        [JsonProperty("previous")]
        public string Previous { get; set; }

        [JsonProperty("results")]
        public List<TeamVaultSecret> Results { get; set; }
    }
}
