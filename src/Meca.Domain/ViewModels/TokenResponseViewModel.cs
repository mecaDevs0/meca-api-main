using Newtonsoft.Json;

public class TokenResponseViewModel
{
    [JsonProperty("access_token")]
    public string AccessToken { get; set; }

    [JsonProperty("refresh_token")]
    public string RefreshToken { get; set; }

    [JsonProperty("expires_in")]
    public long ExpiresIn { get; set; }

    [JsonProperty("expires")]
    public string Expires { get; set; }

    [JsonProperty("expires_type")]
    public string ExpiresType { get; set; }
}
