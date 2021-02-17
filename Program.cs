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

        //Your Lat and Long
        private const float latitude = 0;
        private const float longitude = 0;




        static async Task Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Calling Walgreens API...");
                await CallWalgreeensAPI();
                Console.WriteLine("Calling CVS API...");
                await CallCVSAPI();
                Console.WriteLine("Calling MD Mass Vax API...");
                await CallMdMassVaxAPI();
                Thread.Sleep(120000);
                Console.Clear();
            }
        }

        private static async Task CallWalgreeensAPI()
        {
            try
            {
                var walgreensRequest = new WalgreensRequest()
                {
                    ServiceId = "99",
                    Radius = 25,
                    Position = new Position() { Latitude = latitude, Longitude = longitude },
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
            catch
            {
                Console.WriteLine("Call to Walgreens API Failed...");
            }
        }

        private static async Task CallCVSAPI()
        {
            try
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
                        message = "There are vaccines available at CVS!\n https://www.cvs.com/immunizations/covid-19-vaccine";
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(message))
                    SendMessage(message);
            }
            catch
            {
                Console.WriteLine("Call to CVS API Failed...");
            }
        }

        private static async Task CallMdMassVaxAPI()
        {
            try
            {
                var mssVaxReq = new MdMassVaxRequest()
                {
                    StartDate = DateTime.Now.ToString("yyyy-MM-dd"),
                    EndDate = DateTime.Now.AddDays(7).ToString("yyyy-MM-dd"),
                    VaccineData = "WyJhMVYzZDAwMDAwMDAyMmdFQUEiXQ==",
                    DoseNumber = 1,
                    Url = "https://massvax.maryland.gov/appointment-select"
                };

                var json = JsonConvert.SerializeObject(mssVaxReq);
                StringContent sc = new StringContent(json, Encoding.UTF8, "application/json");

                var result = await client.PostAsync("https://api-massvax.maryland.gov/public/locations/a0Z3d000000C1bOEAS/availability", sc);

                var responseText = await result.Content.ReadAsStringAsync();
                var response = JsonConvert.DeserializeObject<MdMassVaxResponse>(responseText);
                foreach (var day in response.Availability)
                {
                    if (day.Available)
                    {
                        SendMessage("There is a vaccine available at the Six Flags Mass Vax Location!\n https://massvax.maryland.gov/location-search");
                        break;
                    }
                }
            }
            catch
            {
                Console.WriteLine("Call to MD Mass Vax API Failed...");
            }
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
            try
            {
                smtpClient.Send(mailMessage);
            }
            catch
            {
                Console.WriteLine("Message failed to send...");
                Console.WriteLine(message);
            }
        }


    }
}

