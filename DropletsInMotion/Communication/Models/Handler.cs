using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DropletsInMotion.Communication.Models
{
    public class Handler
    {
        [JsonPropertyName("request")]
        public required object Request { get; set; }
        [JsonPropertyName("response")]
        public required string Response { get; set; }
    }
}
