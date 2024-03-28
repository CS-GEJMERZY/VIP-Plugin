using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Core.Config
{
    public class ExtraJumpsConfig
    {
        [JsonPropertyName("Count")]
        public int Count { get; set; } = 0;

        [JsonPropertyName("VelocityZ")]
        public double VelocityZ { get; set; } = 260.0;
    }
}