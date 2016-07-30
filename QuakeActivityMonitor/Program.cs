using System;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.IO;
using CsvHelper;
using System.Linq;
using System.Device.Location;
using System.Collections.Generic;

namespace QuakeActivityMonitor
{
    class Program
    {
        public static void TestCSV()
        {
            string foo = "worldcities.csv";
            string filename = @"C:\Users\worldcities.csv";
            var file = System.IO.Path.GetDirectoryName(filename);

            // string filelocation = @"file:///E:\My%20Documents\Projects\Plethora%20Coding%20Challenge\worldcities.csv";
            var connString = string.Format(
                @"Provider=Microsoft.Jet.OleDb.4.0; Data Source={0};Extended Properties=""Text;HDR=YES;FMT=Delimited""",
                file);

            using (var conn = new OleDbConnection(connString))
            {
                conn.Open();
                var query = "SELECT id, ( 3959 * acos( cos( radians(37) ) * cos( radians( latitude ) ) " +
                    "* cos(radians(longitude) - radians(-122)) + sin(radians(37)) * sin(radians(latitude)) ) ) AS distance " +
                    "FROM [" + foo + "] " +
                    "HAVING distance < 25 " +
                    "ORDER BY distance ";// +
                                         //"LIMIT 20; ";
                //query = "SELECT * FROM [" + foo + "]";
                using (var adapter = new OleDbDataAdapter(query, conn))
                {
                    var ds = new DataSet("CSV File");
                    adapter.Fill(ds);
                }

            }
        

        }

        public static IList<string> TestCSVHelper()
        {
            var closestCities = new List<string>();

            try
            {
                using (var fileReader = File.OpenText(@"E:\My Documents\Projects\worldcities.csv"))
                using (var csvResult = new CsvReader(fileReader))
                {
                    csvResult.Configuration.RegisterClassMap<WorldCitiesClassMap>();

                    var records = csvResult.GetRecords<WorldCities>().ToList();
                    var quakePosition = new GeoCoordinate(35.6795006, -121.1183319);

                    var distanceList =
                        from cities in records
                        select new
                        {
                            cities.Name,
                            distances = quakePosition.GetDistanceTo(
                            new GeoCoordinate(cities.Latitude, cities.Longitude))
                        };

                    closestCities =
                        (from cities in distanceList
                         orderby cities.distances ascending
                         select cities.Name).Take(3).ToList<string>();
                                    
                }
            }
            catch(Exception e)
            {
                String msg = e.Message;
            }

            return closestCities;
        }

        private static List<string> FindClosestCities(IList<WorldCities> records, double longitude, double latitude)
        {
            var quakePosition = new GeoCoordinate(latitude, longitude);
            var nearestCities = new List<string>();
            var distanceList =
                from record in records
                select new
                {
                    record.Name,
                    distance = quakePosition.GetDistanceTo(new GeoCoordinate(record.Latitude, record.Longitude))
                };

            nearestCities =
                (from row in distanceList
                 orderby row.distance ascending
                 select row.Name).Take(3).ToList<string>();

            return nearestCities;
        }

        static void Main(string[] args)
        {
            //TestCSVHelper();
            //TestCSV();
            DateTime dateTime = DateTime.UtcNow.AddHours(-1);
                        
            var baseAddress = ConfigurationManager.AppSettings["BaseAddress"].ToString();
            var method = ConfigurationManager.AppSettings["Method"].ToString();
            var format = ConfigurationManager.AppSettings["Format"].ToString();
            var firstParameter = ConfigurationManager.AppSettings["Parameter"].ToString();
            var orderby = ConfigurationManager.AppSettings["OrderBy"].ToString();

            firstParameter = firstParameter + dateTime.ToString("s");

            var parameter = method + format + firstParameter + orderby;

            var readerUtil = new CsvReaderUtil<WorldCities, WorldCitiesClassMap>(@"E:\My Documents\Projects\worldcities.csv");
            var records = readerUtil.GetRecords();

            APIRequestUtil request = new APIRequestUtil(baseAddress, parameter);
            var pastHourActivity = request.ConsumeAPI();
            


            for (int i = 0; i < pastHourActivity.Metadata.Count; i++)
            {
                var properties = pastHourActivity.Features[i].Properties;
                var geometry = pastHourActivity.Features[i].Geometry;
                var magnitude = properties.Mag;

                var time = new DateTime(1970, 1, 1, 0, 0, 0).AddMilliseconds(Convert.ToInt64(properties.Time)).ToString();
                var coordinates = geometry.Coordinates[0] + ", " + geometry.Coordinates[1];
                List<string> nearestCities = FindClosestCities(records, geometry.Coordinates[0], geometry.Coordinates[1]);

                var output = $@"Date/Time: {time}{Environment.NewLine}Magnitude: {magnitude}{Environment.NewLine}Coordinates: {coordinates}";
                
                Console.WriteLine(output);
                Console.WriteLine("Cities nearest earthquake location: {0}, {1}, {2}", nearestCities[0], nearestCities[1], nearestCities[2]);
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
