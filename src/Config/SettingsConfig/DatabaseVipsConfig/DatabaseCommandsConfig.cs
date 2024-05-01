using System.Text.Json.Serialization;

namespace Core.Config;

public class DatabaseCommandsConfig
{
    // Admin - service
    [JsonPropertyName("css_vp_service_enable")]
    public CommandConfig ServiceEnable { get; set; } = new();

    [JsonPropertyName("css_vp_service_disable")]
    public CommandConfig ServiceDisable { get; set; } = new();

    [JsonPropertyName("css_vp_service_delete")]
    public CommandConfig ServiceDelete { get; set; } = new();

    [JsonPropertyName("css_vp_service_info")]
    public CommandConfig ServiceInfo { get; set; } = new();

    // Admin - player
    [JsonPropertyName("css_vp_player_info")]
    public CommandConfig PlayerInfo { get; set; } = new();

    [JsonPropertyName("css_vp_player_addflags")]
    public CommandConfig PlayerAddFlags { get; set; } = new();

    [JsonPropertyName("css_vp_player_addgroup ")]
    public CommandConfig PlayerAddGroup { get; set; } = new();

    // Player - player
    [JsonPropertyName("css_services ")]
    public CommandConfig Services { get; set; } = new();
}

