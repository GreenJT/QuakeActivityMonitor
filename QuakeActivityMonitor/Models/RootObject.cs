using System.Collections.Generic;

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
