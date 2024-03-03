using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace VIP;

public class RandomVIPData
{
    [JsonPropertyName("enabled")]
    public bool enabled { get; set; } = false;

    [JsonPropertyName("afterRound")]
    public int afterRound { get; set; } = 3;

    [JsonPropertyName("repeatPicking")]
    public int repeatPicking { get; set; } = 3;


    [JsonPropertyName("permissions")]
    public List<string> permissions { get; set; } = new List<string>();
}

