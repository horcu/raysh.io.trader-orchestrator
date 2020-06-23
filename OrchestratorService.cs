using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace raysh.io.trader_orchestrator
{
    internal class OrchestratorService
    {
        //[JsonProperty("name")]
        public string Name { get; set; }

        //[JsonProperty("status")]
        public string status { get; set; }

        //[JsonProperty("timestamp")]
        public long Time { get; set; }

        //[JsonProperty("command")]
        public string Command { get; set; }
    }
}
