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

        public CsvReaderUtil(string filename)
        {
            Filename = filename;  
        }

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
                string msg = ex.Message;
            }

            return records;
        }
    }
}
