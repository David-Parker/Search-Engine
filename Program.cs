﻿using System;
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
            Crawler crawler = new Crawler("http://yahoo.com" );
            crawler.Crawl();
        }
    }
}
