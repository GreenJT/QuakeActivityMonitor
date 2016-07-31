using System;
using System.IO;
using CsvHelper;
using System.Linq;
using System.Collections.Generic;
using CsvHelper.Configuration;

namespace QuakeActivityMonitor
{
    public class CsvReaderUtil<T, U> where U : CsvClassMap
    {
        private string Filename;

        /// <summary>
        /// Utility to map data from a CSV to a model
        /// </summary>
        /// <param name="filename">File location of CSV file.</param>
        public CsvReaderUtil(string filename)
        {
            Filename = filename;  
        }
        /// <summary>
        /// Maps CSV columns to the model then returns a list of the records.
        /// </summary>
        /// <returns>Returns list of CSV records</returns>
        public IList<T> GetRecords()
        {
            var records = new List<T>();
            try
            {
                using (var fileReader = File.OpenText(Filename))
                using (var csvResult = new CsvReader(fileReader))
                {
                    csvResult.Configuration.RegisterClassMap<U>();
                    records = csvResult.GetRecords<T>().ToList();
                }
            }
            catch (Exception ex)
            {
                string errorLogPath = AppDomain.CurrentDomain.BaseDirectory +
                    "ErrorLog " + DateTime.Now.Date.Month + "-" +
                    DateTime.Now.Date.Day + "-" + DateTime.Now.Date.Year + " " +
                    DateTime.Now.TimeOfDay.Hours + "H" + DateTime.Now.TimeOfDay.Minutes + "M" + ".txt";
                ErrorLog log = new ErrorLog(errorLogPath);
                log.WriteError(ex.Message);
            }

            return records;
        }
    }
}
