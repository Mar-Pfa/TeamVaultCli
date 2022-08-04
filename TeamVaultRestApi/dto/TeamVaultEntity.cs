using Newtonsoft.Json;

namespace TeamVaultRestApi.dto
{
    public class TeamVaultEntity
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }
}