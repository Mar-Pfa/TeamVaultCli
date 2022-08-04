using System.Collections.Generic;
using Newtonsoft.Json;

namespace TeamVaultRestApi.dto
{
    public class TeamVaultEntitySearchResponse
    {
        [JsonProperty("results")]
        public List<TeamVaultEntity> Results { get; set; }
    }
}