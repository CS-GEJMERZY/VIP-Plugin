using System.Text.Json.Serialization;

namespace Core.Config;

public class DatabaseCommandsConfig
{
    // Admin - service
    [JsonPropertyName("css_vp_service_enable")]
    public CommandConfig ServiceEnable { get; set; } = new() { Permissions = ["@css/root"] };

    [JsonPropertyName("css_vp_service_disable")]
    public CommandConfig ServiceDisable { get; set; } = new() { Permissions = ["@css/root"] };

    [JsonPropertyName("css_vp_service_delete")]
    public CommandConfig ServiceDelete { get; set; } = new() { Permissions = ["@css/root"] };

    [JsonPropertyName("css_vp_service_info")]
    public CommandConfig ServiceInfo { get; set; } = new() { Permissions = ["@css/root"] };

    // Admin - player
    [JsonPropertyName("css_vp_player_info")]
    public CommandConfig PlayerInfo { get; set; } = new() { Permissions = ["@css/root"] };

    [JsonPropertyName("css_vp_player_removeall")]
    public CommandConfig PlayerRemoveAll { get; set; } = new() { Permissions = ["@css/root"] };

    [JsonPropertyName("css_vp_player_addflags")]
    public CommandConfig PlayerAddFlags { get; set; } = new() { Permissions = ["@css/root"] };

    [JsonPropertyName("css_vp_player_addgroup ")]
    public CommandConfig PlayerAddGroup { get; set; } = new() { Permissions = ["@css/root"] };

    // Player - player
    [JsonPropertyName("css_services ")]
    public CommandConfig Services { get; set; } = new();
}

