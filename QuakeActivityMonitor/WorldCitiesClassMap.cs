using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace QuakeActivityMonitor
{
    public sealed class WorldCitiesClassMap : CsvClassMap<WorldCities>
    {
        public WorldCitiesClassMap()
        {
            Map(m => m.CountryCode).Index(0);
            Map(m => m.SubdivisionCode).Index(1);
            Map(m => m.GNSFD).Index(2);
            Map(m => m.GNSUFI).Index(3);
            Map(m => m.LanguageCode).Index(4);
            Map(m => m.Language).Index(5);
            Map(m => m.Name).Index(6);
            Map(m => m.Latitude).Index(7).Default(0);
            Map(m => m.Longitude).Index(8).Default(0);
        }
    }

    public class MyDoubleConverter : DefaultTypeConverter
    {
        public override object ConvertFromString(TypeConverterOptions options, string text)
        {
            Double output;
            if (Double.TryParse(text, out output))
            {
                return output;
            }
            else
            {
                return 0d;
            }
        }
    }
}
