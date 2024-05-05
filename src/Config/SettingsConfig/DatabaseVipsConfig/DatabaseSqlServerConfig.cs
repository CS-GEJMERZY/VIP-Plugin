using System.Text.Json.Serialization;

namespace Core.Config;

public class DatabaseSqlServerConfig
{
    [JsonPropertyName("host")]
    public string Host { get; set; } = "www.site.com";

    [JsonPropertyName("port")]
    public uint Port { get; set; } = 3306;

    [JsonPropertyName("database")]
    public string Database { get; set; } = "vip-plugin";

    [JsonPropertyName("username")]
    public string Username { get; set; } = "user";

    [JsonPropertyName("password")]
    public string Password { get; set; } = "password";
}
