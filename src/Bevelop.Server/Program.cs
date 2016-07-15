using System;
using Microsoft.Owin.Hosting;

namespace Bevelop.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            const string url = "http://localhost:8080";
            using (WebApp.Start(url))
            {
                Console.WriteLine("Server running on {0}", url);
                Console.ReadLine();
            }
        }
    }
}
