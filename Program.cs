using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace SearchBackend
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("Username: ");
            string username = Console.ReadLine();

            Console.WriteLine("Password: ");
            string password = Console.ReadLine();

            Crawler crawler = new Crawler("https://www.reddit.com", username, password);
            crawler.Crawl();
        }
    }
}
