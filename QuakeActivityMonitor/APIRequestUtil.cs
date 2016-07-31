using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace QuakeActivityMonitor
{
    public class APIRequestUtil
    {
        private String BaseUrl;
        private String Parameters;
        private RootObject JsonOutput;

        public APIRequestUtil(String baseUrl, String parameters)
        {
            this.BaseUrl = baseUrl;
            this.Parameters = parameters;
        }

        /// <summary>
        /// Consumes the information from the API
        /// </summary>
        /// <returns></returns>
        public RootObject ConsumeAPI()
        {
            RunAsync().Wait();
            return JsonOutput;
        }

        /// <summary>
        /// Makes an API request
        /// </summary>
        /// <returns></returns>
        private async Task RunAsync()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(BaseUrl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    HttpResponseMessage response = await client.GetAsync(Parameters);
                    if (response.IsSuccessStatusCode)
                    {
                        var jsonResult = response.Content.ReadAsStringAsync().Result;
                        JsonOutput = JsonConvert.DeserializeObject<RootObject>(jsonResult);
                    }
                }
            }
            catch(Exception ex)
            {
                string errorLogPath = AppDomain.CurrentDomain.BaseDirectory +
                    "ErrorLog " + DateTime.Now.Date.Month + "-" +
                    DateTime.Now.Date.Day + "-" + DateTime.Now.Date.Year + " " +
                    DateTime.Now.TimeOfDay.Hours + "H" + DateTime.Now.TimeOfDay.Minutes + "M" + ".txt";
                ErrorLog log = new ErrorLog(errorLogPath);
                log.WriteError(ex.Message);
                Console.WriteLine(ex.Message + "{0} Press press any key to close.", Environment.NewLine);
                Console.ReadKey();
            }
            
        }
    }
}
