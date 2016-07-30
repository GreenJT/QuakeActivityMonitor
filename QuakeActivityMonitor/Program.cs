using System;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.IO;
using CsvHelper;
using System.Linq;
using System.Device.Location;
using System.Collections.Generic;
using System.Timers;

namespace QuakeActivityMonitor
{
    class Program
    {
        static readonly string BASE_ADDRESS = ConfigurationManager.AppSettings["BaseAddress"].ToString();
        static readonly string METHOD = ConfigurationManager.AppSettings["Method"].ToString();
        static readonly string FORMAT = ConfigurationManager.AppSettings["Format"].ToString();
        static readonly string PARAMETER = ConfigurationManager.AppSettings["Parameter"].ToString();
        static readonly string ORDERBY = ConfigurationManager.AppSettings["OrderBy"].ToString();
        static readonly string FILE_LOCATION = ConfigurationManager.AppSettings["FileLocation"].ToString();
        static readonly int TIME_INTERVAL = Convert.ToInt32(ConfigurationManager.AppSettings["Interval"]);

        public static void TestCSV()
        {
            string foo = "worldcities.csv";
            string filename = @"C:\Users\worldcities.csv";
            var file = System.IO.Path.GetDirectoryName(filename);

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

        /// <summary>
        /// Displays the earthquakes contained within the features parameter.
        /// </summary>
        /// <param name="features">List of Earthquakes and their associated data.</param>
        /// <param name="records">List of records from CSV file.</param>
        private static void EarthQuakeActivity(List<Feature> features, IList<WorldCities> records)
        {
            foreach (Feature feature in features)
            {
                var properties = feature.Properties;
                var geometry = feature.Geometry;
                var magnitude = properties.Mag;

                //gets readable date time from Epoch time
                var time = new DateTime(1970, 1, 1, 0, 0, 0).AddMilliseconds(Convert.ToInt64(properties.Time)).ToString();
                var coordinates = geometry.Coordinates[1] + ", " + geometry.Coordinates[0];
                var nearestCities = FindClosestCities(records, geometry.Coordinates[0], geometry.Coordinates[1]);
                var output = $@"Date/Time: {time}{Environment.NewLine}Magnitude: {magnitude}{Environment.NewLine}Coordinates: {coordinates}";

                Console.WriteLine(output);
                Console.WriteLine("Cities nearest earthquake location: {0}, {1}, {2}", nearestCities[0], nearestCities[1], nearestCities[2]);
                Console.WriteLine(Environment.NewLine);
            }
        }

        /// <summary>
        /// Sets a timer to continuously monitor for activity based on 
        /// the Time Interval.
        /// </summary>
        /// <param name="records"></param>
        public static void ContinuousMonitoring(IList<WorldCities> records)
        {
            Timer myTimer = new Timer();
            myTimer.Elapsed += delegate { Monitor(records); };
            myTimer.Interval = TIME_INTERVAL;
            myTimer.AutoReset = true;
            myTimer.Start();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="records">Record of world cities from CSV file.</param>
        private static void Monitor(IList<WorldCities> records)
        {
            var parameter = CreateUrlParameter(-TIME_INTERVAL);

            var activity = new APIRequestUtil(BASE_ADDRESS, parameter).ConsumeAPI();
            var features = activity.Features;
            if(features.Count > 0)
            {
                EarthQuakeActivity(features, records);
            }
            
        }

        /// <summary>
        /// Outputs the Url parameter based on the time interval in the app.config 
        /// file.
        /// </summary>
        /// <param name="timeInterval"></param>
        /// <returns></returns>
        private static string CreateUrlParameter(int timeInterval)
        {
            DateTime time = DateTime.UtcNow.AddMilliseconds(timeInterval);
            var timeParameter = PARAMETER + time.ToString("s");
            var parameter = METHOD + FORMAT + timeParameter + ORDERBY;

            return parameter;
        }

        static void Main(string[] args)
        {
            //create the url
            //DateTime dateTime = DateTime.UtcNow.AddHours(-1);
            //var startTime = PARAMETER + dateTime.ToString("s");
            //var parameter = METHOD + FORMAT + startTime + ORDERBY;

            //create url parameter for past hour
            var parameter = CreateUrlParameter(-60 * 60 * 1000);

            //get the records from the CSV
            var readerUtil = new CsvReaderUtil<WorldCities, WorldCitiesClassMap>(@FILE_LOCATION);
            var records = readerUtil.GetRecords();

            //get earthquake activity from the past hour
            APIRequestUtil request = new APIRequestUtil(BASE_ADDRESS, parameter);
            var activityPastHour = request.ConsumeAPI();

            //get features for past hour earthquake activity
            var features = activityPastHour.Features;
            Console.WriteLine("----- Earthquake activity in the past hour -----{0}", Environment.NewLine);

            EarthQuakeActivity(features, records);

            Console.WriteLine("----- Continued Monitoring -----");

            ContinuousMonitoring(records);

            Console.ReadKey();

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
