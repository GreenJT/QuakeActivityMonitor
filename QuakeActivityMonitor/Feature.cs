using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuakeActivityMonitor
{
    public class Feature
    {
        public string Type { get; set; }
        public Properties Properties { get; set; }
        public Geometry Geometry { get; set; }
        public string Id { get; set; }
    }
}
