using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuakeActivityMonitor
{
    public class RootObject
    {
        public string Type { get; set; }
        public Metadata Metadata { get; set; }
        public List<Feature> Features { get; set; }
        public List<double> Bbox { get; set; }
    }
}
