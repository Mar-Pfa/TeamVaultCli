using Newtonsoft.Json;

namespace TeamVaultRestApi.dto
{
        public class TeamVaultSearchDto
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("url")]
            public string Url { get; set; }

            public string Id()
            {
                return Url.Substring(9);
            }
    }
}