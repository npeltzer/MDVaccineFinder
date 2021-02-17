using System;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using VaccineFinder.Models;
using System.Text;
using System.Net.Mail;
using System.Net;
using System.Threading;

namespace VaccineFinder
{
    class Program
    {
        private static readonly HttpClient client = new HttpClient();
        private const string gmailAddress = "YOUR_GMAIL_ADDRESS";
        private const string gmailPassword = "YOUR_GMAIL_PASSWORD";
        private const string phoneNumber = "YOUR_CELLPHONE_NUMBER";



        static async Task Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Calling Walgreens...");
                await CallWalgreeensAPI();
                Console.WriteLine("Calling CVS...");
                await CallCVSAPI();
                Thread.Sleep(120000);
                Console.Clear();
            }
        }

        private static async Task CallWalgreeensAPI()
        {
            var walgreensRequest = new WalgreensRequest()
            {
                ServiceId = "99",
                Radius = 25,
                Position = new Position() { Latitude = 39.2994145f, Longitude = -76.60372219999999f },
                AppointmentAvailability = new AppointmentAvailability() { StartDateTime = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd") }

            };

            var json = JsonConvert.SerializeObject(walgreensRequest);
            StringContent sc = new StringContent(json, Encoding.UTF8, "application/json");

            var result = await client.PostAsync("https://www.walgreens.com/hcschedulersvc/svc/v1/immunizationLocations/availability", sc);

            var responseText = await result.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<WalgreensResponse>(responseText);
            if (response.AppointmentAvailability)
                SendMessage("There is a vaccine available at Walgreens!\n https://www.walgreens.com/findcare/vaccination/covid-19/location-screening");

        }

        private static async Task CallCVSAPI()
        {
            client.DefaultRequestHeaders.Referrer = new Uri("https://www.cvs.com/immunizations/covid-19-vaccine");
            var result = await client.GetAsync("https://www.cvs.com/immunizations/covid-19-vaccine.vaccine-status.MD.json?vaccineinfo");

            var responseText = await result.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<CVSResponse>(responseText);
            var message = string.Empty;
            foreach (var city in response?.ResponsePayloadData?.Data?.MD)
            {
                if (city.TotalAvailable != "0")
                {
                    message = "There are vaccines available at CVS!\nhttps://www.cvs.com/immunizations/covid-19-vaccine";
                    break;
                }
            }

            if (!string.IsNullOrEmpty(message))
                SendMessage(message);

        }


        private static void SendMessage(string message)
        {
            var mailMessage = new MailMessage();
            mailMessage.From = new MailAddress(gmailAddress);

            //AT&T: ##@txt.att.net
            //Verizon: ##@vtext.com
            //Sprint: ##@messaging.sprintpcs.com
            //TMobile: ##@tmomail.net
            //Virgin Mobile: ##@vmobl.com
            //Nextel: ##@messaging.nextel.com
            //Boost: ##@myboostmobile.com

            mailMessage.To.Add(new MailAddress(phoneNumber + "@vtext.com"));
            mailMessage.Body = message;
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(gmailAddress, gmailPassword),
                EnableSsl = true,
            };
            smtpClient.Send(mailMessage);
        }


    }
}

