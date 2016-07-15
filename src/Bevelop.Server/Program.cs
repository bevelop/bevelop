using System;
using System.Linq;
using Microsoft.Owin.Hosting;

namespace Bevelop.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            var url = args.Any() ? args[0] : "http://localhost:8080";

            using (WebApp.Start(url))
            {
                Console.WriteLine("Server running on {0}", url);
                Console.ReadLine();
            }
        }
    }
}
