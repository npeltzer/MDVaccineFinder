using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VaccineFinder.Models
{
    public class MdMassVaxRequest
    {

        [JsonProperty("startDate")]
        public string StartDate { get; set; }

        [JsonProperty("endDate")]
        public string EndDate { get; set; }

        [JsonProperty("vaccineData")]
        public string VaccineData { get; set; }

        [JsonProperty("doseNumber")]
        public int DoseNumber { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }
    public class MdMassVaxResponse
    {
        [JsonProperty("locationExtId")]
        public string LocationExtId { get; set; }

        [JsonProperty("vaccineData")]
        public string VaccineData { get; set; }

        [JsonProperty("availability")]
        public List<Availability> Availability { get; set; }
    }
    public class Availability
    {
        [JsonProperty("date")]
        public string Date { get; set; }

        [JsonProperty("available")]
        public bool Available { get; set; }
    }
}



