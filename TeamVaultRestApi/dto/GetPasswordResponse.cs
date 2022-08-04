using Newtonsoft.Json;

namespace TeamVaultRestApi.dto
{
    class GetPasswordResponse
    {
        [JsonProperty("password")]
        public string Password { get; set; }
    }
}
