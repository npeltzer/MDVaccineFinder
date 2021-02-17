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
        private static string phoneNumber = "";
        private static int provider = 0;
        private static float latitude = 0;
        private static float longitude = 0;

        private const string gmailAddress = "MdVaccineFinder@gmail.com";
        private const string gmailPassword = "PASSWORDS";







        static async Task Main(string[] args)
        {
            Console.WriteLine("Welcome to the MD Vaccine Finder app!");

            Console.Write("Enter the phone number you wish to be contacted at: ");
            phoneNumber = Console.ReadLine().Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");

            Console.WriteLine("Please select your cellphone provider from the list: ");
            Console.WriteLine("\t1) Verizon");
            Console.WriteLine("\t2) AT&T");
            Console.WriteLine("\t3) Sprint");
            Console.WriteLine("\t4) TMobile");
            Console.WriteLine("\t5) Virgin Mobile");
            Console.WriteLine("\t6) Nextel");
            Console.WriteLine("\t7) Boost");
            Console.Write("Provider: ");
            provider = int.Parse(Console.ReadLine());

            Console.Write("Enter your MD Zip code: ");
            var zipCode = Console.ReadLine();
            GetLatAndLongFromZipCode(zipCode);

            while (true)
            {
                Console.WriteLine("Would you like to send a test txt or start searching? ");
                Console.WriteLine("\t1) Test");
                Console.WriteLine("\t2) Search");
                Console.Write("Choice: ");
                var testChoice = Console.ReadLine();

                if (testChoice == "1")
                {
                    SendMessage("This is a test txt from the MD Vaccine Search App.");
                }
                else
                {
                    Console.WriteLine("Leave the application running in the background if you wish for it to continually search.");
                    Console.WriteLine("If you wish to stop searching, close the terminal window.");
                    Console.WriteLine("Starting to search... ");

                    while (true)
                    {
                        Console.WriteLine("Calling Walgreens API...");
                        await CallWalgreeensAPI();
                        Console.WriteLine("Calling CVS API...");
                        await CallCVSAPI();
                        Console.WriteLine("Calling Giant API...");
                        await CallGiantAPI();
                        Console.WriteLine("Calling MD Mass Vax API...");
                        await CallMdMassVaxAPI();
                        Thread.Sleep(120000);
                        Console.Clear();
                    }
                }
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
        private static async Task CallGiantAPI()
        {
            try
            {
                var result = await client.GetAsync("https://giantfoodsched.rxtouch.com/rbssched/program/covid19/Patient/Advisory");

                var responseText = await result.Content.ReadAsStringAsync();

                if (!responseText.Contains("There are currently no COVID-19 vaccine appointments available"))
                {
                    SendMessage("There are vaccines available at Giant!\n https://giantfoodsched.rxtouch.com/rbssched/program/covid19/Patient/Advisory");
                }

            }
            catch
            {
                Console.WriteLine("Call to Giant API Failed...");
            }
        }
        private static void SendMessage(string message)
        {
            var mailMessage = new MailMessage();
            mailMessage.From = new MailAddress(gmailAddress);
            var providerExtension = "";

            switch (provider)
            {
                case 1:
                    providerExtension = "@vtext.com";
                    break;
                case 2:
                    providerExtension = "@txt.att.net";
                    break;
                case 3:
                    providerExtension = "@messaging.sprintpcs.com";
                    break;
                case 4:
                    providerExtension = "@tmomail.net";
                    break;
                case 5:
                    providerExtension = "@vmobl.com";
                    break;
                case 6:
                    providerExtension = "@messaging.nextel.com";
                    break;
                case 7:
                    providerExtension = "@myboostmobile.com";
                    break;
                default:
                    providerExtension = "";
                    break;
            }


            mailMessage.To.Add(new MailAddress(phoneNumber + providerExtension));
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

        private static void GetLatAndLongFromZipCode(string zipCode)
        {
            try
            {
                var result = client.GetAsync("http://api.zippopotam.us/us/" + zipCode).Result;
                var responseText = result.Content.ReadAsStringAsync().Result;
                var response = JsonConvert.DeserializeObject<ZipCodeResponse>(responseText);
                latitude = float.Parse(response.Places[0].Latitude);
                longitude = float.Parse(response.Places[0].Longitude);


            }
            catch
            {
                Console.WriteLine("Failed to find lat and long for zip code entered please try again.");
            }
        }


    }
}

