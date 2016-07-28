using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuakeActivityMonitor
{
    public class Metadata
    {
        public long Generated { get; set; }
        public string Url { get; set; }
        public string Title { get; set; }
        public int Status { get; set; }
        public string Api { get; set; }
        public int Count { get; set; }
    }
}
