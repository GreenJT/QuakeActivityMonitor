using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using System.Diagnostics;

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

        public RootObject ConsumeAPI()
        {
            RunAsync().Wait();
            return JsonOutput;
        }

        private async Task RunAsync()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(BaseUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.GetAsync(Parameters);
                if (response.IsSuccessStatusCode)
                {
                    var jsonOutput = response.Content.ReadAsStringAsync().Result;
                    JsonOutput = JsonConvert.DeserializeObject<RootObject>(jsonOutput);
                }

            }
        }
    }
}
