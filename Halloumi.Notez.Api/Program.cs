using Microsoft.Owin.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Halloumi.Notez.Api
{
    class Program
    {
        static void Main(string[] args)
        {
            const string url = "http://localhost:9000";

            using (WebApp.Start<Startup>(url))
            {
                Console.WriteLine("Server started at:" + url);
                Console.ReadLine();
            }

        }
    }
}
