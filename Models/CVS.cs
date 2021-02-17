using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VaccineFinder.Models
{
    public class CVSResponse
    {
        [JsonProperty("responsePayloadData")]
        public ResponsePayloadData ResponsePayloadData { get; set; }
    }
    public class ResponsePayloadData
    {
        [JsonProperty("currentTime")]
        public string CurrentTime { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }
    public class Data
    {
        [JsonProperty("MD")]
        public List<MD> MD { get; set; }
    }

    public class MD
    {
        [JsonProperty("totalAvailable")]
        public string TotalAvailable { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("pctAvailable")]
        public string PctAvailable { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }
}
