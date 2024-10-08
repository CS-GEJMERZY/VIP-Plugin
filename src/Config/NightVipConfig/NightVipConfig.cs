﻿using System.Text.Json.Serialization;

namespace Core.Config;

public class NightVipConfig
{
    [JsonPropertyName("Enabled")]
    public bool Enabled { get; set; } = false;
    
    
    [JsonPropertyName("SendMessageOnVIPResaved")]
    public bool SendMessageOnVIPResaved {get;set;} = false; // center text

    [JsonPropertyName("StartHour")]
    public int StartHour { get; set; } = 22;

    [JsonPropertyName("EndHour")]
    public int EndHour { get; set; } = 8;

    [JsonPropertyName("TimeZone")]
    public string TimeZone { get; set; } = "UTC";

    [JsonPropertyName("RequiredNickPhrase")]
    public string RequiredNickPhrase { get; set; } = "YourSite.com";

    [JsonPropertyName("RequiredScoreboardTag")]
    public string RequiredScoreboardTag { get; set; } = "YourSite.com";

    [JsonPropertyName("PermissionsGranted ")]
    public List<string> PermissionsGranted { get; set; } = [];

    [JsonPropertyName("PermissionExclude")]
    public List<string> PermissionsExclude { get; set; } = [];
}

