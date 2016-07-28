using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuakeActivityMonitor
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Press ESC to stop");
            do
            {
                while (!Console.KeyAvailable)
                {

                }
            } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
        }
    }
}
