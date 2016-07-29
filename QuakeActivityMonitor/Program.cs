using System;
using System.Configuration;

namespace QuakeActivityMonitor
{
    class Program
    {
        static void Main(string[] args)
        {
            DateTime dateTime = DateTime.UtcNow.AddHours(-1);
                        
            var baseAddress = ConfigurationManager.AppSettings["BaseAddress"].ToString();
            var method = ConfigurationManager.AppSettings["Method"].ToString();
            var format = ConfigurationManager.AppSettings["Format"].ToString();
            var firstParameter = ConfigurationManager.AppSettings["Parameter"].ToString();
            var orderby = ConfigurationManager.AppSettings["OrderBy"].ToString();

            firstParameter = firstParameter + dateTime.ToString("s");

            var parameter = method + format + firstParameter + orderby;

            APIRequestUtil request = new APIRequestUtil(baseAddress, parameter);
            var pastHourActivity = request.ConsumeAPI();
            
            for(int i = 0; i < pastHourActivity.Metadata.Count; i++)
            {
                var properties = pastHourActivity.Features[i].Properties;
                var geometry = pastHourActivity.Features[i].Geometry;
                var time = new DateTime(1970, 1, 1, 0, 0, 0).AddMilliseconds(Convert.ToInt64(properties.Time)).ToString();
                var magnitude = properties.Mag;
                var coordinates = geometry.Coordinates[0] + ", " + geometry.Coordinates[1];

                var output = $@"Date/Time: {time}{Environment.NewLine}Magnitude: {magnitude}{Environment.NewLine}Coordinates: {coordinates}";

                Console.WriteLine(output);
                Console.WriteLine();

            }
            Console.ReadLine();

            //Console.WriteLine("Press ESC to stop");
            //do
            //{
            //    while (!Console.KeyAvailable)
            //    {

            //    }
            //} while (Console.ReadKey(true).Key != ConsoleKey.Escape);
        }
    }
}
