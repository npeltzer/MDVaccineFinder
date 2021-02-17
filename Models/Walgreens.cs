using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VaccineFinder.Models
{
    public class WalgreensRequest
    {
        [JsonProperty("serviceId")]
        public string ServiceId { get; set; }

        [JsonProperty("radius")]
        public int Radius { get; set; }

        [JsonProperty("position")]
        public Position Position { get; set; }

        [JsonProperty("appointmentAvailability")]
        public AppointmentAvailability AppointmentAvailability { get; set; }
    }
    public class WalgreensResponse
    {

        [JsonProperty("days")]
        public int Days { get; set; }
        [JsonProperty("stateName")]
        public string StateName { get; set; }

        [JsonProperty("radius")]
        public int Radius { get; set; }

        [JsonProperty("stateCode")]
        public string StateCode { get; set; }

        [JsonProperty("zipCode")]
        public string ZipCode { get; set; }

        [JsonProperty("appointmentAvailability")]
        public bool AppointmentAvailability { get; set; }
    }
    public class Position
    {
        [JsonProperty("latitude")]
        public float Latitude { get; set; }

        [JsonProperty("longitude")]
        public float Longitude { get; set; }

    }
    public class AppointmentAvailability
    {
        [JsonProperty("startDateTime")]
        public string StartDateTime { get; set; }
    }
}
