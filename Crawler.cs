﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using System.IO;
using System.Diagnostics;

namespace SearchBackend
{
    public class Crawler
    {
        private string _start;
        private Dictionary<string, int> _seenURLS;
        public Logger _logger;
        private SQLConnector _sc;
        private int _runtime;

        // StartURL specifies what site to start the crawl on
        // Username and password of the SQL database connection
        // Runtime specifies how many minutes until the program terminates, -1 for no termination
        public Crawler(string StartURL, string username, string password, int runtime)
        {
            _start = StartURL;
            _seenURLS = new Dictionary<string, int>();
            _logger = new Logger("crawler.txt", 1, false);
            _sc = new SQLConnector(String.Format("Server=tcp:o5vep5em15.database.windows.net,1433;Database=Search_Engine;User ID={0}@o5vep5em15;Password={1};Trusted_Connection=False;Encrypt=True;Connection Timeout=30;MultipleActiveResultSets=True;", username, password));
            _runtime = runtime;
        }

        // Starts at the starting URL and continues crawling links and updating database
        public void Crawl()
        {
            // Populate hash with urls from the database
            _seenURLS = _sc.GetVisited();

            Stopwatch sw = new Stopwatch();
            Queue<string> WebBFS = new Queue<string>();
            _logger.Initialize();
            WebBFS.Enqueue(_start);
            sw.Start();

            while (true)
            {
                // Exit after 30 minutes
                if (_runtime != -1 && sw.Elapsed.Minutes >= _runtime)
                    Environment.Exit(0);
                try
                {
                    string currentURL;

                    lock (WebBFS)
                    {
                        // Don't let queue grow indefinitely
                        currentURL = WebBFS.Dequeue();

                        if (WebBFS.Count >= ProducerBlock.high)
                        {
                            while (WebBFS.Count >= ProducerBlock.low)
                            {
                                WebBFS.Dequeue();
                            }
                        }
                    }

                    Console.WriteLine("Crawling " + currentURL);

                    WebRequest request = WebRequest.Create(currentURL);

                    IAsyncResult result = request.BeginGetResponse((IAsyncResult v) =>
                    {
                        try
                        {
                            WebResponse response = request.EndGetResponse(v);
                            Stream data = response.GetResponseStream();
                            string html = String.Empty;
                            Dictionary<string, bool> localSeen = new Dictionary<string, bool>();

                            using (StreamReader sr = new StreamReader(data))
                            {
                                html = sr.ReadToEnd();
                            }

                            html = Parser.SanitizeHtml(html);

                            IEnumerable<string> urls = Parser.GetURLS(html);

                            // Enqueue all the sites found from links
                            foreach (string s in urls)
                            {
                                string nextURL = s;

                                // If the link is relative add the current url
                                if (s.StartsWith("/"))
                                {
                                    nextURL = currentURL + s;
                                }
                                else if (!s.StartsWith("http"))
                                {
                                    nextURL = currentURL + "/" + s;
                                }
                                lock (WebBFS)
                                {
                                    // Don't allow a site to increase the page rank of a url more than once
                                    if (localSeen.ContainsKey(nextURL))
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        localSeen.Add(nextURL, true);
                                    }
                                    // Check if URL exists in our set
                                    if (_seenURLS.ContainsKey(nextURL))
                                    {
                                        _seenURLS[nextURL]++;
                                    }
                                    else
                                    {
                                        _seenURLS.Add(nextURL, 1);
                                        WebBFS.Enqueue(nextURL);
                                    }
                                }
                            }

                            // Parse content and write to SQL database
                            string content = Parser.GetContent(html);

                            Dictionary<string, int> keywords = Parser.GetKeywords(content);

                            // We are done with the large HTML string, have GC collect it
                            content = null;
                            html = null;

                            // No keywords in the case of an HTML overflow
                            if(keywords.Count > 0)
                                _sc.BulkInsert(currentURL, keywords, _seenURLS[currentURL], localSeen);
                        }
                        catch (WebException we)
                        {
                            // Log failure
                            if (_logger._on)
                            {
                                _logger.Log(DateTime.Now + " " + we.Message + " " + "(" + currentURL + ")" + Environment.NewLine);
                            }

                        }
                        catch (Exception we)
                        {
                            if (_logger._on)
                            {
                                _logger.Log(DateTime.Now + " " + we.Message + " " + "(" + currentURL + ")" + Environment.NewLine);
                            }
                            return;
                        }

                    }, null);
                }
                catch (System.InvalidOperationException we)
                {
                    if (_logger._on)
                    {
                        _logger.Log(DateTime.Now + " " + we.Message + Environment.NewLine);
                    }
                    continue;
                }
                catch (Exception we)
                {
                    if (_logger._on)
                    {
                        _logger.Log(DateTime.Now + " " + we.Message + Environment.NewLine);
                    }
                    continue;
                }
            }
        }
    }
}
