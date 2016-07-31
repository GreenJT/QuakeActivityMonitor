using System;
using System.Configuration;
using System.Data;
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

        /// <summary>
        /// Finds the 3 closest cities to the passed longitude and latitude.
        /// </summary>
        /// <param name="records"></param>
        /// <param name="longitude"></param>
        /// <param name="latitude"></param>
        /// <returns>String list of the 3 cities nearest the given lat and long parameters.</returns>
        private static List<string> FindClosestCities(IList<WorldCities> records, double longitude, double latitude)
        {
            var quakePosition = new GeoCoordinate(latitude, longitude);
            var nearestCities = new List<string>();

            try
            {
                //gets the distance between the given lat and long and those in the records
                var distanceList =
                from record in records
                select new
                {
                    record.Name,
                    distance = quakePosition.GetDistanceTo(new GeoCoordinate(record.Latitude, record.Longitude))
                };

                //takes the three records with the shortest from the given lat and long.
                nearestCities =
                    (from row in distanceList
                     orderby row.distance ascending
                     select row.Name).Take(3).ToList<string>();
            }
            catch(Exception ex)
            {
                string errorLogPath = AppDomain.CurrentDomain.BaseDirectory +
                    "ErrorLog " + DateTime.Now.Date.Month + "-" +
                    DateTime.Now.Date.Day + "-" + DateTime.Now.Date.Year + " " +
                    DateTime.Now.TimeOfDay.Hours + "H" + DateTime.Now.TimeOfDay.Minutes + "M" + ".txt";
                ErrorLog log = new ErrorLog(errorLogPath);
                log.WriteError(ex.Message);
            }
            

            return nearestCities;
        }

        /// <summary>
        /// Displays the earthquakes contained within the features parameter.
        /// </summary>
        /// <param name="features">List of Earthquakes and their associated data.</param>
        /// <param name="records">List of records from CSV file.</param>
        private static void EarthQuakeActivity(List<Feature> features, IList<WorldCities> records)
        {
            try
            {
                foreach (Feature feature in features)
                {
                    var properties = feature.Properties;
                    var geometry = feature.Geometry;
                    var magnitude = properties.Mag;

                    //gets readable date time from Epoch time
                    var time = new DateTime(1970, 1, 1, 0, 0, 0).AddMilliseconds(Convert.ToInt64(properties.Time)).ToLocalTime().ToString();
                    var coordinates = geometry.Coordinates[1] + ", " + geometry.Coordinates[0];
                    var nearestCities = FindClosestCities(records, geometry.Coordinates[0], geometry.Coordinates[1]);
                    var output = $@"Date/Time: {time}{Environment.NewLine}Magnitude: {magnitude}{Environment.NewLine}Coordinates: {coordinates}";

                    Console.WriteLine(output);
                    if(nearestCities != null && nearestCities.Count > 0)
                    {
                        Console.WriteLine("Cities nearest earthquake location: {0}, {1}, {2}", nearestCities[0], nearestCities[1], nearestCities[2]);
                    }
                    
                    Console.WriteLine(Environment.NewLine);
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
            }
            
        }

        /// <summary>
        /// Sets a timer to continuously monitor for activity based on 
        /// the Time Interval.
        /// </summary>
        /// <param name="records"></param>
        public static void ContinuousMonitoring(IList<WorldCities> records)
        {
            try
            {
                Timer myTimer = new Timer();
                myTimer.Elapsed += delegate { Monitor(records); };
                myTimer.Interval = TIME_INTERVAL;
                myTimer.AutoReset = true;
                myTimer.Start();
            }
            catch(Exception ex)
            {
                string errorLogPath = AppDomain.CurrentDomain.BaseDirectory +
                    "ErrorLog " + DateTime.Now.Date.Month + "-" +
                    DateTime.Now.Date.Day + "-" + DateTime.Now.Date.Year + " " +
                    DateTime.Now.TimeOfDay.Hours + "H" + DateTime.Now.TimeOfDay.Minutes + "M" + ".txt";
                ErrorLog log = new ErrorLog(errorLogPath);
                log.WriteError(ex.Message);
            }
        }

        /// <summary>
        /// Continuously Monitors for earthquakes
        /// </summary>
        /// <param name="records">Record of world cities from CSV file.</param>
        private static void Monitor(IList<WorldCities> records)
        {
            try
            {
                var parameter = CreateUrlParameter(-TIME_INTERVAL);
                Console.WriteLine("{0} - Checking for additional seismic activity...{1}{1}", DateTime.Now.ToLocalTime().ToString(), Environment.NewLine);
                var activity = new APIRequestUtil(BASE_ADDRESS, parameter).ConsumeAPI();
                var features = activity.Features;
                if (features.Count > 0)
                {
                    EarthQuakeActivity(features, records);
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
            var parameter = "";
            try
            {
                DateTime time = DateTime.UtcNow.AddMilliseconds(timeInterval);
                var timeParameter = PARAMETER + time.ToString("s");
                parameter = METHOD + FORMAT + timeParameter + ORDERBY;
            }
            catch(Exception ex)
            {
                string errorLogPath = AppDomain.CurrentDomain.BaseDirectory +
                    "ErrorLog " + DateTime.Now.Date.Month + "-" +
                    DateTime.Now.Date.Day + "-" + DateTime.Now.Date.Year + " " +
                    DateTime.Now.TimeOfDay.Hours + "H" + DateTime.Now.TimeOfDay.Minutes + "M" + ".txt";
                ErrorLog log = new ErrorLog(errorLogPath);
                log.WriteError(ex.Message);
            }
            

            return parameter;
        }

        static void Main(string[] args)
        {
            try
            {
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
                Console.WriteLine("{0}{0}-------------- Earthquake activity in the past hour --------------{0}", Environment.NewLine);

                //outputting earthquake activity in the past hour
                EarthQuakeActivity(features, records);

                //obscure Simpsons refrence
                Console.WriteLine("---------- Monitoring for further seismic activity every {0} seconds----------{1}{1}Press the any key to end continuous monitoring", (TIME_INTERVAL / 1000), Environment.NewLine);

                //begin continuous monitoring 
                ContinuousMonitoring(records);
                Console.ReadKey();
            }
            catch(Exception ex)
            {
                string errorLogPath = AppDomain.CurrentDomain.BaseDirectory +
                     "ErrorLog " + DateTime.Now.Date.Month + "-" +
                     DateTime.Now.Date.Day + "-" + DateTime.Now.Date.Year + " " +
                     DateTime.Now.TimeOfDay.Hours + "H" + DateTime.Now.TimeOfDay.Minutes + "M" + ".txt";
                ErrorLog log = new ErrorLog(errorLogPath);
                log.WriteError(ex.Message);
            }
        }
    }
}
